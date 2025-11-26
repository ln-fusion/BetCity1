using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    [Header("Data Reference")]
    public InventoryItem item;

    [Header("Visual Components")]
    public Image itemArtworkImage; // 物品背景
    public TextMeshProUGUI itemNameText; // 物品名称
    public TextMeshProUGUI itemInfo; // 物品描述


    [Header("Selection Visual")]
    public Image selectedHighlight; // 选中时的高亮边框（需在Inspector绑定，默认隐藏）
    private InventoryManager inventoryManager; // 引用管理器

    void Start()
    {
        UpdateItemDisplay();
    }

    void Awake()
    {
        // 获取管理器实例（确保场景中InventoryManager是单例或可通过Find找到）
        inventoryManager = FindObjectOfType<InventoryManager>();
        // 初始化：默认未选中，隐藏高亮
        if (selectedHighlight != null)
            selectedHighlight.enabled = false;
    }

    public void OnItemClicked()
    {
        // 通知管理器选中当前道具
        inventoryManager.OnItemSelected(this);
    }

    public void UpdateItemDisplay()
    {
        if (item == null)
            return;

        // 更新物品名称
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName.ToString();
        }

        // 更新物品描述（）
        if (itemInfo != null)
        {
            itemInfo.text = item.itemInfo.ToString();
        }

        // 更新物品艺术图
        if (itemArtworkImage != null)
        {
            itemArtworkImage.sprite = item.itemImage; // Image通过sprite属性赋值
            itemArtworkImage.enabled = item.itemImage != null; // 图片为空时隐藏
        }
        /**/
    }
}
