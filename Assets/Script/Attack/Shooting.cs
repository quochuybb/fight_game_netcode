using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    [SerializeField] private Transform firePoint;
    private CharacterController controller;
    private Vector2 aimDirection = Vector2.right;
    private BulletManager bulletManager;
    private BulletController bulletController;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        bulletManager = BulletManager.instance; 
        controller.onAttackGunEvent.AddListener(OnShooting);
        controller.onLookEvent.AddListener(OnLookMouse);
    }

    private void OnShooting(Bullet bulletConfig)
    {
        float HalfOfSumAngleBullet = -(bulletConfig.numberOfBulletsPerShoot/2f) * bulletConfig.multipleBulletAngle + 0.5f * bulletConfig.multipleBulletAngle;
        for (int i = 0; i < bulletConfig.numberOfBulletsPerShoot; i++)
        {
            float angle = HalfOfSumAngleBullet + i * bulletConfig.multipleBulletAngle;
            CreateBullet(bulletConfig, angle);
        }
    }

    private void OnLookMouse(Vector2 aimDirection)
    {
        this.aimDirection = aimDirection;
    }

    private void CreateBullet(Bullet bulletConfig, float angle)
    {
        bulletManager.ShootBullet(firePoint.position, RotateDirection(aimDirection,angle), bulletConfig);
    }

    private Vector2 RotateDirection(Vector2 aimDirection, float angle)
    {
        return Quaternion.Euler(0,0,angle) * aimDirection;
    }
    
    
}
