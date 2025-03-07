using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = System.Random;

public class StatsHandler : NetworkBehaviour
{

    [SerializeField] public CharacterStats stats;
    private CharacterController characterController;
    [SerializeField] public Bullet bullet;
    public CharacterStats CurrentHealth { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        CurrentHealth = stats.Clone();
        characterController.onDamgeEvent.AddListener(ChangedHealth);
        characterController.onCleanEvent.AddListener(Death);
    }

    public void ChangedHealth(float damage)
    {

        CurrentHealth.maxHealth -= damage;
        if (CurrentHealth.maxHealth <= 0)
        {

            characterController.isClean = true;
        }
    }

    public void ChangedDamage(float damage)
    {
        bullet.damage += damage;
    }

    public void ChangedNumberBullet(int amount)
    {
        bullet.numberOfBulletsPerShoot += amount + 1;
    }

    public void Death()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Time.timeScale = 0;        
        }
        
    }

}
