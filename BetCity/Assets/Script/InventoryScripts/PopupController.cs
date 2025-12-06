using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupController : MonoBehaviour
{
    // 弹窗内容组件（需在Inspector绑定预制体中的对应UI）
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescText;
    [SerializeField] private Button takeButton;
    [SerializeField] private Button cancelButton;

    // 回调函数：点击按钮后通知外部处理逻辑（如添加到背包）
    private System.Action onTake;
    private System.Action onCancel;

    private void Awake()
    {
        // 绑定按钮点击事件
        takeButton.onClick.AddListener(OnTakeClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Init(Souvenir item, System.Action takeCallback, System.Action cancelCallback)
    {
        // 显示道具信息
        itemNameText.text = item.Name;
        itemDescText.text = item.Info;
        image.sprite = item.Image; // 假设item有itemImage字段

        // 保存回调（点击按钮后执行外部逻辑）
        onTake = takeCallback;
        onCancel = cancelCallback;
    }

    // 点击“获取”按钮
    private void OnTakeClicked()
    {
        onTake?.Invoke(); // 执行外部的“获取”逻辑（如添加到背包）
        Destroy(gameObject); // 关闭弹窗
    }

    // 点击“放弃”按钮
    private void OnCancelClicked()
    {
        onCancel?.Invoke(); // 执行外部的“放弃”逻辑（可选）
        Destroy(gameObject); // 关闭弹窗
    }
}