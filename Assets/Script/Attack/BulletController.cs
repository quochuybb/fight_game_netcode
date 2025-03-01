using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private BulletManager bulletManager;
    [SerializeField] private Bullet bulletConfig;
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
        if (currentDuration > bulletConfig.duration)
        {
            DestroyBullet(transform.position, true);
        }
        _rigidbody2D.velocity = direction * bulletConfig.speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) != 0) 
        {
            DestroyBullet(other.transform.position, true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            DestroyBullet(other.transform.position, true);
        } else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            DestroyBullet(gameObject.transform.position, true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            DestroyBullet(gameObject.transform.position, true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Village"))
        {
            DestroyBullet(gameObject.transform.position, true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Stained"))
        {
            DestroyBullet(gameObject.transform.position, true);
        }
    }

    public void InitConfigBullet(Bullet bulletConfig, Vector2 direction, BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
        this.bulletConfig = bulletConfig;
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
            bulletManager.CreateEffectDestroyBullet(pos,bulletConfig);
        }
        gameObject.SetActive(false);
    }

    public float GetDamage()
    {
        return bulletConfig.damage;
    }
}
