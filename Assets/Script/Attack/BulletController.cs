using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private BulletManager bulletManager;
    private NetworkVariable<BulletNetworkSerializable> bulletConfigNetworkVariable = new NetworkVariable<BulletNetworkSerializable>();
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float currentDuration;
    private Vector2 direction;
    private bool isShoot;
    private Vector2 lastPosition;
    
    private CircleCollider2D col2D;
    private ContactFilter2D filter;
    private List<RaycastHit2D> results = new List<RaycastHit2D>();

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    public override void OnNetworkSpawn()
    {
        col2D = GetComponent<CircleCollider2D>();
        
        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Wall"));
        filter.useTriggers = true;   
        bulletConfigNetworkVariable.OnValueChanged += OnBulletConfigChanged;
        base.OnNetworkSpawn();
    }
    public override void OnNetworkDespawn()
    {
        bulletConfigNetworkVariable.OnValueChanged -= OnBulletConfigChanged;
        base.OnNetworkDespawn();
    }
    private void OnBulletConfigChanged(BulletNetworkSerializable previousValue, BulletNetworkSerializable newValue)
    {
        if (previousValue.bouncing != newValue.bouncing)
        {
            Debug.Log($"Client: Số lần nảy thay đổi từ {previousValue.bouncing} thành {newValue.bouncing}");
        }
        
        if (previousValue.size != newValue.size)
        {
            UpdateSpriteBullet();
        }
    }

    private void Update()
    {
        if (!isShoot) return;

        if (IsServer) 
        {
            currentDuration += Time.deltaTime;
            if (currentDuration > bulletConfigNetworkVariable.Value.timeExist)
            {
                DestroyBulletServerRpc();
                currentDuration = 0;
            }
        }
        
        _rigidbody2D.velocity = direction * bulletConfigNetworkVariable.Value.speed;
        lastPosition = _rigidbody2D.position;


    }
    [ServerRpc]
    private void BouncesChangedServerRpc()
    {
        BulletNetworkSerializable currentConfig = bulletConfigNetworkVariable.Value;

        currentConfig.bouncing--;
        bulletConfigNetworkVariable.Value = currentConfig;    
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.gameObject.CompareTag("Bullet"))
        {

            if (bulletConfigNetworkVariable.Value.bouncing> 0)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    DestroyBulletServerRpc();
                    
                }

                if (!other.CompareTag("Wall")) return;


                Vector2 dir = _rigidbody2D.velocity.normalized;
                float dist = _rigidbody2D.velocity.magnitude * Time.fixedDeltaTime;


                results.Clear();
                int hitCount = col2D.Cast(dir, filter, results, dist);


                if (hitCount > 0)
                {
                    RaycastHit2D hit = results[0];
                    Vector2 normal = hit.normal;             
                    Vector2 incoming = _rigidbody2D.velocity.normalized;
                    Vector2 reflectDir = Vector2.Reflect(incoming, normal);
                    BulletNetworkSerializable currentConfig = bulletConfigNetworkVariable.Value;
                    currentConfig.bouncing--;
                    bulletConfigNetworkVariable.Value = currentConfig;
                    if (incoming == Vector2.zero)
                    {
                        DestroyBulletServerRpc();
                        return;
                    }


                    transform.position = hit.centroid;
                    direction = reflectDir;
                }
                else
                {
      
                    Vector2 incoming = _rigidbody2D.velocity.normalized;
                    Vector2 fallbackDir = new Vector2(-incoming.y, incoming.x);
                    direction = fallbackDir;

                }

            }
            else
            {
                DestroyBulletServerRpc();
            }


        }
    }

    
    
    public void InitConfigBullet(BulletNetworkSerializable bulletNetwork, Vector2 direction)
    {
        if (IsServer)
        {
            this.bulletConfigNetworkVariable.Value = bulletNetwork;
        }
        this.bulletManager = BulletManager.instance;
        this.direction = direction;
        if (IsServer)
        {
            UpdateSpriteBullet();
            isShoot = true;
        }
        currentDuration = 0f;

    }
    
    public void UpdateSpriteBullet()
    {
        transform.localScale = Vector3.one * bulletConfigNetworkVariable.Value.size;
    }
    
    [ServerRpc]
    public void DestroyBulletServerRpc()
    {
        bulletManager.RequestDestroyFromBullet(this.NetworkObject, bulletConfigNetworkVariable.Value);
    }

    public float GetDamage()
    {
        return bulletConfigNetworkVariable.Value.damage;
    }
}
