
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : Singletons<PlayersManager>
{
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

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
