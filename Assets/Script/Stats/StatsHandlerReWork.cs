using System;
using System.Collections;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandlerReWork : NetworkBehaviour
{
    [Header("Base Stats")]
    [SerializeField] public CharacterStats stats;
    [SerializeField] public Bullet statsAttack;

    [Header("Effects & Animator")]
    [SerializeField] public ParticleSystem dieEffect;
    [SerializeField] private Animator animator;

    [Header("State Management")]
    [SerializeField] private float respawnTime = 5f;
    public NetworkVariable<CharacterState> State = new NetworkVariable<CharacterState>(CharacterState.Alive, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Ghost Effect")]
    [SerializeField] private SpriteRenderer[] playerSprites; 
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Networked Stats")]
    public NetworkVariable<CharacterStatsNetwork> currentStats = new NetworkVariable<CharacterStatsNetwork>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<BulletNetworkSerializable> currentAttackStats = new NetworkVariable<BulletNetworkSerializable>(writePerm: NetworkVariableWritePermission.Server);

    [Header("UI (Owner Only)")]
    [SerializeField] private GameObject healthUI;
    [SerializeField] private Slider healthSlider;
    private DeathScreenUI deathScreenUI;

    private CharacterController _characterController;

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
            State.Value = CharacterState.Alive;
        }

        if (IsOwner)
        {
            GameObject deathScreenObject = GameObject.FindGameObjectWithTag("DeathScreenUI");
            if (deathScreenObject != null)
            {
                deathScreenUI = deathScreenObject.GetComponent<DeathScreenUI>();
            }

            if (healthUI != null)
                healthUI.SetActive(true);

            if (healthSlider != null)
            {
                healthSlider.maxValue = stats.Mapping().healthPoint;
                healthSlider.value = currentStats.Value.healthPoint;
            }
        }

        HandleStateChanged(State.Value, State.Value);
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

    private void HandleStateChanged(CharacterState previousState, CharacterState newState)
    {
        bool isDead = newState == CharacterState.Dead;

        foreach (var sprite in playerSprites)
        {
            if (sprite != null)
                sprite.color = isDead ? ghostColor : Color.white;
        }

        // Turn off physics and animations for clients
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = !isDead;
        if (animator != null)
            animator.SetBool("Death", isDead);

        if (IsOwner)
        {
            if (deathScreenUI != null)
            {
                if (isDead)
                    deathScreenUI.Show(respawnTime);
                else
                    deathScreenUI.Hide();
            }
            if (healthUI != null)
            {
                healthUI.SetActive(!isDead);
            }
        }
    }

    private void OnLocalDamaged(float dmg)
    {
        ApplyDamageServerRpc(dmg);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyDamageServerRpc(float delta, ServerRpcParams p = default)
    {
        //make player not take dam when die
        if (State.Value == CharacterState.Dead) return;

        var stats = currentStats.Value;
        stats.healthPoint = Mathf.Max(0, stats.healthPoint - delta);
        currentStats.Value = stats;

        UpdateHealthSliderClientRpc(stats.healthPoint);

        if (stats.healthPoint == 0)
        {
            //player died
            State.Value = CharacterState.Dead;
            if (dieEffect != null) dieEffect.Play();
            StartCoroutine(RespawnTimerCoroutine()); 
        }
    }

    private IEnumerator RespawnTimerCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        // respawn logic
        var stats = currentStats.Value;
        stats.healthPoint = this.stats.Mapping().healthPoint;
        currentStats.Value = stats;

        State.Value = CharacterState.Alive;
    }

    [ClientRpc]
    private void UpdateHealthSliderClientRpc(float newHP, ClientRpcParams rpc = default)
    {
        if (IsOwner && healthSlider != null)
        {
            healthSlider.maxValue = stats.Mapping().healthPoint;
            healthSlider.value = newHP;
        }
    }

    private void OnLocalBuff(ItemNetworkSerializable item, ServerRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        ApplyBuffServerRpc(item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyBuffServerRpc(ItemNetworkSerializable item, ServerRpcParams rpcParams = default)
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
        if (obj == null || string.IsNullOrEmpty(targetFieldName)) return;
        Type type = obj.GetType();
        FieldInfo field = type.GetField(targetFieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(float))
        {
            float currentValue = (float)field.GetValue(obj);
            field.SetValue(obj, currentValue + newValue);
        }
    }
}