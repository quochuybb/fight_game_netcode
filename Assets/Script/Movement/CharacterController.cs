using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController : NetworkBehaviour
{
    public bool useGun;
    public bool useMelee;
    public bool isClean = false;
    public bool canThrow = false;
    private float lastTimeShoot;
    protected float lastTimeAttack = 0;
    [SerializeField] protected Bullet bulletConfig;
    [SerializeField] protected BombConfig bombConfig;

    public virtual void Awake()
    {
        useGun = false;
        useMelee = false;
    }

    public virtual void Update()
    {
        HandleDelayTime();
        if (isClean)
        {
            onCleanEvent.Invoke();
            isClean = false;
        }
    }

    public void HandleDelayTime()
    {
        lastTimeShoot += Time.deltaTime;
        if (useGun && lastTimeShoot > bulletConfig.delay)
        {
            lastTimeShoot = 0f;
            useGun = false;
            onAttackGunEvent.Invoke(bulletConfig);
        }

        //if (canThrow)
        //{
        //    canThrow = false;
        //    onThrow.Invoke();
        //}
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {   
            BulletController bulletController = other.gameObject.GetComponent<BulletController>();
            if (bulletController != null && !isClean)
            {
                onDamgeEvent.Invoke(bulletController.GetDamage());
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("TelePort") && gameObject.tag == "Clean")
        {
            gameObject.SetActive(false);
        } 

        //else if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        //{
        //    Debug.Log(other.gameObject.name);
        //    onDamgeEvent.Invoke(bombConfig.damage);
        //}
    }

    private readonly MoveEvent _moveEvent = new MoveEvent();
    private readonly LookEvent _lookEvent = new LookEvent();
    private readonly AttackGunEvent _attackGun = new AttackGunEvent();
    private readonly ThrowEvent _throwEvent = new ThrowEvent();
    private UnityEvent OnClean = new UnityEvent();
    private UnityEvent OnHealthChanged = new UnityEvent();
    private UnityEvent<float> OnDamge  = new UnityEvent<float>();
    private UnityEvent _onDash = new UnityEvent();

    public ThrowEvent onThrow => _throwEvent;
    public UnityEvent onDash => _onDash;
    public AttackGunEvent onAttackGunEvent => _attackGun;
    public MoveEvent onMoveEvent => _moveEvent;
    public LookEvent onLookEvent => _lookEvent;
    public UnityEvent onCleanEvent => OnClean;
    public UnityEvent onHealthChangedEvent => OnHealthChanged;
    public UnityEvent<float> onDamgeEvent => OnDamge;
}
