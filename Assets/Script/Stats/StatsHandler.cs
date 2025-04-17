using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class StatsHandler : NetworkBehaviour
{
    public static StatsHandler Instance;
    [SerializeField] public CharacterStats stats;
    [SerializeField] public Bullet statsAttack;
    private CharacterController characterController;
    public NetworkVariable<CharacterStatsNetwork> currentStatsNetworkVariableClient = new NetworkVariable<CharacterStatsNetwork>(new CharacterStatsNetwork(), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    public NetworkVariable<BulletNetworkSerializable> currentStatsAttackNetworkVariableClient = new NetworkVariable<BulletNetworkSerializable>(new BulletNetworkSerializable(), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);    
    public NetworkVariable<CharacterStatsNetwork> currentStatsNetworkVariableHost = new NetworkVariable<CharacterStatsNetwork>(new CharacterStatsNetwork(), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    public NetworkVariable<BulletNetworkSerializable> currentStatsAttackNetworkVariableHost = new NetworkVariable<BulletNetworkSerializable>(new BulletNetworkSerializable(), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    public CharacterStatsNetwork currentClient = new CharacterStatsNetwork();
    public BulletNetworkSerializable currentAttackClient = new BulletNetworkSerializable();
    public CharacterStatsNetwork currentHost = new CharacterStatsNetwork();
    public BulletNetworkSerializable currentAttackHost = new BulletNetworkSerializable();

    private const float BUFF_UPDATE_INTERVAL = 0.1f;
    private float lastBuffUpdateTime = 0f;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterController.onDamgeEvent.AddListener(ChangedHealthServerRpc);
        characterController.onCleanEvent.AddListener(Death);
        characterController.onBuffEvent.AddListener(BuffStats);

    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentStatsNetworkVariableHost.Value = stats.Mapping();
            currentStatsNetworkVariableClient.Value = stats.Mapping();
            currentHost= stats.Mapping();
            currentClient = stats.Mapping();
            currentStatsAttackNetworkVariableClient.Value = statsAttack.Mapping();
            currentStatsAttackNetworkVariableHost.Value = statsAttack.Mapping();
            currentAttackClient= statsAttack.Mapping();
            currentAttackHost = statsAttack.Mapping();
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
        currentHost = current;
    }
    private void OnStatsClientChanged(CharacterStatsNetwork previous, CharacterStatsNetwork current)
    {
        currentClient = current;
    }
    private void OnStatsAttackHostChanged(BulletNetworkSerializable previous, BulletNetworkSerializable current)
    {
        currentAttackHost = current;
    }
    private void OnStatsAttackClientChanged(BulletNetworkSerializable previous, BulletNetworkSerializable current)
    {
        currentAttackClient = current;
    }


    [ServerRpc(RequireOwnership = false)]
    public void ChangedHealthServerRpc(float damage)
    {
        currentStatsNetworkVariableClient.Value.healthPoint -= damage;
        if (currentStatsNetworkVariableClient.Value.healthPoint <= 0)
        {
            this.gameObject.SetActive(false);
        }
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
        if (Time.time - lastBuffUpdateTime >= BUFF_UPDATE_INTERVAL)
        {
            lastBuffUpdateTime = Time.time;
            if (IsOwner)
            {
                UpdateBuffServerRpc(item);
            }
        }
    }


    private void HandleClientBuffAttack()
    {
        currentAttackClient= currentStatsAttackNetworkVariableClient.Value;

    }
    private void HandleClientBuff()
    {
        currentClient= currentStatsNetworkVariableClient.Value;

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
                SetFieldByName(currentAttackHost, item.nameStatsBuff, item.statsBuff);
                Debug.LogError(currentStatsAttackNetworkVariableHost.Value.numberOfBulletsPerShoot);
                
            }
            else
            {
                SetFieldByName(currentStatsNetworkVariableHost.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(currentHost, item.nameStatsBuff, item.statsBuff);
            }
            //UpdateBuffClientRpc(newBuff);
        }
        else
        {
            if (item.typeBuff == 1)
            {
                SetFieldByName(currentStatsAttackNetworkVariableClient.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(currentAttackClient, item.nameStatsBuff, item.statsBuff);
                
            }
            else
            {
                SetFieldByName(currentStatsNetworkVariableClient.Value, item.nameStatsBuff, item.statsBuff);
                SetFieldByName(currentClient, item.nameStatsBuff, item.statsBuff);
            }
            Debug.LogError(currentStatsAttackNetworkVariableClient.Value.numberOfBulletsPerShoot);

        }

    }
    [ClientRpc]
    private void UpdateBuffClientRpc(CharacterStatsNetwork newBuff)
    {

        currentHost = newBuff;
        
    } 
    
    public void ChangedNumberBullet(int amount)
    {
        //bullet.numberOfBulletsPerShoot += amount + 1;
    }

    public void Death()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Time.timeScale = 0;        
        }
    }

}
