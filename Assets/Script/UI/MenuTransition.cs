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
    [SerializeField] private RectTransform Setting;
    private UnityEvent OnOpenSettings = new UnityEvent();
    public UnityEvent onOpenSettings => OnOpenSettings;

    private void Awake()
    {
        instance = this;
        onOpenSettings.AddListener(OpenSettingButtonPressed);
    }

    public void OnlineButtonPressed()
    {
        MovePanelServerRpc();
    }
    public void JoinButtonPressed()
    {
        MovePanelServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void MovePanelServerRpc(ServerRpcParams rpcParams = default)
    {
        MovePanelClientRpc();
    }

    [ClientRpc]
    private void MovePanelClientRpc(ClientRpcParams rpcParams = default)
    {
        Online.DOAnchorPos(new Vector2(0,2000), 1, false);

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
    public void OpenSettingButtonPressed()
    {
        OpenPanelSettingsServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void OpenPanelSettingsServerRpc(ServerRpcParams rpcParams = default)
    {
        OpenPanelSettingsClientRpc();
    }

    [ClientRpc] private void OpenPanelSettingsClientRpc(ClientRpcParams rpcParams = default)
    {
        if (Setting.gameObject.activeInHierarchy)
        {
            Setting.gameObject.SetActive(false);

        }
        else
        {
            Setting.gameObject.SetActive(true);
        }
    }
}
