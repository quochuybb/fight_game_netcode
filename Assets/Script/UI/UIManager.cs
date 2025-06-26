using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton; 
    [SerializeField] private TextMeshProUGUI idLobby;
    [SerializeField] private UnityTransport _transport;
    [SerializeField] private string joinCode;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI error;



    
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
        var host = Dns.GetHostEntry(Dns.GetHostName()); // List IP connect Router
        joinCode = host.AddressList[0].ToString(); // IP private PC User
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost()) // Start Host in PC user
            {
                
            }
            else
            {
                Debug.Log("Host failed to start");
            }
        }); 
        startClientButton.onClick.AddListener(() =>
        {
            if (!IPAddress.TryParse(inputField.text, out var ipAddress))
            {
                error.text = "Invalid IP address format. Please enter a valid IPv4 or IPv6 address.";
                return;
            }
            try
            {
                _transport.SetConnectionData(ipAddress.ToString(), 7777, null);                 

                if (!NetworkManager.Singleton.StartClient())
                {
                    error.text = "Network is not ready or already connected.";
                }
            }
            catch (InvalidOperationException)
            {
                error.text = "Network is not ready or already connected.";
            }
            catch (SocketException)
            {
                error.text = "Unable to open network connection.";
            }
            catch (Exception ex)
            {
                error.text = $"Unexpected error: {ex.Message}";
            }
        }); 

    }

}
