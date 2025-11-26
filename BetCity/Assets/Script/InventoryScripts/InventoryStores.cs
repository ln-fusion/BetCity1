using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//读取道具基本信息
//[CreateAssetMenu(fileName="New inventory", menuName="Inventory/New inventory")]
public class InventoryStores : MonoBehaviour
{
    public TextAsset InventoryData;
    public List<InventoryItem> itemList=new List<InventoryItem>();

    private int currentItemIndex = 0;

    void Start()
    {
        
    }

    public void LoadItemData()
    {
        string[] dataRow = InventoryData.text.Split('\n');
        foreach (var row in dataRow)
        {
            string[] rowArray = row.Split(',');
            //Debug.Log("读到"+rowArray[0]);
            if (rowArray[0]=="#")
            {
                continue;
            } 
            else if (rowArray[0]=="explorer")
               //探索类
            {
                int id = int.Parse(rowArray[1]);
                string itemName = rowArray[2];
                string itemInfo = rowArray[3];                
                int itemArtworkid = int.Parse(rowArray[4]);  
                int count = int.Parse(rowArray[5]);
                ExplorerItem explorerItem = new ExplorerItem(id,itemName,itemInfo,itemArtworkid,count);
                itemList.Add(explorerItem);
                //Debug.Log(itemList[0].itemName);

            }
            else if (rowArray[0]=="battle")
               //战斗类
            {
                int id = int.Parse(rowArray[1]);
                string itemName = rowArray[2];
                string itemInfo = rowArray[3];                
                int itemArtworkid = int.Parse(rowArray[4]);
                int count = int.Parse(rowArray[5]);    
                BattleItem battleItem = new BattleItem(id,itemName,itemInfo,itemArtworkid,count);
                itemList.Add(battleItem);
            }
        }
        //Debug.Log(itemList[0]);
    }

    public InventoryItem Ordereditem()
    {
        InventoryItem orderedItem = itemList[currentItemIndex];
        currentItemIndex = (currentItemIndex + 1) % itemList.Count; 
        return orderedItem;
    }
}
