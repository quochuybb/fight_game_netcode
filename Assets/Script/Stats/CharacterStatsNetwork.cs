using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterStatsNetwork : IEquatable<CharacterStatsNetwork>,INetworkSerializable
{
    public float healthPoint;
    public float damagePercentage;
    public float speedMove;
    public bool Equals(CharacterStatsNetwork other)
    {
        return healthPoint == other.healthPoint && damagePercentage == other.damagePercentage && speedMove == other.speedMove;
    }

    public override bool Equals(object obj)
    {
        if (obj is CharacterStatsNetwork other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(healthPoint, damagePercentage, speedMove);
    }

    public static bool operator ==(CharacterStatsNetwork left, CharacterStatsNetwork right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CharacterStatsNetwork left, CharacterStatsNetwork right)
    {
        return !(left == right);
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref healthPoint);
        serializer.SerializeValue(ref damagePercentage);
        serializer.SerializeValue(ref speedMove);
    }
}
