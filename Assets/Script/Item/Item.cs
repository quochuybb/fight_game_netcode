using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "ItemConfig")]
public class Item : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string nameStatsBuff;
    public float statsBuff;
    public int typeBuff;
    public ItemNetworkSerializable Mapping()
    {
        ItemNetworkSerializable item = new ItemNetworkSerializable();
        item.itemID = itemID;
        item.itemName = itemName;
        item.nameStatsBuff = nameStatsBuff;
        item.statsBuff = statsBuff;
        item.typeBuff = typeBuff;
        return item;
    }
        
}