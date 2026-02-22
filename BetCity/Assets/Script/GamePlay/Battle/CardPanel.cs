using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 卡牌栏面板，支持DOTween动画
/// </summary>
public class CardPanel : MonoBehaviour
{
    [Header("可见性设置")]
    [SerializeField] private bool visible = true;
    
    [Header("面板组件")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform cardContainer; // 卡牌容器
    
    [Header("动画设置")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    
    [Header("卡牌动画设置")]
    [SerializeField] private float cardAddDuration = 0.3f;
    [SerializeField] private float cardRemoveDuration = 0.2f;
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private Vector3 cardAddStartOffset = new Vector3(0, -100, 0);
    
    [Header("悬停效果")]
    [SerializeField] private bool enableHoverEffect = true;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, 20, 0);
    
    [Header("卡牌交互设置")]
    [SerializeField] private CardInteractionMode cardInteractionMode = CardInteractionMode.Drag;
    
    private List<GameObject> cardObjects = new List<GameObject>();
    private Sequence currentSequence;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;
    
    void Awake()
    {
        InitializeComponents();
        SetupPositions();
    }
    
    void Start()
    {
        if (visible)
            Show(false);
        else
            Hide(false);
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (cardContainer == null)
            cardContainer = transform;
    }
    
    /// <summary>
    /// 设置显示/隐藏位置
    /// </summary>
    private void SetupPositions()
    {
        visiblePosition = panelRect.anchoredPosition;
        // 默认向下隐藏
        hiddenPosition = visiblePosition + new Vector3(0, -panelRect.rect.height - 50, 0);
    }
    
    #region 面板显示/隐藏
    
    /// <summary>
    /// 显示卡牌栏
    /// </summary>
    public void Show(bool animated = true)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        
        visible = true;
        
        currentSequence?.Kill();
        
        if (animated)
        {
            currentSequence = DOTween.Sequence();
            currentSequence.Append(panelRect.DOAnchorPos(visiblePosition, showDuration).SetEase(showEase));
            currentSequence.Join(canvasGroup.DOFade(1f, showDuration));
        }
        else
        {
            panelRect.anchoredPosition = visiblePosition;
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 隐藏卡牌栏
    /// </summary>
    public void Hide(bool animated = true)
    {
        visible = false;
        
        currentSequence?.Kill();
        
        if (animated)
        {
            currentSequence = DOTween.Sequence();
            currentSequence.Append(panelRect.DOAnchorPos(hiddenPosition, hideDuration).SetEase(hideEase));
            currentSequence.Join(canvasGroup.DOFade(0f, hideDuration));
            currentSequence.OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            panelRect.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 切换显示状态
    /// </summary>
    public void Toggle()
    {
        if (visible)
            Hide();
        else
            Show();
    }
    
    #endregion
    
    #region 卡牌管理
    
    /// <summary>
    /// 添加卡牌到卡牌栏
    /// </summary>
    public void AddCard(GameObject cardPrefab, bool animated = true)
    {
        if (cardPrefab == null) return;
        
        GameObject cardObj = Instantiate(cardPrefab, cardContainer);
        cardObjects.Add(cardObj);
        
        // 设置交互模式
        var battleCard = cardObj.GetComponent<BattleCard>();
        if (battleCard != null)
        {
            battleCard.SetInteractionMode(cardInteractionMode);
        }
        
        if (animated)
        {
            // 设置起始位置
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            Vector3 targetPos = cardRect.anchoredPosition;
            cardRect.anchoredPosition = targetPos + cardAddStartOffset;
            
            // 设置起始透明度
            CanvasGroup cardGroup = cardObj.GetComponent<CanvasGroup>();
            if (cardGroup == null)
                cardGroup = cardObj.AddComponent<CanvasGroup>();
            cardGroup.alpha = 0f;
            
            // 播放动画
            Sequence seq = DOTween.Sequence();
            seq.Append(cardRect.DOAnchorPos(targetPos, cardAddDuration).SetEase(Ease.OutBack));
            seq.Join(cardGroup.DOFade(1f, cardAddDuration));
        }
        
        // 添加悬停效果（仅在非拖动模式下）
        if (enableHoverEffect && cardInteractionMode == CardInteractionMode.Click)
            AddHoverEffect(cardObj);
        
        RefreshCardPositions();
    }
    
    /// <summary>
    /// 移除卡牌
    /// </summary>
    public void RemoveCard(GameObject cardObj, bool animated = true)
    {
        if (cardObj == null || !cardObjects.Contains(cardObj)) return;
        
        cardObjects.Remove(cardObj);
        
        if (animated)
        {
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            CanvasGroup cardGroup = cardObj.GetComponent<CanvasGroup>();
            
            Sequence seq = DOTween.Sequence();
            seq.Append(cardRect.DOScale(0f, cardRemoveDuration).SetEase(Ease.InBack));
            seq.Join(cardGroup.DOFade(0f, cardRemoveDuration));
            seq.OnComplete(() => Destroy(cardObj));
        }
        else
        {
            Destroy(cardObj);
        }
        
        RefreshCardPositions();
    }
    
    /// <summary>
    /// 移除指定索引的卡牌
    /// </summary>
    public void RemoveCardAtIndex(int index, bool animated = true)
    {
        if (index < 0 || index >= cardObjects.Count) return;
        RemoveCard(cardObjects[index], animated);
    }
    
    /// <summary>
    /// 清空所有卡牌
    /// </summary>
    public void ClearCards(bool animated = true)
    {
        if (animated)
        {
            Sequence seq = DOTween.Sequence();
            for (int i = cardObjects.Count - 1; i >= 0; i--)
            {
                GameObject card = cardObjects[i];
                RectTransform cardRect = card.GetComponent<RectTransform>();
                CanvasGroup cardGroup = card.GetComponent<CanvasGroup>();
                
                seq.Join(cardRect.DOScale(0f, cardRemoveDuration).SetEase(Ease.InBack).SetDelay(i * 0.05f));
                seq.Join(cardGroup.DOFade(0f, cardRemoveDuration).SetDelay(i * 0.05f));
            }
            seq.OnComplete(() => {
                foreach (var card in cardObjects)
                    Destroy(card);
                cardObjects.Clear();
            });
        }
        else
        {
            foreach (var card in cardObjects)
                Destroy(card);
            cardObjects.Clear();
        }
    }
    
    /// <summary>
    /// 刷新卡牌位置（自动排列）
    /// </summary>
    private void RefreshCardPositions()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer.GetComponent<RectTransform>());
    }
    
    #endregion
    
    #region 悬停效果
    
    /// <summary>
    /// 为卡牌添加悬停效果
    /// </summary>
    private void AddHoverEffect(GameObject cardObj)
    {
        var trigger = cardObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = cardObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        // 鼠标进入
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => OnCardHoverEnter(cardObj));
        trigger.triggers.Add(pointerEnter);
        
        // 鼠标离开
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => OnCardHoverExit(cardObj));
        trigger.triggers.Add(pointerExit);
    }
    
    private void OnCardHoverEnter(GameObject cardObj)
    {
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutQuad);
        cardRect.DOAnchorPos(cardRect.anchoredPosition + (Vector2)hoverOffset, hoverDuration).SetEase(Ease.OutQuad);
        
        // 提升层级
        cardObj.transform.SetAsLastSibling();
    }
    
    private void OnCardHoverExit(GameObject cardObj)
    {
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.DOScale(1f, hoverDuration).SetEase(Ease.OutQuad);
        cardRect.DOAnchorPos(cardRect.anchoredPosition - (Vector2)hoverOffset, hoverDuration).SetEase(Ease.OutQuad);
    }
    
    #endregion
    
    #region 公共属性
    
    public int CardCount => cardObjects.Count;
    public bool IsVisible => visible;
    public List<GameObject> GetCards() => new List<GameObject>(cardObjects);
    
    #endregion
    
    void OnDestroy()
    {
        currentSequence?.Kill();
        DOTween.Kill(transform);
    }
}
