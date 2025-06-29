using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : NetworkBehaviour
{
    public static HealthBarUI instance;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private StatsHandler statsHandler;
    [SerializeField] private CharacterController characterController;

    private void Awake()
    {
        instance = this;
        characterController.OnDamgeEvent.AddListener(UpdateUIClientRpc);

    }

    private void Start()
    {

        healthSlider.maxValue = statsHandler.currentStatsNetworkVariableHost.Value.healthPoint;
        healthSlider.value = statsHandler.currentStatsNetworkVariableHost.Value.healthPoint;


    }

    
    [ClientRpc]
    public void UpdateUIClientRpc(float value)
    {
        

    }

    

}
