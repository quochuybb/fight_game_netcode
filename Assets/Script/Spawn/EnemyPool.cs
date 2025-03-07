using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool EnemyPoolInstance;
    
    [SerializeField] private List<GameObject> _listObjectPool;
    [SerializeField] private List<GameObject>_objectPrefab;
    private int _objectPoolSize;

    private void Awake()
    {
        EnemyPoolInstance = this;
    }

    private void Start()
    {
        Random random = new Random();
        _objectPoolSize = 20;
        _listObjectPool = new List<GameObject>();
        for (int i = 0; i < _objectPoolSize; i++)
        {
            GameObject obj = Instantiate(_objectPrefab[random.Next(0,_objectPrefab.Count)]);
            obj.SetActive(false);
            _listObjectPool.Add(obj);

        }
    }

    public GameObject GetObject()
    {
        
        for (int i = 0; i < _listObjectPool.Count; i++)
        {
            if (!_listObjectPool[i].activeInHierarchy)
            {
                return _listObjectPool[i];
            }
        }

        return null;
    }
}
