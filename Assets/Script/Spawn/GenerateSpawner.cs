using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSpawner : MonoBehaviour
{
    [SerializeField] private GameObject BaseTele;
    [SerializeField] private GameObject Player;

    private void Awake()
    {
        Instantiate(BaseTele, new Vector3(0,0,0), Quaternion.identity);
        //Instantiate(Player, new Vector3(1,0,0), Quaternion.identity);

    }
}
