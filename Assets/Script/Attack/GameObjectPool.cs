using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameObjectPool : NetworkBehaviour
{
    public static GameObjectPool ObjectPoolInstance;
    
    [SerializeField] private GameObject _objectPrefab;
    [SerializeField] private int _objectPoolSize;
    [SerializeField] private List<NetworkObject> _listObjectPool;
    private void Awake()
    {
        ObjectPoolInstance = this;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializePool();
        }
    }

    private void InitializePool()
    {
        _listObjectPool = new List<NetworkObject>();
        for (int i = 0; i < _objectPoolSize; i++)
        {
            GameObject gameObject = Instantiate(_objectPrefab);
            gameObject.SetActive(false);
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Spawn();
            _listObjectPool.Add(networkObject);
        }
    }

    public NetworkObject GetObject()
    {
        for (int i = 0; i < _listObjectPool.Count; i++)
        {
            if (!_listObjectPool[i].isActiveAndEnabled)
            {
                return _listObjectPool[i];
            }
        }
        return null;
    }
}
