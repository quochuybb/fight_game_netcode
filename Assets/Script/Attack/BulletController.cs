using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private BulletManager bulletManager;
    [SerializeField] public BulletNetworkSerializable bulletConfig;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float currentDuration;
    private Vector2 direction;
    private bool isShoot;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (!isShoot)
        {
            return;
        }
        currentDuration += Time.deltaTime;
        if (currentDuration > bulletConfig.timeExist)
        {
            DestroyBullet(transform.position, false);
            currentDuration = 0;
        }
        _rigidbody2D.velocity = direction * bulletConfig.speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DestroyBullet(transform.position, true);
    }

    public void InitConfigBullet(BulletNetworkSerializable bulletNetwork, Vector2 direction)
    {
        this.bulletManager = BulletManager.instance;
        this.bulletConfig = bulletNetwork;
        this.direction = direction;
        UpdateSpriteBullet();
        currentDuration = 0f;
        isShoot = true;

    }

    public void UpdateSpriteBullet()
    {
        transform.localScale = Vector3.one * bulletConfig.size;
        spriteRenderer.color = bulletConfig.colorBullet;
    }
    
    public void DestroyBullet(Vector3 pos, bool animate)
    {
        if (animate)
        {
            bulletManager.CreateEffectDestroyBulletClientRpc(pos, bulletConfig);
        }
        bulletManager.RequestDestroyFromBullet(this.NetworkObject);
    }

    public float GetDamage()
    {
        return bulletConfig.damage;
    }
}
