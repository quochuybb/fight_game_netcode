using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct BulletNetworkSerializable : IEquatable<BulletNetworkSerializable>,INetworkSerializable
{
    public ushort damage;
    public ushort speed;  
    

    public float size;     
    public float delay;    
    public float timeExist; 
    public float multipleBulletAngle; 
    public byte bouncing;           
    public byte numberOfBulletsPerShoot; 
    
    
    public bool Equals(BulletNetworkSerializable other)
    {
        return size == other.size && damage == other.damage && speed == other.speed && delay == other.delay && timeExist == other.timeExist && bouncing == other.bouncing 
               && multipleBulletAngle == other.multipleBulletAngle && numberOfBulletsPerShoot==other.numberOfBulletsPerShoot;
    }

    public override bool Equals(object obj)
    {
        if (obj is BulletNetworkSerializable other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(size, damage, speed, delay, timeExist, bouncing,numberOfBulletsPerShoot, multipleBulletAngle);
    }

    public static bool operator ==(BulletNetworkSerializable left, BulletNetworkSerializable right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BulletNetworkSerializable left, BulletNetworkSerializable right)
    {
        return !(left == right);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref size);
        serializer.SerializeValue(ref delay);
        serializer.SerializeValue(ref timeExist);
        serializer.SerializeValue(ref multipleBulletAngle);
        serializer.SerializeValue(ref bouncing);
        serializer.SerializeValue(ref numberOfBulletsPerShoot);
    }
}
