using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton; 
    [SerializeField] private TextMeshProUGUI playerInGameText;
    [SerializeField] private GameObject mainmenu;
    [SerializeField] private GameObject optionmenu;

    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        playerInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }

    private void Start()
    {
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
            }
            else
            {
                Debug.Log("Host failed to start");
            }
        }); 
        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
            }
            else
            {
                Debug.Log("Client failed to start");
            }
        }); 
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started");
            }
            else
            {
                Debug.Log("Server failed to start");
            }
        }); 
    }
    public void showoptionsmenu()
    {
        if (mainmenu.activeSelf)
        {
            mainmenu.SetActive(false);
            optionmenu.SetActive(true);
        }
        else
        {
            mainmenu.SetActive(true);
            optionmenu.SetActive(false);
        }
    }

}
