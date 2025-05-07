using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton; 
    [SerializeField] private TextMeshProUGUI idLobby;
    [SerializeField] private UnityTransport _transport;
    [SerializeField] private string joinCode;
    [SerializeField] private TMP_InputField inputField;

    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        idLobby.text = joinCode;


    }

    private void Start()
    {
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
                string ip = _transport.ConnectionData.Address;
                ushort port = _transport.ConnectionData.Port;
                joinCode = $"{ip}:{port}";
                Debug.Log($"Lobby code: {joinCode}");
            }
            else
            {
                Debug.Log("Host failed to start");
            }
        }); 
        startClientButton.onClick.AddListener(() =>
        {
            joinCode = inputField.text;
            var parts = joinCode.Split(':');
            string ip = parts[0];
            ushort port = ushort.Parse(parts[1]);
            _transport.SetConnectionData(ip, port, null);                 
            NetworkManager.Singleton.StartClient();    
        }); 

    }

}
