using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
public class UIManager : NetworkBehaviour
{
    public static UIManager instance;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton; 
    [SerializeField] private TextMeshProUGUI idLobby;
    [SerializeField] private UnityTransport _transport;
    [SerializeField] private string joinCode;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI error;
    [SerializeField] private MenuTransition menuTransition;
    [SerializeField] private List<GameObject> pointUI;


    private void Awake()
    {

        instance = this; 
        menuTransition = GetComponent<MenuTransition>();
        Cursor.visible = true;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        foreach (var point in pointUI)
        {
            point.SetActive(false);
        }
    }

    private void Update()
    {
        idLobby.text = joinCode;
    }

    private void Start()
    {
        var host = GetLocalIP().ToString().Split('.');
        var net = $"{host[0]}.{host[1]}.{host[2]}.";
        joinCode = host[^1];
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost()) 
            {
                
            }
            else
            {
                Debug.Log("Host failed to start");
            }
        }); 
        startClientButton.onClick.AddListener(() =>
        {
            if (inputField.text == null)
            {
                error.text = $"Null input field";
                return;
            }
            if (!IPAddress.TryParse(net + inputField.text, out var ipAddress))
            {
                error.text = "Please enter a valid IPv4 or IPv6 address.";
                return;
            }
            try
            {
                menuTransition.JoinGame();
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
    private static string GetLocalIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ip))
                return ip.ToString();
        }

        return null;
    }
    public void ShowPoint(float alive)
    {
        pointUI[4 - (int)alive].SetActive(true);
    }


}
