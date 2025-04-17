using System;
using Unity.Netcode;

public class CharacterStatsNetwork : IEquatable<CharacterStatsNetwork>,INetworkSerializable
{
    public float healthPoint;
    public float damagePercentage;
    public float speedMove;
    public float armor;
    public bool isPoisoning;
    public bool isBurning;
    public bool Equals(CharacterStatsNetwork other)
    {
        return healthPoint == other.healthPoint && damagePercentage == other.damagePercentage && speedMove == other.speedMove && armor == other.armor && isPoisoning == other.isPoisoning;
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
        return HashCode.Combine(healthPoint, damagePercentage, speedMove, armor, isPoisoning, isBurning);
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
        serializer.SerializeValue(ref armor);
        serializer.SerializeValue(ref isPoisoning);
        serializer.SerializeValue(ref isBurning);
    }
}
