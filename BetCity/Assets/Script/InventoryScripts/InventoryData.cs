using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryData : MonoBehaviour
{
    [Header("数据引用")]
    public InventoryStores InventoryStores;
    public TextAsset playerItemData;

    [Header("物品数据数组")]
    //public int[] playerInventory;
    public Dictionary<int, int> playerInventory = new Dictionary<int, int>();

    void Start()
    {
    }

    //读取仓库数据
}
