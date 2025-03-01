using Unity.Collections;
using Unity.Netcode;
public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.info);
    }

    public override string ToString()
    {
        return info.ToString();
    }
    public static implicit operator string(NetworkString networkString) => networkString.ToString();
    public static implicit operator NetworkString(string networkString) => new NetworkString() {info = new FixedString32Bytes(networkString)};
    
}
