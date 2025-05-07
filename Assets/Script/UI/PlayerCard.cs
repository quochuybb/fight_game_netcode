using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCard : NetworkBehaviour
{
    [SerializeField] private NetworkObject prefabToSpawn;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; 
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnectedServerRpc;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;    
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnectedServerRpc;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Player")
        {

            if (!IsServer) return;

            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);


        }
    }
    
    private void SpawnPlayerForClient(ulong clientId)
    {
        var instance = Instantiate(prefabToSpawn);
        instance.SpawnWithOwnership(clientId);                       
    }
    [ServerRpc]
    private void HandleClientConnectedServerRpc(ulong clientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                continue; 
            SpawnPlayerForClient(client.ClientId);
        }    
    }
}
