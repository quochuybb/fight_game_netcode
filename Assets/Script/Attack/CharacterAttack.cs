using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterAttack : NetworkBehaviour
{
    [SerializeField] private Transform firePoint;
    private CharacterController controller;
    private NetworkVariable<Vector2> networkAimDirection = new NetworkVariable<Vector2>( Vector2.right, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);

    [SerializeField] private StatsHandler statsHandler;
    private BulletManager bulletManager;
    private float lastTimeShoot;
    private bool canShoot;
    [SerializeField] private GameObject bulletPrefab;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bulletManager = BulletManager.instance;
        canShoot = false;
    }

    
    public override void OnNetworkSpawn()
    {

        if (IsOwner)
        {
            controller.OnAttackGunEvent.AddListener(OnShooting);
            controller.OnLookEvent.AddListener(OnLookMouse);
        }
        
    }

    private void Update()
    {
        HandleDelayTime();
    }

    private void OnShooting()
    {
        if (!canShoot)
        {
            return;
        }
        canShoot = false;
        if (IsServer)
        {
            CreateAngleAndBullet(statsHandler.currentStatsAttackNetworkVariableHost.Value);
        }
        else
        {
            CreateAngleAndBullet(statsHandler.currentStatsAttackNetworkVariableClient.Value);
        }


    }

    private void CreateAngleAndBullet(BulletNetworkSerializable bullet)
    {
        float HalfOfSumAngleBullet = -(bullet.numberOfBulletsPerShoot/2f) * bullet.multipleBulletAngle
                                     + 0.5f * bullet.multipleBulletAngle;
        for (int i = 0; i < bullet.numberOfBulletsPerShoot; i++)
        {
            float angle = HalfOfSumAngleBullet + i * bullet.multipleBulletAngle;
            CreateBullet(bullet, angle);
        }
    }

    private void OnLookMouse(Vector2 aimDirection)
    {
        this.networkAimDirection.Value = aimDirection;
    }

    private void CreateBullet(BulletNetworkSerializable bulletNetwork, float angle)
    {
        float bulletRadius = 0.15f * bulletNetwork.size ; 

        Vector2 dir = firePoint.right; 
        
        Vector2 offset = dir * bulletRadius;

        Vector2 spawnPos = (Vector2)firePoint.position + offset;

        bulletManager.ShootBulletServerRpc(spawnPos, transform.rotation, bulletNetwork,RotateDirection(networkAimDirection.Value, angle));
    }

    private Vector2 RotateDirection(Vector2 aimDirection, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * aimDirection;    
    }
    public void HandleDelayTime()
    {
        lastTimeShoot += Time.deltaTime;
        if (lastTimeShoot > statsHandler.currentStatsAttackNetworkVariableHost.Value.delay)
        {
            lastTimeShoot = 0f;
            canShoot=true;
        }
        
    }
    
}
