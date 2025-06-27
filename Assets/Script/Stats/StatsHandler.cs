using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandler : NetworkBehaviour
{
    public static StatsHandler Instance;
    [SerializeField] public CharacterStats stats;
    [SerializeField] public Bullet statsAttack;
    [SerializeField] public ParticleSystem dieEffect;
    private CharacterController _characterController;
    public NetworkVariable<CharacterStatsNetwork> currentStatsNetworkVariableClient = new NetworkVariable<CharacterStatsNetwork>(new CharacterStatsNetwork());
    public NetworkVariable<BulletNetworkSerializable> currentStatsAttackNetworkVariableClient = new NetworkVariable<BulletNetworkSerializable>(new BulletNetworkSerializable());    
    public NetworkVariable<CharacterStatsNetwork> currentStatsNetworkVariableHost = new NetworkVariable<CharacterStatsNetwork>(new CharacterStatsNetwork());
    public NetworkVariable<BulletNetworkSerializable> currentStatsAttackNetworkVariableHost = new NetworkVariable<BulletNetworkSerializable>(new BulletNetworkSerializable());
    private CharacterStatsNetwork CurrentClient = new CharacterStatsNetwork();
    private BulletNetworkSerializable CurrentAttackClient = new BulletNetworkSerializable();
    private CharacterStatsNetwork CurrentHost = new CharacterStatsNetwork();
    private BulletNetworkSerializable CurrentAttackHost = new BulletNetworkSerializable();
    public NetworkVariable<Vector2> networkPosition;
    private const float BuffUpdateInterval = 0.1f;
    private float _lastBuffUpdateTime ;
    private float lastNetworkUpdate;
    [SerializeField] private GameObject healthSlider;
    
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _characterController.OnDamgeEvent.AddListener(Injured);
        _characterController.OnBuffEvent.AddListener(BuffStats);

    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentStatsNetworkVariableHost.Value = stats.Mapping();
            currentStatsNetworkVariableClient.Value = stats.Mapping();
            CurrentHost= stats.Mapping();
            CurrentClient = stats.Mapping();
            currentStatsAttackNetworkVariableClient.Value = statsAttack.Mapping();
            currentStatsAttackNetworkVariableHost.Value = statsAttack.Mapping();
            CurrentAttackClient= statsAttack.Mapping();
            CurrentAttackHost = statsAttack.Mapping();
            healthSlider.GetComponent<Slider>().maxValue = CurrentHost.healthPoint;
        }
        else
        {
            healthSlider.GetComponent<Slider>().maxValue = CurrentClient.healthPoint;
        }
    }

    private void Start()
    {
        Instance = this;
        currentStatsNetworkVariableClient.OnValueChanged += OnStatsClientChanged;
        currentStatsNetworkVariableHost.OnValueChanged += OnStatsHostChanged;
        currentStatsAttackNetworkVariableClient.OnValueChanged += OnStatsAttackClientChanged;
        currentStatsAttackNetworkVariableHost.OnValueChanged += OnStatsAttackHostChanged;


    }
    private void OnStatsHostChanged(CharacterStatsNetwork previous, CharacterStatsNetwork current)
    {
        CurrentHost = current;
    }
    private void OnStatsClientChanged(CharacterStatsNetwork previous, CharacterStatsNetwork current)
    {
        CurrentClient = current;        
    }
    private void OnStatsAttackHostChanged(BulletNetworkSerializable previous, BulletNetworkSerializable current)
    {
        CurrentAttackHost = current;
    }
    private void OnStatsAttackClientChanged(BulletNetworkSerializable previous, BulletNetworkSerializable current)
    {
        CurrentAttackClient = current;
    }

    public void Injured(float damage)
    {
        if (IsOwner)
        {
            ChangeHealthServerRpc(damage);
        }
        else
        {
            ChangeHealthClient(damage);

        }
    }
    private void ChangeHealthClient(float damage)
    {
        if (currentStatsNetworkVariableClient.Value.armor > 0)
        {
            currentStatsNetworkVariableClient.Value.armor -= damage;
            if (currentStatsNetworkVariableClient.Value.armor <= 0)
            {
                currentStatsNetworkVariableClient.Value.armor = 0;
            }
        }
        else
        {
            currentStatsNetworkVariableClient.Value.healthPoint -= damage;
            healthSlider.GetComponent<Slider>().value -= damage;

        }        
        if (currentStatsNetworkVariableClient.Value.healthPoint <= 0)
        {
            currentStatsNetworkVariableClient.Value.alive -= 1;
            currentStatsNetworkVariableClient.Value.healthPoint = stats.Mapping().healthPoint + 5;
            currentStatsNetworkVariableClient.Value.armor += 2;
            currentStatsAttackNetworkVariableClient.Value.damage += 2;
            currentStatsAttackNetworkVariableClient.Value.speed += 3;
            RunDieEffectClientRpc();
            if (currentStatsNetworkVariableClient.Value.alive <= 0)
            {
                NetworkManager.Singleton.Shutdown();

            }
        }
    }
    



    [ServerRpc]
    public void ChangeHealthServerRpc(float damage)
    {
        if (currentStatsNetworkVariableHost.Value.armor > 0)
        {
            currentStatsNetworkVariableHost.Value.armor -= damage;
            if (currentStatsNetworkVariableHost.Value.armor <= 0)
            {
                currentStatsNetworkVariableHost.Value.armor = 0;
            }
        }
        else
        {
            currentStatsNetworkVariableHost.Value.healthPoint -= damage;
            healthSlider.GetComponent<Slider>().value -= damage;

        }
        if (currentStatsNetworkVariableHost.Value.healthPoint <= 0)
        {
            currentStatsNetworkVariableHost.Value.alive -= 1;
            currentStatsNetworkVariableHost.Value.healthPoint = stats.Mapping().healthPoint + 5;
            currentStatsNetworkVariableHost.Value.armor += 2;
            currentStatsAttackNetworkVariableHost.Value.damage += 2;
            currentStatsAttackNetworkVariableHost.Value.speed += 3;
            RunDieEffectClientRpc();
            if (currentStatsNetworkVariableHost.Value.alive <= 0)
            {
                NetworkManager.Singleton.Shutdown();

            }
        }
    }

    [ClientRpc]
    public void RunDieEffectClientRpc()
    {
        dieEffect.Stop();
        dieEffect.Play();
    }
    
    public void BuffStats(ItemNetworkSerializable item, ServerRpcParams rpcParams = default)
    {
        if (IsOwner)
        {
            HandleOwnerBuff(item);
        }
        else
        {
            if (item.typeBuff == 0)
            {
                HandleClientBuff();

            }
            else
            {
                HandleClientBuffAttack();
            }
        }
    }
    private void HandleOwnerBuff(ItemNetworkSerializable item)
    {
        if (Time.time - _lastBuffUpdateTime >= BuffUpdateInterval)
        {
            _lastBuffUpdateTime = Time.time;
            if (IsOwner)
            {
                UpdateBuffServerRpc(item);
            }
        }
    }


    private void HandleClientBuffAttack()
    {
        CurrentAttackClient= currentStatsAttackNetworkVariableClient.Value;
    }
    private void HandleClientBuff()
    {
        CurrentClient= currentStatsNetworkVariableClient.Value;

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
    [ServerRpc]
    private void UpdateBuffServerRpc(ItemNetworkSerializable item, ServerRpcParams serverRpcParams = default)
    {
        
        if (serverRpcParams.Receive.SenderClientId == 0)
        {

            if (item.typeBuff == 1)
            {
                SetFieldByName(currentStatsAttackNetworkVariableHost.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(CurrentAttackHost, item.nameStatsBuff, item.statsBuff);
            }
            else
            {
                SetFieldByName(currentStatsNetworkVariableHost.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(CurrentHost, item.nameStatsBuff, item.statsBuff);
            }
        }
        else
        {
            if (item.typeBuff == 1)
            {
                SetFieldByName(currentStatsAttackNetworkVariableClient.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(CurrentAttackClient, item.nameStatsBuff, item.statsBuff);

                
            }
            else
            {
                SetFieldByName(currentStatsNetworkVariableClient.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(CurrentClient, item.nameStatsBuff, item.statsBuff);
            }

        }

    }

}
