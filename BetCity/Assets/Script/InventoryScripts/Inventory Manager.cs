using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryPrefab;
    public GameObject InventoryPool;
    public GameObject BagPool;
    public SouvenirDataManager souvenirDataManager; //在Inspector绑定
    List <GameObject> items =new List<GameObject>();

    public BagData BagData;

    [Header("Description Display")]
    public TextMeshProUGUI descriptionDisplayText; // 面板上显示描述的文本组件（需在Inspector绑定）
    public InventoryDisplay selectedItemDisplay; // 记录当前选中的道具

    void Start()
    {
        LoadAllItems();
    }

    //保存
    public void OnClickOpen()
    {
        BagData.SaveBagData();
        BagData.SaveInventoryData();
        Debug.Log("数据保存成功！");
    }

    private void LoadAllItems()
    {
        LoadItemsFromDictionary(BagData.playerBag, BagPool); // 背包：不记录
        LoadItemsFromDictionary(BagData.playerInventory, InventoryPool);  // 仓库：记录
    }

    /// <summary>
    /// 从字典中加载道具并实例化UI
    /// </summary>
    /// <param name="itemDict">道具字典（ID-数量）</param>
    /// <param name="parent">实例化的父容器</param>
    // 重载：加载道具UI并标记是否为仓库道具（修改原有LoadItemsFromDictionary）
    private void LoadItemsFromDictionary(Dictionary<int, int> itemDict, GameObject parent)
    {
        /*
        foreach (var itemPair in itemDict)
        {
            int itemId = itemPair.Key;
            int itemCount = itemPair.Value;

            // 只实例化数量>0的道具
            if (itemCount > 0)
            {
                SouvenirData itemData = souvenirDataManager.GetDataById(itemId);
                // 从道具数据库中查找对应ID的道具信息
                Souvenir itemData = InventoryStores.itemList.Find(item => item.id == itemId);
                if (itemData != null)
                {
                    // 实例化对应数量的道具UI（如果需要显示多个相同道具）
                    for (int i = 0; i < itemCount; i++)
                    {
                        InstantiateItemUI(itemData, parent);
                    }
                }
                else
                {
                    Debug.LogError($"道具数据库中未找到ID为{itemId}的道具");
                }
            }
        }
        */
    }

    /// 实例化单个道具UI
    public void InstantiateItemUI(Souvenir itemData, GameObject parent)
    {
        GameObject newItem = Instantiate(InventoryPrefab, parent.transform);
        InventoryDisplay display = newItem.GetComponent<InventoryDisplay>();

        if (display != null)
        {
            display.souvenir = itemData; // 给道具UI赋值数据
            // 如果有数量显示组件，可在这里设置（例如：display.countText.text = "1";）
        }

        items.Add(newItem); // 加入列表管理
    }

    /// 清空所有实例化的道具UI
    private void ClearAllItems()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }
        items.Clear();
    }

    /// 道具选中逻辑
    public void OnItemSelected(InventoryDisplay clickedItem)
    {
        // 取消上一个选中道具的高亮
        if (selectedItemDisplay != null)
        {
            selectedItemDisplay.selectedHighlight.enabled = false;
        }

        // 点击已选中的道具，取消选中
        if (selectedItemDisplay == clickedItem)
        {
            selectedItemDisplay = null;
            descriptionDisplayText.text = "";
            return;
        }

        // 选中当前道具
        selectedItemDisplay = clickedItem;
        selectedItemDisplay.selectedHighlight.enabled = true;

        // 更新描述文本
        if (descriptionDisplayText != null && clickedItem.souvenir != null)
        {
            descriptionDisplayText.text = clickedItem.souvenir.info;
        }
    }

    /// 保存背包数据（调用BagData的保存方法）
    public void SaveInventoryData()
    {
        BagData.SaveBagData();
        BagData.SaveInventoryData();
        Debug.Log("背包和仓库数据已保存");
    }

    private bool IsBagFull()
    {
        int currentBagTotal = 0;
        foreach (var count in BagData.playerBag.Values)
            currentBagTotal += count;
        return currentBagTotal >= BagData.bagMaxTotal;
    }

    public void TransferInventoryToBag(InventoryDisplay selectedItem)
    {
        // 父容器识别：判断选中的是【仓库】道具（父容器是InventoryPool）
        if (selectedItem.transform.parent != InventoryPool.transform)
        {
            Debug.LogWarning("选中的不是仓库（Inventory）道具，无法转移到背包（Bag）");
            return;
        }

        int itemId = selectedItem.souvenir.id;
        Souvenir itemData = selectedItem.souvenir;

        // 校验背包容量是否已满
        if (IsBagFull())
        {
            Debug.LogWarning("背包（Bag）已满，无法转移");
            return;
        }

        // 校验仓库是否还有该道具
        if (!BagData.playerInventory.ContainsKey(itemId) || BagData.playerInventory[itemId] < 1)
        {
            Debug.LogWarning("仓库（Inventory）中已无该道具，转移失败");
            return;
        }

        // 数据层转移：仓库减1，背包加1
        BagData.playerInventory[itemId]--;
        if (BagData.playerBag.ContainsKey(itemId))
            BagData.playerBag[itemId]++;
        else
            BagData.playerBag[itemId] = 1;

        // UI层同步：仓库删除选中UI，背包新增UI
        Destroy(selectedItem.gameObject); // 仓库UI消失
        InstantiateItemUI(itemData, BagPool); // 背包新增UI
    }

    public void TransferBagToInventory(InventoryDisplay selectedItem)
    {
        // 父容器识别：判断选中的是【背包】道具（父容器是BagPool）
        if (selectedItem.transform.parent != BagPool.transform)
        {
            Debug.LogWarning("选中的不是背包（Bag）道具，无法转移到仓库（Inventory）");
            return;
        }

        int itemId = selectedItem.souvenir.id;
        Souvenir itemData = selectedItem.souvenir;

        // 校验背包是否还有该道具（可按需添加仓库容量限制）
        if (!BagData.playerBag.ContainsKey(itemId) || BagData.playerBag[itemId] < 1)
        {
            Debug.LogWarning("背包（Bag）中已无该道具，转移失败");
            return;
        }

        // 数据层转移：背包减1，仓库加1
        BagData.playerBag[itemId]--;
        if (BagData.playerInventory.ContainsKey(itemId))
            BagData.playerInventory[itemId]++;
        else
            BagData.playerInventory[itemId] = 1;

        // UI层同步：背包删除选中UI，仓库新增UI
        Destroy(selectedItem.gameObject); // 背包UI消失
        InstantiateItemUI(itemData, InventoryPool); // 仓库新增UI
    }

    public void OnTransferToBagButtonClick()
    {
        if (selectedItemDisplay != null)
        {
            TransferInventoryToBag(selectedItemDisplay);
            // 转移后取消选中状态
            selectedItemDisplay.selectedHighlight.enabled = false;
            selectedItemDisplay = null;
            descriptionDisplayText.text = "";
        }
        else
        {
            Debug.LogWarning("请先选中仓库（Inventory）中的道具");
        }
    }

    // 点击“转移到仓库”按钮触发（背包→仓库）
    public void OnTransferToInventoryButtonClick()
    {
        if (selectedItemDisplay != null)
        {
            TransferBagToInventory(selectedItemDisplay);
            selectedItemDisplay.selectedHighlight.enabled = false;
            selectedItemDisplay = null;
            descriptionDisplayText.text = "";
        }
        else
        {
            Debug.LogWarning("请先选中背包（Bag）中的道具");
        }
    }

}