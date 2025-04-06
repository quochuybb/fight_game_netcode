using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemNetworkSerializable : INetworkSerializable
{
    public string itemID;
    public string itemName;
    public string typeItemBuff;
    public float statsBuff;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref typeItemBuff);
        serializer.SerializeValue(ref statsBuff);
    }
}
