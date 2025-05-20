using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ChestController : NetworkBehaviour
{
    private UnityEvent<Transform> OnOpenChest =  new UnityEvent<Transform>();
    public UnityEvent<Transform> onOpenChest => OnOpenChest;
    [SerializeField] private float chestOpenTime;
    private float timer = 0;    
    

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > this.chestOpenTime)
        {
            timer = 0f;
            onOpenChest.Invoke(gameObject.transform);
        }
    }
}
