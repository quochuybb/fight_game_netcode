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
    private CharacterController characterController;
    public CharacterStatsNetwork currentStatsHost { get; private set; }
    public CharacterStatsNetwork currentStatsClient { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentStatsHost = stats.MappingToStruct();
        currentStatsClient = stats.MappingToStruct();
        characterController.onDamgeEvent.AddListener(ChangedHealthServerRpc);
        characterController.onCleanEvent.AddListener(Death);
        characterController.onBuffEvent.AddListener(BuffStatsServerRpc);
    }

    private void Start()
    {
        Instance = this;
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangedHealthServerRpc(float damage)
    {
        currentStatsHost.healthPoint -= damage;
        if (currentStatsHost.healthPoint <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BuffStatsServerRpc(ItemNetworkSerializable item,bool isHost, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (senderId == NetworkManager.Singleton.LocalClientId && isHost)
        {
            currentStatsHost.damagePercentage += 3;
            var hostRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { senderId } }
                
            };
            UpdateClientBuffClientRpc(currentStatsHost.damagePercentage, true, hostRpcParams);
            Debug.LogError("Server buff updated: Server " + currentStatsHost.damagePercentage);
            Debug.LogError("Server buff updated: Client " + currentStatsClient.damagePercentage);

        }
        else
        {
            currentStatsClient.damagePercentage += 3;
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { senderId } }
            };
            UpdateClientBuffClientRpc(currentStatsClient.damagePercentage, false, clientRpcParams);
            Debug.LogError("Client buff updated: Server " + currentStatsHost.damagePercentage);
            Debug.LogError("Client buff updated: Client " + currentStatsClient.damagePercentage);
        }
    }

    [ClientRpc]
    public void UpdateClientBuffClientRpc(float buff,bool isHost, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.IsServer && isHost)
        {
            return;
        }
        if (isHost) 
        {
            currentStatsHost.damagePercentage = buff;
        }
        else
        {
            currentStatsClient.damagePercentage = buff;
        }
        Debug.LogError("IsHost " + isHost);
        Debug.LogError("UpdateClientBuffClientRpc buff updated: Host " + currentStatsHost.damagePercentage);
        Debug.LogError("UpdateClientBuffClientRpc buff updated: Client " + currentStatsClient.damagePercentage);
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
