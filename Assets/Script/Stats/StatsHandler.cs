using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterState
{
    Alive,
    Dead
}
public class StatsHandler : NetworkBehaviour
{
    public static StatsHandler Instance;
    [Header("Base Stats")]
    [SerializeField] public CharacterStats stats;
    [SerializeField] public Bullet statsAttack;
    [Header("Effects & Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] public ParticleSystem dieEffect;
    private CharacterController _characterController;
    [Header("Networked Stats")]
    public NetworkVariable<CharacterStatsNetwork> currentStats =
        new NetworkVariable<CharacterStatsNetwork>(
            writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<BulletNetworkSerializable> currentAttackStats =
        new NetworkVariable<BulletNetworkSerializable>(
            writePerm: NetworkVariableWritePermission.Server);  
    public NetworkVariable<CharacterState> State =
        new NetworkVariable<CharacterState>(CharacterState.Alive,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    [SerializeField] private float respawnTime = 15f;
    [Header("Ghost Effect")]
    [SerializeField] private SpriteRenderer[] playerSprites; 
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.5f);
    private Collider2D _collider2D;

    [Header("UI (Owner Only)")]
    [SerializeField] private GameObject healthUI;
    [SerializeField] private Slider healthSlider;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        _characterController = GetComponent<CharacterController>();
        _characterController.OnDamgeEvent.AddListener(OnLocalDamaged);
        _characterController.OnBuffEvent.AddListener(OnLocalBuff);
        State.OnValueChanged += HandleStateChanged;

        if (IsServer)
        {
            currentStats.Value = stats.Mapping();
            currentAttackStats.Value = statsAttack.Mapping();
        }

        currentStats.OnValueChanged += OnStatsChanged;
        currentAttackStats.OnValueChanged += OnStatsAttackChanged;

        if (healthUI != null)
            healthUI.SetActive(IsOwner);
        
        if (IsOwner && healthSlider != null)
        {
            healthSlider.maxValue = stats.Mapping().healthPoint;
            healthSlider.value = currentStats.Value.healthPoint;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (_characterController != null)
        {
            _characterController.OnDamgeEvent.RemoveListener(OnLocalDamaged);
            _characterController.OnBuffEvent.RemoveListener(OnLocalBuff);
        }
        if (State != null)
        {
            State.OnValueChanged -= HandleStateChanged;
        }
    }
    private void OnStatsChanged(CharacterStatsNetwork oldValue, CharacterStatsNetwork newValue)
    {

    }
    
    private void OnStatsAttackChanged(BulletNetworkSerializable oldValue, BulletNetworkSerializable newValue)
    {
        
    }
    private void HandleStateChanged(CharacterState previousState, CharacterState newState)
    {
        bool isDead = newState == CharacterState.Dead;

        foreach (var sprite in playerSprites)
        {
            if (sprite != null)
                sprite.color = isDead ? ghostColor : Color.white;
        }

        if (_collider2D != null)
            _collider2D.enabled = !isDead;
        if (animator != null)
            animator.SetBool("Death", isDead);
        
    }
    private void OnLocalDamaged(float dmg)
    {
        ApplyDamageServerRpc(dmg);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ApplyDamageServerRpc(
         float delta, ServerRpcParams p = default)
    {
        var stats = currentStats.Value;
        var attackStats = currentAttackStats.Value;

        if (stats.armor > 0)
        {
            stats.armor = Mathf.Max(0, stats.armor - delta);
        }
        else
        {
            stats.healthPoint = Mathf.Max(0, stats.healthPoint - delta);
            if (stats.healthPoint == 0)
            {
                State.Value = CharacterState.Dead;
                RunDieEffectClientRpc(p.Receive.SenderClientId);
                stats.healthPoint = this.stats.Mapping().healthPoint + 5*(this.stats.Mapping().alive - stats.alive + 1);
                UpdateHealthSliderClientRpc(delta, stats.healthPoint);
                stats.alive -= 1;
                UpdatePointUIClientRpc(stats.alive,p.Receive.SenderClientId);
                if (stats.alive == 0)
                {
                    EndGameClientRpc(stats.alive);
                    return;
                }
                stats.speedMove += 2;
                attackStats.damage += 2;
                StartCoroutine(RespawnTimerCoroutine()); 
            }
            UpdateHealthSliderClientRpc(delta, stats.healthPoint);



        }
        currentStats.Value = stats;
        currentAttackStats.Value = attackStats;
    }
    private IEnumerator RespawnTimerCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        State.Value = CharacterState.Alive;
    }
    [ClientRpc]
    public void EndGameClientRpc(
        float alive, ClientRpcParams rpc = default)
    {
        CameraFollow.instance.OnResetCamera();
        MenuTransition.instance.ShowPanelEndGame(alive);
    }
    [ClientRpc]
    private void UpdateHealthSliderClientRpc(
        float delta, float newMaxHP,ClientRpcParams rpc = default)
    {
        if (IsOwner)
        {
            if (healthSlider.value <= 0)
            {
                healthSlider.maxValue = newMaxHP;
                healthSlider.value = healthSlider.maxValue;
                return;
            }
            healthSlider.value -= delta;

        }
    }
    
    [ClientRpc]
    public void UpdatePointUIClientRpc(
        float alive,ulong target, ClientRpcParams rpc = default)
    {

        if (!IsOwner)
        {
            //UIManager.instance.ShowPoint(alive);
        }
    }
    [ClientRpc]
    private void RunDieEffectClientRpc(
        ulong targetClient, ClientRpcParams rpc = default)
    {
        dieEffect.Stop();
        dieEffect.Play();
    }
    private void OnLocalBuff(ItemNetworkSerializable item, ServerRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        ApplyBuffServerRpc(item);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ApplyBuffServerRpc(
        ItemNetworkSerializable item, ServerRpcParams rpcParams = default)
    {
        var stats = currentStats.Value;
        var attackStats = currentAttackStats.Value;
        if (item.typeBuff == 0)
            SetFieldByName(stats, item.nameStatsBuff, item.statsBuff);
        else
            SetFieldByName(attackStats, item.nameStatsBuff, item.statsBuff);
        
        currentStats.Value = stats;
        currentAttackStats.Value = attackStats;
    }
    public void SetFieldByName(object obj, string targetFieldName, float newValue)
    {
        if (obj == null || string.IsNullOrEmpty(targetFieldName))
            return;

        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {

            if (field.Name.Equals(targetFieldName, StringComparison.Ordinal))
            {
                
                object valueObj = field.GetValue(obj);
                if (valueObj is float currentValue)
                {
                    float value = currentValue + newValue;
                    field.SetValue(obj, value);
                }
            }
        }
        
    }

}
