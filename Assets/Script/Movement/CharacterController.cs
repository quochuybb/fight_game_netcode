using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController : NetworkBehaviour
{
    public bool useGun;
    private float lastTimeShoot;
    protected float LastTimeAttack = 0;
    private static readonly ServerRpcParams DefaultServerRpcParams = new ServerRpcParams();


    public virtual void Awake()
    {
        useGun = false;
    }
    public virtual void Update()
    {
        HandleDelayTime();

    }

    private void HandleDelayTime()
    {
        if (useGun)
        {
            useGun = false;
            OnAttackGunEvent.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {

            BulletController bulletController = other.gameObject.GetComponent<BulletController>();
            if (bulletController != null)
            {
                OnDamgeEvent.Invoke(bulletController.GetDamage());
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("TelePort"))
        {
            gameObject.SetActive(false);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            OnBuffEvent.Invoke(other.gameObject.GetComponent<ItemController>().GetConfig(),DefaultServerRpcParams);
        }
        
    }

    private readonly MoveEvent _moveEvent = new MoveEvent();
    private readonly LookEvent _lookEvent = new LookEvent();
    private readonly AttackGunEvent _attackGun = new AttackGunEvent();
    private readonly ThrowEvent _throwEvent = new ThrowEvent();
    private readonly UnityEvent _onDie = new UnityEvent();
    private readonly UnityEvent _onHealthChanged = new UnityEvent();
    private readonly UnityEvent<float> _onDamge  = new UnityEvent<float>();
    private readonly UnityEvent _onDash = new UnityEvent();
    private readonly BuffEvent _onBuff = new BuffEvent();
    

    public ThrowEvent OnThrow => _throwEvent;
    public UnityEvent OnDash => _onDash;
    public AttackGunEvent OnAttackGunEvent => _attackGun;
    public MoveEvent OnMoveEvent => _moveEvent;
    public LookEvent OnLookEvent => _lookEvent;
    public UnityEvent OnDieEvent => _onDie;
    public UnityEvent OnHealthChangedEvent => _onHealthChanged;
    public UnityEvent<float> OnDamgeEvent => _onDamge;
    public BuffEvent OnBuffEvent => _onBuff;
}
