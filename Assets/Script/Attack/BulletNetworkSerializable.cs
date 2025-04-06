using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct BulletNetworkSerializable : INetworkSerializable
{
    public float size;
    public float damage;
    public float speed;
    public float delay;
    public float timeExist;
    public Color colorBullet;
    public float multipleBulletAngle;
    public int numberOfBulletsPerShoot;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref size);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref delay);
        serializer.SerializeValue(ref timeExist);
        serializer.SerializeValue(ref colorBullet);
        serializer.SerializeValue(ref multipleBulletAngle);
        serializer.SerializeValue(ref numberOfBulletsPerShoot);
    }
}
