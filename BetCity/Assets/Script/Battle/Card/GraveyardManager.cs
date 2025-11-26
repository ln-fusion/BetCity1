using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraveyardManager : MonoBehaviour
{
    [Header("墓地UI组件")]
    public GameObject graveyardPanel;          // 墓地悬浮窗面板
    public Transform cardListContainer;        // 卡牌列表容器
    public GameObject cardPrefab;              // 直接使用卡牌预制体（不是条目）
    public Button closeButton;                 // 关闭按钮
    public TextMeshProUGUI graveyardCountText; // 墓地数量显示文本

    [Header("墓地按钮")]
    public Button graveyardButton;             // 打开墓地面板的按钮

    [Header("显示设置")]
    public float cardScale = 0.5f;             // 卡牌缩放比例
    public int cardsPerRow = 3;                // 每行显示卡牌数量
    public bool showNewestFirst = true;        // 是否最新卡牌显示在最前面

    // 墓地数据
    private List<Card> graveyardCards = new List<Card>();
    private List<GameObject> displayedCardObjects = new List<GameObject>();

    public static GraveyardManager Instance { get; private set; }

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    private void Start()
    {
        RegisterToCombatManager();
    }

    private void OnDestroy()
    {
        UnregisterFromCombatManager();
    }

    private void InitializeUI()
    {
        // 确保面板初始状态为关闭
        if (graveyardPanel != null)
        {
            graveyardPanel.SetActive(false);
        }

        // 绑定按钮事件
        if (graveyardButton != null)
        {
            graveyardButton.onClick.AddListener(ToggleGraveyardPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseGraveyardPanel);
        }

        // 更新墓地数量显示
        UpdateGraveyardCountDisplay();
    }

    private void RegisterToCombatManager()
    {
        // 在这里注册战斗管理器的相关事件
    }

    private void UnregisterFromCombatManager()
    {
        // 取消注册的事件
    }

    public void ToggleGraveyardPanel()
    {
        if (graveyardPanel == null) return;

        if (graveyardPanel.activeSelf)
        {
            CloseGraveyardPanel();
        }
        else
        {
            OpenGraveyardPanel();
        }
    }

    public void OpenGraveyardPanel()
    {
        if (graveyardPanel == null) return;

        graveyardPanel.SetActive(true);
        RefreshGraveyardDisplay();

        Debug.Log("打开墓地面板，当前墓地卡牌数量: " + graveyardCards.Count);
    }
    public void CloseGraveyardPanel()
    {
        if (graveyardPanel == null) return;

        graveyardPanel.SetActive(false);
        ClearDisplayedCards();
        Debug.Log("关闭墓地面板");
    }

    /// <summary>
    /// 刷新墓地显示
    /// </summary>
    public void RefreshGraveyardDisplay()
    {
        // 清理现有的显示卡牌
        ClearDisplayedCards();

        // 确定显示的卡牌列表
        List<Card> cardsToDisplay = GetCardsToDisplay();

        // 创建卡牌显示对象
        for (int i = 0; i < cardsToDisplay.Count; i++)
        {
            CreateCardDisplay(cardsToDisplay[i], i);
        }

        // 更新墓地数量显示
        UpdateGraveyardCountDisplay();
    }


    private List<Card> GetCardsToDisplay()
    {
        List<Card> displayList = new List<Card>();

        if (showNewestFirst)
        {
            // 从后往前遍历（最新卡牌在前）
            for (int i = graveyardCards.Count - 1; i >= 0; i--)
            {
                displayList.Add(graveyardCards[i]);
            }
        }
        else
        {
            // 从前往后遍历（最早卡牌在前）
            displayList.AddRange(graveyardCards);
        }

        return displayList;
    }

    private void CreateCardDisplay(Card card, int index)
    {
        if (cardPrefab == null || cardListContainer == null) return;

        GameObject cardObj = Instantiate(cardPrefab, cardListContainer);
        displayedCardObjects.Add(cardObj);

        // 设置卡牌数据
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.card = card;
            cardDisplay.UpdateCardDisplay();
        }

        // 设置BattleCard状态
        BattleCard battleCard = cardObj.GetComponent<BattleCard>();
        if (battleCard != null)
        {
            battleCard.state = BattleCardState.inGrave;
            battleCard.playerOwner = card.owner;
        }

        // 调整卡牌大小和位置
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * cardScale;

            // 简单的位置排列（可以根据需要改进为网格布局）
            rectTransform.anchoredPosition = new Vector2(
                (index % cardsPerRow) * 120 * cardScale,
                -(index / cardsPerRow) * 160 * cardScale
            );
        }

        // 禁用交互组件（墓地中的卡牌通常不能直接操作）
        Button button = cardObj.GetComponent<Button>();
        if (button != null) button.interactable = false;

        // 可以添加点击查看详情的功能
        AddCardClickHandler(cardObj, card);
    }

    private void AddCardClickHandler(GameObject cardObj, Card card)
    {
        // 可以在这里实现点击卡牌查看详情的功能
        Button button = cardObj.GetComponent<Button>();
        if (button == null) button = cardObj.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnGraveyardCardClicked(card));
        button.interactable = true; // 重新启用交互
    }

    private void OnGraveyardCardClicked(Card card)
    {
        Debug.Log($"点击了墓地中的卡牌: {card.cardName}");
    }

    private void ClearDisplayedCards()
    {
        foreach (GameObject cardObj in displayedCardObjects)
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        displayedCardObjects.Clear();
    }

    private void UpdateGraveyardCountDisplay()
    {
        if (graveyardCountText != null)
        {
            graveyardCountText.text = $"墓地: {graveyardCards.Count}";
        }
    }

    public void SendCardToGraveyard(Card card)
    {
        if (card == null) return;

        graveyardCards.Add(card);

        Debug.Log($"卡牌 [{card.cardName}] 已送入墓地");

        // 如果墓地面板是打开的，刷新显示
        if (graveyardPanel != null && graveyardPanel.activeSelf)
        {
            RefreshGraveyardDisplay();
        }
        else
        {
            UpdateGraveyardCountDisplay();
        }

        // 触发卡牌送入墓地事件
        OnCardSentToGraveyard?.Invoke(card);
    }

    public void SendCardsToGraveyard(List<Card> cards)
    {
        if (cards == null || cards.Count == 0) return;

        graveyardCards.AddRange(cards);
        Debug.Log($"{cards.Count}张卡牌已送入墓地");

        // 如果墓地面板是打开的，刷新显示
        if (graveyardPanel != null && graveyardPanel.activeSelf)
        {
            RefreshGraveyardDisplay();
        }
        else
        {
            UpdateGraveyardCountDisplay();
        }

        // 触发批量卡牌送入墓地事件
        OnCardsSentToGraveyard?.Invoke(cards);
    }

    public void ClearGraveyard()
    {
        int count = graveyardCards.Count;
        graveyardCards.Clear();

        if (graveyardPanel != null && graveyardPanel.activeSelf)
        {
            RefreshGraveyardDisplay();
        }
        else
        {
            UpdateGraveyardCountDisplay();
        }

        Debug.Log($"墓地已清空，移除了{count}张卡牌");
    }

    public int GetGraveyardCardCount()
    {
        return graveyardCards.Count;
    }

    public List<Card> GetAllGraveyardCards()
    {
        return new List<Card>(graveyardCards);
    }

    public bool RemoveCardFromGraveyard(Card card)
    {
        bool removed = graveyardCards.Remove(card);

        if (removed)
        {
            if (graveyardPanel != null && graveyardPanel.activeSelf)
            {
                RefreshGraveyardDisplay();
            }
            else
            {
                UpdateGraveyardCountDisplay();
            }

            Debug.Log($"卡牌 [{card.cardName}] 已从墓地移除");
        }

        return removed;
    }

    // 事件定义
    public delegate void CardSentToGraveyardHandler(Card card);
    public delegate void CardsSentToGraveyardHandler(List<Card> cards);

    public static event CardSentToGraveyardHandler OnCardSentToGraveyard;
    public static event CardsSentToGraveyardHandler OnCardsSentToGraveyard;
}