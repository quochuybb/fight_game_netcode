// Assets/Scripts/Connection/ConnectionManager.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;


[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
public class ConnectionManager : MonoBehaviour
{
    [Header("References")]
    public NetworkManager networkManager;
    public UnityTransport unityTransport;
    public LobbyInfo lobbyInfo;
    public string relayCode;
    public int maxPlayers = 4;
    private UnityAuthService unityAuthService;
    private UnityLobbyService unityLobbyService;
    private UnityRelayService unityRelayService;
    private TransportConfigurator transportConfigurator;
    private CancellationTokenSource Cts;
    private ILobbyEvents _lobbyEvents;
    private bool initialized;

    private void Start()
    {
        transportConfigurator = new TransportConfigurator(unityTransport);
        unityAuthService = new UnityAuthService();
        unityLobbyService = new UnityLobbyService();
        unityRelayService = new UnityRelayService();
        Cts = new CancellationTokenSource();
    }

    private async Task SubscribeLobbyEventsAsync(string lobbyId)
    {
        var callBacksLobby = new LobbyEventCallbacks();
        callBacksLobby.LobbyChanged += OnChangedLobby;
        callBacksLobby.KickedFromLobby += OnKickedMember;
        try
        {
            _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callBacksLobby);
            Debug.Log("Subscribed to lobby events");

        }
        catch (Exception e)
        { 
            Debug.LogError("Subscribe Lobby Events Fail: " + e.Message);
            throw;
        }
    }

    private void OnChangedLobby(ILobbyChanges changes)
    {
        Debug.Log("OnChangedLobby");
        OnLobbyUpdated?.Invoke(lobbyInfo);
    }
    private void OnKickedMember()
    {
        OnLobbyUpdated?.Invoke(lobbyInfo);
    }

    
    public event Action<LobbyInfo> OnLobbyUpdated;

    public async Task PrepareLobbyAsync()
    {
        if (initialized) return;
        try
        {
            Cts.Token.ThrowIfCancellationRequested();
            var playerData = await unityAuthService.SignInAnonymouslyAsync(Cts.Token); 
            Debug.Log("Creating lobby...");
            lobbyInfo = await unityLobbyService.CreateLobby(playerData.playerId, maxPlayers,Cts.Token);
            Debug.Log($"Lobby created. id={lobbyInfo.lobbyId}, lobbyJoinCode (share to players)={lobbyInfo.joinCode}");
            initialized = true;
            OnLobbyUpdated?.Invoke(lobbyInfo);
            await SubscribeLobbyEventsAsync(lobbyInfo.lobbyId);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[ConnectionManager] StartHostAsync canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConnectionManager] StartHostAsync failed: {ex}");
            try { await StopHostAsync(); } catch { }
            throw;
        }
    }
    public async Task StartGameNetworkAsync(bool localHostPlays = true, CancellationToken externalToken = default)
    {
        if (lobbyInfo == null)
            throw new InvalidOperationException("Lobby not prepared. Call PrepareLobbyAsync() first.");
        var localCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        var ct = localCts.Token;

        try
        {
            ct.ThrowIfCancellationRequested();
            Debug.Log("Creating Relay allocation...");
            var allocation = await unityRelayService.CreateAllocation(Math.Max(1, maxPlayers - 1), ct);

            var joinCode = await unityRelayService.GetJoinCodeAllocation(allocation.AllocationId, ct);
            relayCode = joinCode;
            Debug.Log($"Relay allocation created. joinCode={joinCode}");

            var updateData = new Dictionary<string, DataObject>()
            {
                { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { "hostId", new DataObject(DataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId) }
            };

            await unityLobbyService.UpdateLobby(lobbyInfo.lobbyId, updateData, ct);

            lobbyInfo.Metadata["relayJoinCode"] = joinCode;
            OnLobbyUpdated?.Invoke(lobbyInfo);

            await transportConfigurator.ApplyHostAllocationAsync(allocation, ct);

            if (networkManager == null) networkManager = GetComponent<NetworkManager>();

            if (localHostPlays)
            {
                Debug.Log("Starting Host (local plays)...");
                networkManager.StartHost();
            }
            else
            {
                Debug.Log("Starting Server (no local player)...");
                networkManager.StartServer();
            }

            initialized = true;
            OnLobbyUpdated?.Invoke(lobbyInfo);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[ConnectionManager] StartGameNetworkAsync canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConnectionManager] StartGameNetworkAsync failed: {ex}");
            try { await StopHostAsync(); } catch { }
            throw;
        }
        finally
        {
            localCts.Dispose();
        }
    }

    public async Task JoinLobbyAsync(string lobbyId, CancellationToken externalToken = default)
    {
        if (initialized) return;

        try
        {
            var localCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            var ct = localCts.Token;
            var playerData = await unityAuthService.SignInAnonymouslyAsync(ct); 
            Debug.Log("Join lobby...");
            lobbyInfo = await unityLobbyService.JoinLobbyByJoinCode(lobbyId, ct);
            Debug.Log($"Join lobby. id={lobbyInfo.lobbyId}, lobbyJoinCode (share to players)={lobbyInfo.joinCode}");
            initialized = true;
            OnLobbyUpdated?.Invoke(lobbyInfo);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[ConnectionManager] JoinLobby canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConnectionManager] JoinLobby failed: {ex}");
            try { await StopHostAsync(); } catch { }
            throw;
        }
    }

    public async Task StopHostAsync()
    {
        try
        {
            if (Cts != null)
            {
                Cts.Cancel();
                Cts.Dispose();
                Cts = null;
            }
        }
        catch { }
        try
        {
            if (networkManager != null && (networkManager.IsHost || networkManager.IsServer))
            {
                networkManager.Shutdown();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error shutting down network: {ex.Message}");
        }

        try
        {
            if (lobbyInfo != null && lobbyInfo.lobbyId == AuthenticationService.Instance.PlayerId)
            {
                await unityLobbyService.LeaveLobby(lobbyInfo.lobbyId, CancellationToken.None);
                Debug.Log($"Deleted lobby {lobbyInfo.lobbyId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DeleteLobby failed: {ex.Message}");
        }

        initialized = false;
        lobbyInfo = null;
        relayCode = null;
        OnLobbyUpdated?.Invoke(null);
    }

    
}
