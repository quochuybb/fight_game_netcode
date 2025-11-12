using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI refs")]
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button startGameClientButton;
    [SerializeField] private TMP_InputField inputField;       
    [SerializeField] private TextMeshProUGUI idLobby;         
    [SerializeField] private TextMeshProUGUI playersListText; 
    [SerializeField] private TextMeshProUGUI error;
    [SerializeField] private ConnectionManager connectionManager;

    private CancellationTokenSource _uiPollCts;

    private void Awake()
    {
        instance = this;
        Cursor.visible = true;
    }

    private void Start()
    {
        if (connectionManager == null)
        {
            Debug.LogError("[UIManager] ConnectionManager not found in scene!");
            error.text = "Missing ConnectionManager";
            DisableAllButtons();
            return;
        }

        startHostButton.onClick.AddListener(OnHostClicked);
        startClientButton.onClick.AddListener(OnJoinClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        startGameClientButton.onClick.AddListener(OnStartGameClientClicked);

        idLobby.text = "";
        playersListText.text = "";
        error.text = "";
        startGameButton.interactable = false;
        startGameClientButton.interactable = false;

        connectionManager.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDestroy()
    {
        connectionManager.OnLobbyUpdated -= OnLobbyUpdated;
        StopUiPolling();
    }

    private async void OnHostClicked()
    {
        SetInteractable(false);
        error.text = "";

        try
        {
            await connectionManager.PrepareLobbyAsync();

            if (connectionManager.LobbyInfo != null)
            {
                idLobby.text = connectionManager.LobbyInfo.joinCode ?? "";
                playersListText.text = FormatPlayers(connectionManager.LobbyInfo);

                startGameButton.interactable = true;

                StartUiPolling();

                startClientButton.interactable = false;
                startHostButton.interactable = false;
            }
            else
            {
                throw new Exception("Failed to create lobby (no lobby info).");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[UI] Host prepare canceled.");
            error.text = "Canceled";
            SetInteractable(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnHostClicked failed: {ex}");
            error.text = "Failed to create lobby: " + ex.Message;
            SetInteractable(true);
        }
    }


    private async void OnJoinClicked()
    {
        SetInteractable(false);
        error.text = "";

        try
        {
            if (inputField.text == null)
            {
                error.text = "Fill lobby code";
                SetInteractable(true);
            }
            await connectionManager.JoinLobbyAsync(inputField.text);

            if (connectionManager.LobbyInfo != null)
            {
                idLobby.text = connectionManager.LobbyInfo.joinCode ?? "";
                playersListText.text = FormatPlayers(connectionManager.LobbyInfo);

                startGameClientButton.interactable = true;

                StartUiPolling();

                startClientButton.interactable = false;
                startHostButton.interactable = false;
            }
            else
            {
                throw new Exception("Failed to join lobby (no lobby info).");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[UI] Join lobby canceled.");
            error.text = "Canceled";
            SetInteractable(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnJoinClicked failed: {ex}");
            error.text = "Failed to join lobby: " + ex.Message;
            SetInteractable(true);
        }
    }

    private async void OnStartGameClicked()
    {
        startGameButton.interactable = false;
        error.text = "";

        try
        {
            await connectionManager.StartGameNetworkAsync(localHostPlays: true);

            Debug.Log("Game network started (host).");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[UI] StartGame canceled.");
            error.text = "Canceled";
            startGameButton.interactable = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnStartGameClicked failed: {ex}");
            error.text = "Failed to start game: " + ex.Message;
            startGameButton.interactable = true;
        }
    }
    private async void OnStartGameClientClicked()
    {
        startGameClientButton.interactable = false;
        error.text = "";

        try
        {
            await connectionManager.JoinGameNetworkAsync();

            Debug.Log("Game network started (client).");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[UI] StartGame canceled.");
            error.text = "Canceled";
            startGameClientButton.interactable = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnStartGameClientClicked failed: {ex}");
            error.text = "Failed to start game: " + ex.Message;
            startGameClientButton.interactable = true;
        }
    }


    private void OnLobbyUpdated(LobbyInfo info)
    {
        if (info == null)
        {
            idLobby.text = "";
            playersListText.text = "(0 players)";
            return;
        }

        idLobby.text = info.joinCode ?? "";
        playersListText.text = FormatPlayers(info);
        startGameButton.interactable = !string.IsNullOrEmpty(idLobby.text);
        startGameClientButton.interactable = !string.IsNullOrEmpty(idLobby.text);

    }

    private void StartUiPolling()
    {
        StopUiPolling();
        _uiPollCts = new CancellationTokenSource();
        _ = UiPollLoopAsync(_uiPollCts.Token);
    }

    private void StopUiPolling()
    {
        if (_uiPollCts != null)
        {
            _uiPollCts.Cancel();
            _uiPollCts.Dispose();
            _uiPollCts = null;
        }
    }

    private async Task UiPollLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var lobby = connectionManager.LobbyInfo;
                    if (lobby != null)
                    {
                        idLobby.text = lobby.joinCode ?? "";
                        playersListText.text = FormatPlayers(lobby);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("UiPollLoop error: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0), ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    private string FormatPlayers(LobbyInfo lobby)
    {
        if (lobby == null || lobby.Players == null || lobby.Players.Count == 0) return "(0 players)";
        return string.Join("\n", lobby.Players.Select(p => p.DisplayName ?? p.Id));
    }

    private void SetInteractable(bool enabled)
    {
        startHostButton.interactable = enabled;
        startClientButton.interactable = enabled;
        startGameButton.interactable = enabled && !string.IsNullOrEmpty(idLobby.text);
        startGameClientButton.interactable = enabled && !string.IsNullOrEmpty(idLobby.text);
        inputField.interactable = enabled;
    }

    private void DisableAllButtons()
    {
        startHostButton.interactable = false;
        startClientButton.interactable = false;
        startGameButton.interactable = false;
        startGameClientButton.interactable = false;
        inputField.interactable = false;
    }
}
