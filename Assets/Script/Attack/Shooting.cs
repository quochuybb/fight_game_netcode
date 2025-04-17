using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    [SerializeField] private Transform firePoint;
    private CharacterController controller;
    private NetworkVariable<Vector2> networkAimDirection = new NetworkVariable<Vector2>( Vector2.right, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);

    [SerializeField] private Bullet bulletConfig;
    [SerializeField] private StatsHandler statsHandler;
    private BulletNetworkSerializable bulletNetworkSerializable = new BulletNetworkSerializable();
    private BulletManager bulletManager;
    private float lastTimeShoot;
    private bool canShoot;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bulletManager = BulletManager.instance;
        canShoot = false;
    }
    
    public override void OnNetworkSpawn()
    {
        bulletNetworkSerializable = bulletConfig.Mapping();
        if (IsOwner)
        {
            controller.onAttackGunEvent.AddListener(OnShooting);
            controller.onLookEvent.AddListener(OnLookMouse);
        }
        
    }

    private void Update()
    {
        HandleDelayTime();
    }

    private void OnShooting()
    {
        if (!canShoot)
        {
            return;
        }
        canShoot = false;
        float HalfOfSumAngleBullet = -(bulletNetworkSerializable.numberOfBulletsPerShoot/2f) * bulletNetworkSerializable.multipleBulletAngle + 0.5f * bulletNetworkSerializable.multipleBulletAngle;
        for (int i = 0; i < bulletNetworkSerializable.numberOfBulletsPerShoot; i++)
        {
            float angle = HalfOfSumAngleBullet + i * bulletNetworkSerializable.multipleBulletAngle;
            CreateBullet(bulletNetworkSerializable, angle);
        }

    }

    private void OnLookMouse(Vector2 aimDirection)
    {
        this.networkAimDirection.Value = aimDirection;
    }

    private void CreateBullet(BulletNetworkSerializable bulletNetwork, float angle)
    {
        bulletManager.ShootBulletServerRpc(firePoint.position, transform.rotation, bulletNetwork,RotateDirection(networkAimDirection.Value, angle));
    }

    private Vector2 RotateDirection(Vector2 aimDirection, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * aimDirection;    
    }
    public void HandleDelayTime()
    {
        lastTimeShoot += Time.deltaTime;
        if (lastTimeShoot > bulletNetworkSerializable.delay)
        {
            lastTimeShoot = 0f;
            canShoot=true;
        }
        
    }
    
}
