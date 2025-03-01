using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet", menuName = "Bullet")]
public class Bullet : AttackConfig
{
    public float duration;
    public Color colorBullet;
    public float multipleBulletAngle;
    public int numberOfBulletsPerShoot;
}
