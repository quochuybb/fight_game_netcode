using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemController : NetworkBehaviour, INetworkSerializable
{
    public SpriteRenderer spriteRenderer; 
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private CircleCollider2D circleCollider ;
    private ItemNetworkSerializable config;
    private CharacterController characterController;
    private ItemManager itemManager;
    private SpriteManager spriteManager;
    private bool isSpawned = false;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteManager = FindObjectOfType<SpriteManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DestroyItem(transform.position,true);
    }
    public void Init(ItemNetworkSerializable config)
    {
        this.config = config;
        itemManager = ItemManager.instance;
        UpdateSpriteItem();
        isSpawned = true;
        UpdateColliderToSprite();
    }
    public void UpdateSpriteItem()
    {
        spriteRenderer.sprite = spriteManager.GetSprite(this.config.itemID);

    }
    [ClientRpc]
    public void UpdateItemClientRpc(ItemNetworkSerializable newConfig)
    {
        Init(newConfig);
    }
    public void UpdateColliderToSprite()
    {
        if (spriteRenderer.sprite != null)
        {
            float radius = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y) / 2f;
            circleCollider.radius = radius;
        }
    }
    public void DestroyItem(Vector3 pos, bool animate)
    {

        if (animate)
        {
            itemManager.CreateEffectDestroyItemClientRpc(pos);
        }
        itemManager.RequestDestroyFromItem(this.NetworkObject);
    }
    public ItemNetworkSerializable GetConfig()
    {
        return config;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref config);
    }
}
