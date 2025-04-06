using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "ItemConfig")]
public class Item : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string typeItemBuff;
    public float statsBuff;

    public ItemNetworkSerializable Mapping()
    {
        ItemNetworkSerializable item = new ItemNetworkSerializable();
        item.itemID = itemID;
        item.itemName = itemName;
        item.typeItemBuff = typeItemBuff;
        return item;
    }
        
}