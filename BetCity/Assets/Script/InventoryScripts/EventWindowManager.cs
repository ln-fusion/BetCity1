using UnityEngine;
using UnityEngine.UI;

public class EventWindowManager : MonoBehaviour
{
    // 事件窗口预制体
    public GameObject eventWindowPrefab; 
    // 实例化后的窗口对象
    private GameObject eventWindowInstance; 

    // 引用道具弹窗管理器
    public ItemPopupManager itemPopupManager; 

    // 主界面按钮调用：打开事件窗口
    public void OpenEventWindow()
    {
        // 避免重复创建
        if (eventWindowInstance != null) return; 

        // 实例化窗口到 Canvas 下
        eventWindowInstance = Instantiate(eventWindowPrefab, FindObjectOfType<Canvas>().transform, false);

        // 找到“打开宝箱”按钮并绑定事件
        Button randomBtn = eventWindowInstance.transform.Find("RandomItemButton").GetComponent<Button>();
        randomBtn.onClick.AddListener(() => OnRandomButtonClick(randomBtn.gameObject));

        // 找到“关闭”按钮并绑定事件
        Button closeBtn = eventWindowInstance.transform.Find("CloseButton").GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseEventWindow);
    }

    // “打开宝箱”按钮点击事件
    private void OnRandomButtonClick(GameObject buttonObj)
    {
        // 销毁被点击的按钮
        Destroy(buttonObj); 

        // 触发随机道具事件
        itemPopupManager.ShowRandomItemPopup(); 
    }

    // 关闭事件窗口
    public void CloseEventWindow()
    {
        if (eventWindowInstance != null)
        {
            Destroy(eventWindowInstance);
            eventWindowInstance = null;
        }
    }
}