using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet", menuName = "Bullet")]
public class Bullet : ScriptableObject 
{
    public float size;
    public ushort damage;
    public ushort speed;
    public float delay;
    public float timeExist;
    public byte bouncing;
    public float multipleBulletAngle;
    public byte numberOfBulletsPerShoot;
    
    public BulletNetworkSerializable Mapping()
    {
        BulletNetworkSerializable serializable = new BulletNetworkSerializable();
        serializable.damage = this.damage;
        serializable.timeExist = this.timeExist;
        serializable.numberOfBulletsPerShoot = this.numberOfBulletsPerShoot;
        serializable.multipleBulletAngle = this.multipleBulletAngle;
        serializable.bouncing = this.bouncing;
        serializable.speed = this.speed;
        serializable.delay = this.delay;
        serializable.size = this.size;
        return serializable;
    }
}
