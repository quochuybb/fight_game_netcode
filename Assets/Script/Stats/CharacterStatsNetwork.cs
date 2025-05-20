using System;
using Unity.Netcode;

public class CharacterStatsNetwork : IEquatable<CharacterStatsNetwork>,INetworkSerializable
{
    public float alive;
    public float healthPoint;
    public float gut;
    public float speedMove;
    public float armor;
    public float poison;
    public float burn;
    public bool Equals(CharacterStatsNetwork other)
    {
        return healthPoint == other.healthPoint && gut == other.gut && speedMove == other.speedMove && armor == other.armor && poison == other.poison && burn == other.burn && alive == other.alive;
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
        return HashCode.Combine(healthPoint, gut, speedMove, armor, poison, burn, alive);
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
        serializer.SerializeValue(ref alive);
        serializer.SerializeValue(ref healthPoint);
        serializer.SerializeValue(ref gut);
        serializer.SerializeValue(ref speedMove);
        serializer.SerializeValue(ref armor);
        serializer.SerializeValue(ref poison);
        serializer.SerializeValue(ref burn);
    }
}
