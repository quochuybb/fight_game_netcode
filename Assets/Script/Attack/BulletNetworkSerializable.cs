using System;
using Unity.Netcode;
using UnityEngine;

public struct BulletNetworkSerializable : IEquatable<BulletNetworkSerializable>, INetworkSerializable
{
    public float size;
    public float damage;
    public float speed;
    public float delay;
    public float timeExist;
    public float bouncing;
    public float multipleBulletAngle;
    public byte numberOfBulletsPerShoot; 

    public bool Equals(BulletNetworkSerializable other)
    {
        return size == other.size
               && damage == other.damage
               && speed == other.speed
               && delay == other.delay
               && timeExist == other.timeExist
               && bouncing == other.bouncing
               && multipleBulletAngle == other.multipleBulletAngle
               && numberOfBulletsPerShoot == other.numberOfBulletsPerShoot;
    }

    public override bool Equals(object obj)
    {
        return obj is BulletNetworkSerializable other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(size, damage, speed, delay, timeExist, bouncing, multipleBulletAngle, numberOfBulletsPerShoot);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize primitives directly
        serializer.SerializeValue(ref size);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref delay);
        serializer.SerializeValue(ref timeExist);
        serializer.SerializeValue(ref bouncing);
        serializer.SerializeValue(ref multipleBulletAngle);
        serializer.SerializeValue(ref numberOfBulletsPerShoot);
    }
}