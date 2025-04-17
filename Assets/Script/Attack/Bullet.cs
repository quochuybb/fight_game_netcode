using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet", menuName = "Bullet")]
public class Bullet : AttackConfig 
{
    public float timeExist;
    public Color colorBullet;
    public float multipleBulletAngle;
    public float numberOfBulletsPerShoot;
    
    public BulletNetworkSerializable Mapping()
    {
        BulletNetworkSerializable serializable = new BulletNetworkSerializable();
        serializable.damage = this.damage;
        serializable.timeExist = this.timeExist;
        serializable.numberOfBulletsPerShoot = this.numberOfBulletsPerShoot;
        serializable.multipleBulletAngle = this.multipleBulletAngle;
        serializable.colorBullet = this.colorBullet;
        serializable.speed = this.speed;
        serializable.delay = this.delay;
        serializable.size = this.size;
        return serializable;
    }
}
