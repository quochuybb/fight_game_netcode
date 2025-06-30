
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayersManager : Singletons<PlayersManager>
{
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
    public UnityEvent startSpawnItems = new UnityEvent();
    
    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        } 
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (IsServer)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Client {id} connected");

                }
                playersInGame.Value++;
                if (playersInGame.Value >= 2)
                {
                    if (IsOwner)
                    {
                        startSpawnItems.Invoke();

                    }
                }
            }
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (IsServer)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Client {id} disconnected");
                }
                playersInGame.Value--;
            }
        };
    }
}
