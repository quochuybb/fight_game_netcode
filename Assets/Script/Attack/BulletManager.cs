using Unity.Netcode;
using UnityEngine;

public class BulletManager : NetworkBehaviour
{

    [SerializeField] private ParticleSystem particleSystem;
    public static BulletManager instance;
    [SerializeField] private GameObject bulletPrefab;

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
        Debug.LogError("RequestDestroyFromBullet");
        DestroyBullet(networkObject,bullet);
    }

    public void DestroyBullet(NetworkObject networkObject, BulletNetworkSerializable bullet)
    {
        if (!IsServer) return; 
        Debug.LogError("DestroyBullet");

        if (networkObject == null || !networkObject.IsSpawned)
        {
            return;
        }

        CreateEffectDestroyBulletClientRpc(networkObject.transform.position, bullet);

        networkObject.Despawn(false);
        Debug.LogError(networkObject.IsSpawned);

        

    }
    [ServerRpc(RequireOwnership = false)]
    public void ShootBulletServerRpc(Vector2 startPos, Quaternion rotation, BulletNetworkSerializable bulletNetwork, Vector2 direction)
    {
        if (!IsServer) return;
        NetworkObject bullet = NetworkPooling.Singleton.GetNetworkObject(bulletPrefab,startPos, rotation);
        BulletController bulletController = bullet.gameObject.GetComponent<BulletController>();
        bullet.Spawn();

        bulletController.InitConfigBullet(bulletNetwork,direction); 
        
    }
    
}
