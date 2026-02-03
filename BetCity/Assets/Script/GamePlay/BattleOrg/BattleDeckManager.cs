using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDeckManager : MonoBehaviour
{
    [Header("UI 组件")]
    public TextMeshProUGUI deckCountText;

    [Header("显示设置")]
    public string displayFormat = "Deck{0}";

    private void Start()
    {
        // 初始化显示
        UpdateDeckCountDisplay();

        // 注册事件监听
        RegisterEvents();
    }

    private void OnDestroy()
    {
        // 取消事件监听
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.onDeckCountChanged.AddListener(OnDeckCountChanged);
        }
        else
        {
            Debug.LogWarning("战斗管理器未找到");
        }
    }

    private void UnregisterEvents()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.onDeckCountChanged.RemoveListener(OnDeckCountChanged);

        }
    }

    private void OnDeckCountChanged(int newCount)
    {
        UpdateDisplay(newCount);
    }

    private void UpdateDisplay(int count)
    {
        if (deckCountText == null)
        {
            Debug.LogWarning("DeckCountText 未设置");
            return;
        }

        // 更新文本
        deckCountText.text = string.Format(displayFormat, count);

    }

    public void UpdateDeckCountDisplay()
    {
        if (CombatManager.Instance != null)
        {
            int currentCount = CombatManager.Instance.GetPublicDeckCount();
            UpdateDisplay(currentCount);
        }
    }

    public void RefreshDisplay()
    {
        UpdateDeckCountDisplay();
    }

    public void SetDisplayFormat(string newFormat)
    {
        displayFormat = newFormat;
        UpdateDeckCountDisplay();
    }
}