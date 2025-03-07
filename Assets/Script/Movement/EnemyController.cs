using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : CharacterController
{
    protected Transform target;
    public override void Awake()
    {
        base.Awake();
        target = FindPlayer();
    }

    private void Start()
    {
        onCleanEvent.AddListener(Clean);
    }

    public virtual void FixedUpdate()
    {
        target = FindPlayer();
    }

    public Transform FindPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    public Transform FindClean()
    {
        try
        {
            return GameObject.FindGameObjectWithTag("Clean").GetComponent<Transform>();
        }
        catch (Exception e)
        {
            return GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
    }
    public Transform FindTelePort()
    {
        return GameObject.FindGameObjectWithTag("TelePort").GetComponent<Transform>();
    }

    public float DistanceToTarget(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    public Vector2 DirectionToTarget(Transform target)
    {
        return (target.position - transform.position).normalized;
    }

    public bool CanSeeObject(Transform target)
    {
        return DistanceToTarget(target) < 100f;
    }

    public void Clean()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            gameObject.SetActive(false);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Village"))
        {
            gameObject.tag = "Clean";
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Stained"))
        {
            Destroy(gameObject);
        }
    }
    
}
