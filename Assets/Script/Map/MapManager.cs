using Unity.Netcode;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance { get; private set; }

    public NetworkVariable<int> selectedMap = new NetworkVariable<int>(
        0, writePerm: NetworkVariableWritePermission.Server);


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
        {
            if (selectedMap.Value == 0)
            {
                selectedMap.Value = Random.Range(1, 4); // pick 1..3
                Debug.Log($"[MapManager] server selected map {selectedMap.Value}");
            }
        }
    }

    public Vector3[] GetPositionsForMap(int index)
    {
        switch (index)
        {
            case 1: return new Vector3[] { /* ... */ };
            case 2: return new Vector3[] { /* ... */ };
            case 3: return new Vector3[] { /* ... */ };
            default: return new Vector3[0];
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}