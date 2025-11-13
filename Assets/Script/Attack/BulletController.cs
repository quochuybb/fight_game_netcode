using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private BulletManager bulletManager;
    private BulletNetworkSerializable pendingInit;
    private bool hasPendingInit = false;
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

        base.OnNetworkSpawn();
        if (hasPendingInit)
        {
            bulletConfigNetworkVariable.Value = pendingInit;
            hasPendingInit = false;
        }
    }

    private void Update()
    {
        if (!isShoot)
        {
            return;
        }
        currentDuration += Time.deltaTime;
        if (currentDuration > bulletConfigNetworkVariable.Value.timeExist)
        {
            if (IsOwner)
            {
                DestroyBulletServerRpc();
            }
            currentDuration = 0;
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

    private void BouncesChangedClient()
    {
        BulletNetworkSerializable currentConfig = bulletConfigNetworkVariable.Value;

        currentConfig.bouncing--;
        bulletConfigNetworkVariable.Value = currentConfig;

    }
    private void BouncesChanged()
    {
        if (IsOwner)
        {
            BouncesChangedServerRpc();
        }
        else
        {
            BouncesChangedClient();

        }    
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Bullet"))
        {

            if (bulletConfigNetworkVariable.Value.bouncing> 0)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    if (IsOwner)
                    {
                        DestroyBulletServerRpc();
                    }                }

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
                    if (incoming == Vector2.zero)
                    {
                        if (IsOwner)
                        {
                            DestroyBulletServerRpc();
                        }
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
                BouncesChanged();

            }
            else
            {
                if (IsOwner)
                {
                    DestroyBulletServerRpc();
                }
            }


        }
    }

    
    public void InitConfigBullet(BulletNetworkSerializable bulletNetwork, Vector2 direction)
    {
        this.bulletManager = BulletManager.instance;
        if (NetworkObject != null && NetworkObject.IsSpawned && IsServer)
        {
            this.bulletConfigNetworkVariable.Value = bulletNetwork;
        }
        else
        {
            pendingInit = bulletNetwork;
            hasPendingInit = true;
        }

        this.direction = direction;
        UpdateSpriteBullet();
        currentDuration = 0f;
        isShoot = true;

    }
    
    public void UpdateSpriteBullet()
    {
        transform.localScale = Vector3.one * bulletConfigNetworkVariable.Value.size;
    }
    
    [ServerRpc]
    public void DestroyBulletServerRpc()
    {
        bulletManager.RequestDestroyFromBullet(this.NetworkObject,bulletConfigNetworkVariable.Value);
    }

    public float GetDamage()
    {
        return bulletConfigNetworkVariable.Value.damage;
    }
}
