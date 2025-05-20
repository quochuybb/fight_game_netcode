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
    [SerializeField] private List<ChestController> _listChestController;
    private float timer;


    private void Awake()
    {
        instance = this;
        timer = 0f;
    }

    private void Start()
    {
        foreach (var chestController in _listChestController)
        {
            chestController.onOpenChest.AddListener(OpenChest);
        }
        foreach (var item in items)
        {
            listItemNetworkSerializables.Add(item.Mapping());
        }
    }

    public void RequestDestroyFromItem(NetworkObject networkObject)
    {
        if (IsOwner)
        {
            this.itemNetworkObject = networkObject;
            DestroyItemServerRpc();
        }
        
    }

    public void OpenChest(Transform chestTransform)
    {
        float randomX = Random.Range(chestTransform.position.x-1f, chestTransform.position.x+1f);
        float randomY = Random.Range(chestTransform.position.y-1f, chestTransform.position.y+1f);
        Vector3 spanwItem = new Vector3(randomX, randomY, chestTransform.position.z);
        int randomIndex = Random.Range(0, listItemNetworkSerializables.Count);
        ItemNetworkSerializable itemNetworkSerializable = listItemNetworkSerializables[randomIndex];
        SpawnItemServerRpc(spanwItem,transform.rotation,itemNetworkSerializable);
    }
    [ClientRpc]
    public void CreateEffectDestroyItemClientRpc(Vector3 position)
    {
        particleSystem.transform.position = position;
        ParticleSystem.EmissionModule em = particleSystem.emission;
        em.SetBurst(0, new ParticleSystem.Burst(0, Mathf.Ceil( 3f)));
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startSpeedMultiplier = 7f;
        particleSystem.Stop();
        particleSystem.Play();
    }

    [ServerRpc]
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
