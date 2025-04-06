using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

public class NetworkPooling : NetworkBehaviour
{
    public static NetworkPooling Singleton { get; private set; }

    [SerializeField]
    List<PoolConfigObject> PooledPrefabsList;
    
    HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

    Dictionary<GameObject, ObjectPool<NetworkObject>> m_PooledObjects = new Dictionary<GameObject, ObjectPool<NetworkObject>>();

    public void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        foreach (var configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
    }

    public override void OnNetworkDespawn()
    {
        foreach (var prefab in m_Prefabs)
        {
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            m_PooledObjects[prefab].Clear();
        }
        m_PooledObjects.Clear();
        m_Prefabs.Clear();
    }

    public void OnValidate()
    {
        for (var i = 0; i < PooledPrefabsList.Count; i++)
        {
            var prefab = PooledPrefabsList[i].Prefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkPooling)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }
     public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
     {

         var networkObject = m_PooledObjects[prefab].Get(); 
         var noTransform = networkObject.transform; 
         noTransform.position = position; 
         noTransform.rotation = rotation; 
         return networkObject;
     }


     public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
     { 
         m_PooledObjects[prefab].Release(networkObject);
     }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
    void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        NetworkObject CreateFunc()
        {
            return Instantiate(prefab).GetComponent<NetworkObject>();
        }

        void ActionOnGet(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);

        }

        void ActionOnDestroy(NetworkObject networkObject)
        {
            Destroy(networkObject.gameObject);
        }

        m_Prefabs.Add(prefab);

        // Create the pool
        m_PooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

        // Populate the pool
        var prewarmNetworkObjects = new List<NetworkObject>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
        }
        foreach (var networkObject in prewarmNetworkObjects)
        {
            m_PooledObjects[prefab].Release(networkObject);
        }

        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
    }


}

