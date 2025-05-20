using Unity.Netcode;
using UnityEngine;

public class BulletManager : NetworkBehaviour
{

    [SerializeField] private ParticleSystem particleSystem;
    public static BulletManager instance;
    [SerializeField] private GameObject bulletPrefab;
    private NetworkObject bulletNetworkObject;

    private void Awake()
    {
        instance = this;

    }


    [ClientRpc]
    public void CreateEffectDestroyBulletClientRpc(Vector3 position, BulletNetworkSerializable bullet)
    {
        particleSystem.transform.position = position;
        ParticleSystem.EmissionModule em = particleSystem.emission;
        em.SetBurst(0, new ParticleSystem.Burst(0, Mathf.Ceil(bullet.size * 5f)));
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startSpeedMultiplier = bullet.size * 10f;
        particleSystem.Stop();
        particleSystem.Play();
    }

    public void RequestDestroyFromBullet(NetworkObject networkObject,BulletNetworkSerializable bullet)
    {
        this.bulletNetworkObject = networkObject;
        DestroyBulletServerRpc(bullet);
    }

    [ServerRpc]
    public void DestroyBulletServerRpc(BulletNetworkSerializable bullet)
    {
        if (bulletNetworkObject.IsSpawned)
        {
            CreateEffectDestroyBulletClientRpc(bulletNetworkObject.transform.position, bullet);
            bulletNetworkObject.Despawn();

        }
        if (!bulletNetworkObject.gameObject.activeInHierarchy)
        {
            return;
        }
        CreateEffectDestroyBulletClientRpc(bulletNetworkObject.transform.position, bullet);
        NetworkPooling.Singleton.ReturnNetworkObject(this.bulletNetworkObject,bulletPrefab);

    }
    [ServerRpc(RequireOwnership = false)]
    public void ShootBulletServerRpc(Vector2 startPos, Quaternion rotation, BulletNetworkSerializable bulletNetwork, Vector2 direction)
    {
        
        NetworkObject bullet = NetworkPooling.Singleton.GetNetworkObject(bulletPrefab,startPos, rotation);
        BulletController bulletController = bullet.gameObject.GetComponent<BulletController>();
        bulletController.InitConfigBullet(bulletNetwork,direction);
        bullet.Spawn();
    }
    
}
