using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterStatsNetwork : INetworkSerializable
{
    public float healthPoint;
    public float damagePercentage;
    public float speedMove;
    public Color color;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref healthPoint);
        serializer.SerializeValue(ref speedMove);
        serializer.SerializeValue(ref damagePercentage);
        serializer.SerializeValue(ref color);
    }
}
