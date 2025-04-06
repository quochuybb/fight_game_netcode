using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : NetworkBehaviour
{

    public static ItemManager instance;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private GameObject itemPrefab;
    private NetworkObject itemNetworkObject;
    [SerializeField] private List<Item> items;
    private List<ItemNetworkSerializable> listItemNetworkSerializables = new List<ItemNetworkSerializable>();
    private float timer;
    private int count = 0;


    private void Awake()
    {
        instance = this;
        timer = 0f;
        count = 0;
    }

    private void Start()
    {
        foreach (var item in items)
        {
            listItemNetworkSerializables.Add(item.Mapping());
        }
    }

    public void RequestDestroyFromItem(NetworkObject networkObject)
    {
        this.itemNetworkObject = networkObject;
        DestroyItemServerRpc();
    }
    public Vector2 GetRandomPosition()
    {
        float randomX = Random.Range(-5f, 5f);
        float randomY = Random.Range(-5f, 5f);
        return new Vector2(randomX, randomY);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10f && count < 4)
        {
            timer = 0f;
            count+=1;
            Debug.Log(count);
            int randomIndex = Random.Range(0, listItemNetworkSerializables.Count);
            ItemNetworkSerializable itemNetworkSerializable = listItemNetworkSerializables[randomIndex];
            SpawnItemServerRpc(GetRandomPosition(),transform.rotation,itemNetworkSerializable);
        }
    }
    [ClientRpc]
    public void CreateEffectDestroyItemClientRpc(Vector3 position )
    {
        particleSystem.transform.position = position;
        ParticleSystem.EmissionModule em = particleSystem.emission;
        em.SetBurst(0, new ParticleSystem.Burst(0, Mathf.Ceil( 3f)));
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startSpeedMultiplier = 7f;
        particleSystem.Stop();
        particleSystem.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc()
    {
        if (itemNetworkObject.IsSpawned)
        {
            itemNetworkObject.Despawn();
        }
        if (!itemNetworkObject.gameObject.activeInHierarchy)
        {
            return;
        }
        NetworkPooling.Singleton.ReturnNetworkObject(this.itemNetworkObject,itemPrefab);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnItemServerRpc(Vector2 startPos, Quaternion rotation, ItemNetworkSerializable itemNetwork)
    {
        NetworkObject item = NetworkPooling.Singleton.GetNetworkObject(itemPrefab,startPos, rotation);
        ItemController itemController = item.gameObject.GetComponent<ItemController>();
        itemController.Init(itemNetwork);
        item.Spawn();
        itemController.UpdateItemClientRpc(itemNetwork);
    }


    
}
