using UnityEngine;

public class ItemPopupManager : MonoBehaviour
{
    [Header("必要引用")]
    public BagData bagData; // 背包数据（内含随机道具函数）
    //public InventoryStores inventoryStores; // 道具数据库（用于查找道具详情）
    public GameObject popupPrefab; // 弹窗预制体
    public Transform canvasTransform; // 弹窗父级（Canvas）

    // 按钮点击触发：调用BagData的随机函数并显示弹窗
    public void ShowRandomItemPopup()
    {
        /*
        // 校验引用
        if (bagData == null)
        {
            Debug.LogError("BagData未赋值");
            return;
        }
        if (inventoryStores == null || inventoryStores.itemList.Count == 0)
        {
            Debug.LogError("道具数据库未赋值或为空");
            return;
        }
        if (popupPrefab == null || canvasTransform == null)
        {
            Debug.LogError("弹窗预制体或Canvas未赋值");
            return;
        }

        // 调用BagData中的随机道具函数，获取随机道具ID
        int randomItemId = bagData.GetRandomid(); // 假设BagData中已实现此方法
        if (randomItemId == -1)
        {
            Debug.Log("所有道具已在背包中，无新道具可获取");
            return;
        }

        // 根据ID从数据库中查找道具详情
        Souvenir targetItem = inventoryStores.itemList.Find(item => item.id == randomItemId);
        if (targetItem == null)
        {
            Debug.LogError($"未找到ID为{randomItemId}的道具");
            return;
        }

        // 实例化弹窗并显示道具信息
        ShowPopup(targetItem);*/
    }

    // 实例化弹窗并绑定逻辑
    private void ShowPopup(Souvenir item)
    {
        GameObject popup = Instantiate(popupPrefab, canvasTransform);
        PopupController controller = popup.GetComponent<PopupController>();
        if (controller == null)
        {
            Debug.LogError("弹窗预制体缺少PopupController组件");
            Destroy(popup);
            return;
        }

        // 初始化弹窗内容，绑定"获取/放弃"回调
        controller.Init(
            item,
            takeCallback: () => 
            { 
                // 1. 更新背包数据（原有逻辑保留）
                if (bagData.playerBag.ContainsKey(item.Id))
                    bagData.playerBag[item.Id]++;
                else
                    bagData.playerBag[item.Id] = 1;

                // 2. 查找InventoryManager，实例化道具UI（核心新增）
                InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager != null)
                {
                    // 调用InventoryManager已有的实例化方法，直接生成UI到背包容器
                    inventoryManager.InstantiateItemUI(item, inventoryManager.BagPool);
                }
                else
                {
                    Debug.LogError("场景中未找到InventoryManager，无法实例化道具UI");
                }

                Destroy(popup);
            },
            cancelCallback: () => Destroy(popup)
        );
    }
}