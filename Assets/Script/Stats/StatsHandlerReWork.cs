using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandlerReWork : NetworkBehaviour
{
    public static StatsHandlerReWork Instance;
    [SerializeField] public CharacterStats stats;
    [SerializeField] public Bullet statsAttack;
    [SerializeField] public ParticleSystem dieEffect;
    private CharacterController _characterController;
    public NetworkVariable<CharacterStatsNetwork> currentStats =
        new NetworkVariable<CharacterStatsNetwork>(
            writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<BulletNetworkSerializable> currentAttackStats =
        new NetworkVariable<BulletNetworkSerializable>(
            writePerm: NetworkVariableWritePermission.Server);  
    public NetworkVariable<Vector2> networkPosition;
    
    [Header("UI (Owner Only)")]
    [SerializeField] private GameObject healthUI;
    [SerializeField] private Slider healthSlider;
    public override void OnNetworkSpawn()
    {
        _characterController = GetComponent<CharacterController>();
        _characterController.OnDamgeEvent.AddListener(OnLocalDamaged);
        _characterController.OnBuffEvent.AddListener(OnLocalBuff);
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
    private void OnStatsChanged(CharacterStatsNetwork oldValue, CharacterStatsNetwork newValue)
    {

    }
    
    private void OnStatsAttackChanged(BulletNetworkSerializable oldValue, BulletNetworkSerializable newValue)
    {

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
                RunDieEffectClientRpc(p.Receive.SenderClientId);
                stats.healthPoint = this.stats.Mapping().healthPoint + 5*(this.stats.Mapping().alive - stats.alive + 1);
                UpdateHealthSliderClientRpc(delta, stats.healthPoint);
                stats.alive -= 1;
                stats.speedMove += 2;
                attackStats.damage += 2;
            }
            UpdateHealthSliderClientRpc(delta, stats.healthPoint);



        }
        currentStats.Value = stats;
        currentAttackStats.Value = attackStats;
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
