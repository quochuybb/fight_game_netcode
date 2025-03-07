using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SpawnEnemy : CharacterController
{
    [SerializeField] private ParticleSystem particleSystem;
    private float spawnTimer = 0f; 
    private float spawnInterval = 10f; 
    public static SpawnEnemy instance;
    private EnemyPool objectPool;
    private bool canSpawn = true;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (canSpawn)
        {
            if (spawnTimer >= spawnInterval)
            {
                EnemySpawn(gameObject.transform.position); 
                spawnTimer = 0f; 
            }
        }


        if (isClean)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        objectPool = EnemyPool.EnemyPoolInstance;
    }

    public void CreateEffectSpawn(Vector3 position)
    {
        particleSystem.transform.position = position;
        particleSystem.Stop();
        particleSystem.Play();
    }

    public void EnemySpawn(Vector2 startPos)
    {
        
        Random random = new Random(); 
        startPos.x = startPos.x + random.Next(-1, 1) ; 
        startPos.y = startPos.y + random.Next(-1, 1) ; 
        GameObject enemy = objectPool.GetObject();
        if (enemy == null)
        {
            canSpawn = false;
            return;
        }
        enemy.transform.position = startPos;
        enemy.SetActive(true);
        
    }
}
