using Unity.Netcode;

public class ItemNetworkSerializable : INetworkSerializable
{
    public string itemID;
    public string itemName;
    public string nameStatsBuff;
    public float statsBuff;
    public int typeBuff;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref nameStatsBuff);
        serializer.SerializeValue(ref statsBuff);
        serializer.SerializeValue(ref typeBuff);
        
    }
}
