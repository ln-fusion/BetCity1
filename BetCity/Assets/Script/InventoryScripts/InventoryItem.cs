using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Inventory/New InventoryItem")]
public class InventoryItem
{
    public int id;
    public string itemName;
    [TextArea]
    public string itemInfo;
    int itemArtworkid;
    public Sprite itemImage;
    public int count;

    public InventoryItem(int _id, string _itemName, string _itemInfo, int _itemArtworkid, int _count)
    {
        this.id = _id;
        this.itemName = _itemName;
        this.itemInfo = _itemInfo;
        this.itemArtworkid = _itemArtworkid;
        this.count = _count;
        LoadItemArtwork();
        
    }

    public void LoadItemArtwork()
    {
        string path = $"Image/InventoryImage/{itemArtworkid}";
        itemImage = Resources.Load<Sprite>(path);
    }
}
// 探索类
public class ExplorerItem : InventoryItem
{
    public ExplorerItem(int _id, string _itemName, string _itemInfo, int _itemArtworkId, int _count) 
        : base(_id, _itemName, _itemInfo, _itemArtworkId,  _count)
    {
    }
}

// 战斗类
public class BattleItem : InventoryItem
{
    public BattleItem(int _id, string _itemName, string _itemInfo, int _itemArtworkId, int _count) 
        : base(_id, _itemName, _itemInfo, _itemArtworkId,  _count)
    {
    }
}

