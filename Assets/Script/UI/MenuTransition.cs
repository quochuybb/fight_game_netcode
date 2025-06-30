using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MenuTransition : NetworkBehaviour
{
    public static MenuTransition instance;
    [SerializeField] private RectTransform Online;
    [SerializeField] private RectTransform MainMenu;
    [SerializeField] private RectTransform Options;
    [SerializeField] private RectTransform Setting;
    private UnityEvent OnOpenSettings = new UnityEvent();
    public UnityEvent onOpenSettings => OnOpenSettings;

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        onOpenSettings.AddListener(OpenPanelSettings);


    }

    public void OpenOnlineScreen()
    {
        MovePanelToOpenOnlineScreen();
    }

    private void MovePanelToOpenOnlineScreen(ClientRpcParams rpcParams = default)
    {
        MainMenu.DOAnchorPos(new Vector2(0,-2000), 1.5f, false);
        Online.DOAnchorPos(new Vector2(0,0), 1.5f, false);
    }
    
    public void BackToMainMenuFromOnlineScreen()
    {
        MovePanelOnlineBackMainMenu();
    }
    private void MovePanelOnlineBackMainMenu(ClientRpcParams rpcParams = default)
    {
        MainMenu.DOAnchorPos(new Vector2(0,0), 1.5f, false);
        Online.DOAnchorPos(new Vector2(2000,0), 1.5f, false);
    }
    
    public void OpenOptionsScreen()
    {
        MovePanelToOpenOptionsScreen();
    }
    private void MovePanelToOpenOptionsScreen(ClientRpcParams rpcParams = default)
    {
        MainMenu.DOAnchorPos(new Vector2(0,-2000), 1.5f, false);
        Options.DOAnchorPos(new Vector2(0,0), 1.5f, false);
    }
    
    public void BackToMainMenuFromOptionsScreen()
    {
        MovePanelOptionsBackMainMenu();
    }
    private void MovePanelOptionsBackMainMenu(ClientRpcParams rpcParams = default)
    {
        MainMenu.DOAnchorPos(new Vector2(0,0), 1.5f, false);
        Options.DOAnchorPos(new Vector2(2000,0), 1.5f, false);
    }
    public void JoinGame()
    {
        MovePannelJoinGame();
    }
    private void MovePannelJoinGame(ClientRpcParams rpcParams = default)
    {
        Online.DOAnchorPos(new Vector2(20000,0), 1.5f, false);
    }


    
    
    public void QuitGameButtonPressed()
    {
        MovePanelBackServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void MovePanelBackServerRpc(ServerRpcParams rpcParams = default)
    {
        MovePanelBackClientRpc();
    }

    [ClientRpc]
    private void MovePanelBackClientRpc(ClientRpcParams rpcParams = default)
    {
        NetworkManager.Singleton.Shutdown();
        Online.DOAnchorPos(new Vector2(0,0), 1, false);
        Setting.gameObject.SetActive(false);
    }

    private void OpenPanelSettings()
    {
        Setting.gameObject.SetActive(true);

    }
    public void ClosePanelSettings()
    {
        Setting.gameObject.SetActive(false);

    }
    
}
