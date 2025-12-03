using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//读取背包数据
public class BagData : MonoBehaviour
{
    [Header("数据引用")]
    //public InventoryStores InventoryStores;
    public TextAsset BagItemData;
    public TextAsset InventoryItemData;

    [Header("物品数据数组")]
    //public int[] playerInventory;
    public Dictionary<int, int> playerBag = new Dictionary<int, int>();
    public Dictionary<int, int> playerInventory = new Dictionary<int, int>();
    public int bagMaxTotal = 20;

    void Start()
    {
        //InventoryStores.LoadItemData();
        LoadInventoryData();
        LoadBagData();
    }

    //读取背包数据
    public void LoadBagData()
    {
        /*
        playerBag.Clear(); // 清空背包数据

        // 分割CSV行（按换行符拆分）
        string[] dataRow = BagItemData.text.Split('\n');
        int j = 1; // 用于控制最大加载行数（与道具总数匹配）

        foreach (var row in dataRow)
        {
            // 处理空行：直接跳过（避免空行分割后数组越界）
            if (string.IsNullOrWhiteSpace(row))
                continue;

            string[] rowArray = row.Split(',');

            // 跳过表头行（以#开头）
            if (rowArray.Length > 0 && rowArray[0] == "#")
                continue;

            // 限制加载行数不超过道具数据库总数，且行数据格式正确（至少3列）
            if (rowArray.Length >= 3 && rowArray[0] != "#" && j <= InventoryStores.itemList.Count)
            {
                j++;
                // 安全解析ID和数量（避免格式错误导致崩溃）
                if (int.TryParse(rowArray[1], out int id) && int.TryParse(rowArray[2], out int num))
                {
                    playerBag[id] = num; // 存入背包字典
                }
                else
                {
                    Debug.LogWarning($"背包CSV行解析失败：ID或数量不是有效数字，行内容：{row}");
                }
            }
        }*/
    }

    public void SaveBagData()
    {
        string path = Application.dataPath + "/Data/bagdata.csv";
        List<string> dataLines = new List<string>();

        for (int i = 0; i < playerBag.Count; i++)
        {
            if (playerBag[i] > 0)  // 只保存数量大于0的物品
            {
                dataLines.Add("inventory,"+ i.ToString() + "," + playerBag[i].ToString());
            }
        }

        // 写入文件
        File.WriteAllLines(path, dataLines);
    }

    public void Additem(int id)
    {
        playerBag[id]++;
    }

    public void Reduceitem(int id)
    {
        playerBag[id]--;
    }

    public int GetRandomid()
    {
        /*
        // 1. 收集所有道具的ID（从道具数据库）
        List<int> allItemIds = new List<int>();
        foreach (var item in InventoryStores.itemList)
        {
            allItemIds.Add(item.id);
        }

        // 2. 筛选出“背包中数量为0或不存在”的道具ID（即真正未拥有的道具）
        List<int> missingIds = new List<int>();
        foreach (int id in allItemIds)
        {
            // 检查逻辑：如果ID不在背包中，或在背包中但数量为0 → 视为未拥有
            if (!playerBag.ContainsKey(id) || playerBag[id] <= 0)
            {
                missingIds.Add(id);
            }
        }

        // 3. 处理没有可随机道具的情况
        if (missingIds.Count == 0)
        {
            Debug.Log("所有道具都已拥有（数量≥1），无新道具可随机！");
            return -1; // 返回无效ID，外部需判断
        }

        // 4. 从有效ID中随机选择一个
        int randomIndex = Random.Range(0, missingIds.Count);
        return missingIds[randomIndex];*/
        return 0;
    }

    public void GetSpecificitem(int id)
    {
        if (playerBag.ContainsKey(id))
            return ;
        else
        {
            Additem(id);
        }
    }


    //读取仓库数据
    public void LoadInventoryData()
    {
        /*
        playerInventory.Clear(); // 清空仓库数据

        // 分割CSV行（按换行符拆分）
        string[] dataRow = InventoryItemData.text.Split('\n');
        int j = 1; // 用于控制最大加载行数（与道具总数匹配）

        foreach (var row in dataRow)
        {
            // 处理空行：直接跳过（避免空行分割后数组越界）
            if (string.IsNullOrWhiteSpace(row))
                continue;

            string[] rowArray = row.Split(',');

            // 跳过表头行（以#开头）
            if (rowArray.Length > 0 && rowArray[0] == "#")
                continue;

            // 限制加载行数不超过道具数据库总数，且行数据格式正确（至少3列）
            if (rowArray.Length >= 3 && rowArray[0] != "#" && j <= InventoryStores.itemList.Count)
            {
                j++;
                // 安全解析ID和数量（避免格式错误导致崩溃）
                if (int.TryParse(rowArray[1], out int id) && int.TryParse(rowArray[2], out int num))
                {
                    playerInventory[id] = num; // 存入仓库字典
                }
                else
                {
                    Debug.LogWarning($"仓库CSV行解析失败：ID或数量不是有效数字，行内容：{row}");
                }
            }
        }*/
    }

    public void SaveInventoryData()
    {
        string path = Application.dataPath + "/Data/inventorydata.csv";
        List<string> dataLines = new List<string>();

        for (int i = 0; i < playerInventory.Count; i++)
        {
            if (playerInventory[i] > 0)  // 只保存数量大于0的物品
            {
                dataLines.Add("inventory,"+ i.ToString() + "," + playerInventory[i].ToString());
            }
        }

        // 写入文件
        File.WriteAllLines(path, dataLines);
    }
}
