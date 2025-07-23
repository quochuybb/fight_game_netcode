using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        joinCode = $"{host[0]}.{host[1]}.{host[2]}.{host[3]}";
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
            if (!IPAddress.TryParse(inputField.text, out var ipAddress))
            {
                error.text = "Please enter a valid IPv4 or IPv6 address.";
                return;
            }
            try
            {
                Debug.LogError(ipAddress.ToString());
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
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()        
            .Where(nic =>
                nic.Description.ToLower().Contains("tailscale"))
            .ToList();

        
        foreach (var nic in interfaces)
        {
            foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
            {
                if (unicast.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(unicast.Address) &&
                    unicast.Address.ToString().StartsWith("100."))
                {
                    return unicast.Address.ToString();
                }
            }
        }
        return null;
    }
    public void ShowPoint(float alive)
    {
        pointUI[4 - (int)alive].SetActive(true);
    }


}
