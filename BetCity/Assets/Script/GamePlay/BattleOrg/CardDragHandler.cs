using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 卡牌拖动控制器
/// 支持从手牌/临时区域拖动卡牌到场地格子
/// </summary>
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("拖动设置")]
    [SerializeField] private bool canDrag = true;
    [SerializeField] private float dragScale = 1.08f;
    [SerializeField] private float returnDuration = 0.3f;
    
    [Header("视觉反馈")]
    [SerializeField] private bool showDragPreview = true;
    [SerializeField] private Color dragTintColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float hoverScale = 1.04f;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Transform originalParent;
    private Vector2 originalPosition;
    private int originalSiblingIndex;
    private Vector3 normalScale;
    private bool isDragging = false;
    
    // 拖动预览
    private GameObject dragPreview;
    
    // 有效投放目标
    private Block targetBlock;
    private readonly Dictionary<Block, Color> blockBaseColors = new Dictionary<Block, Color>();
    private readonly Dictionary<Block, Vector3> blockBaseScales = new Dictionary<Block, Vector3>();
    
    // 事件
    public System.Action<CardDragHandler> OnDragStart;
    public System.Action<CardDragHandler> OnDragEnd;
    public System.Action<CardDragHandler, Block> OnDropOnBlock;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvas = GetComponentInParent<Canvas>();
        normalScale = transform.localScale;
    }
    
    #region 拖动接口实现
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        
        // 检查是否可以拖动（手牌与临时区域的怪物卡可拖动）
        BattleCard battleCard = GetComponent<BattleCard>();
        CardDisplay display = GetComponent<CardDisplay>();
        
        if (battleCard == null || (battleCard.state != BattleCardState.inTemp && battleCard.state != BattleCardState.inHand))
        {
            eventData.pointerDrag = null;
            return;
        }
        
        // 使用完全限定名称引用 MonsterCard
        if (display == null || !(display.card is BetCity.GamePlay.CardOrg.MonsterCard))
        {
            eventData.pointerDrag = null;
            return;
        }
        
        isDragging = true;

        transform.DOKill();
        rectTransform.DOKill();
        
        // 保存原始信息
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 拖动开始前先恢复到标准尺寸，避免继承悬停放大
        transform.localScale = normalScale;
        
        // 提升到最前层级
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        
        // 禁用射线检测（让下层可接收事件）
        canvasGroup.blocksRaycasts = false;
        
        // 保持原始大小（拖动时不放大）
        transform.DOScale(normalScale, 0.15f).SetEase(Ease.OutQuad);
        
        // 改变透明度
        canvasGroup.alpha = dragTintColor.a;
        
        // 创建拖动预览
        if (showDragPreview)
        {
            CreateDragPreview();
        }
        
        // 显示所有可用格子
        ShowAvailableBlocks();
        
        OnDragStart?.Invoke(this);
        
        Debug.Log($"开始拖动卡牌: {display.card.cardName}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // 跟随鼠标位置
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
        // 检测悬停的格子
        DetectBlockUnderPointer(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        
        // 销毁预览
        if (dragPreview != null)
        {
            Destroy(dragPreview);
        }
        
        // 隐藏所有格子提示
        HideAllBlocks();
        
        // 检查是否拖到有效格子上
        if (targetBlock != null && targetBlock.card == null)
        {
            // 成功放置到格子
            DropOnBlock(targetBlock);
        }
        else
        {
            // 返回原位
            ReturnToOriginalPosition();
        }
        
        OnDragEnd?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.DOScale(normalScale * hoverScale, 0.2f).SetEase(Ease.OutQuad);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.DOScale(normalScale, 0.2f).SetEase(Ease.OutQuad);
        }
    }
    
    #endregion
    
    #region 拖动逻辑
    
    /// <summary>
    /// 创建拖动预览
    /// </summary>
    private void CreateDragPreview()
    {
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(originalParent, false);
        
        RectTransform previewRect = dragPreview.AddComponent<RectTransform>();
        previewRect.anchorMin = rectTransform.anchorMin;
        previewRect.anchorMax = rectTransform.anchorMax;
        previewRect.pivot = rectTransform.pivot;
        previewRect.anchoredPosition = originalPosition;
        previewRect.sizeDelta = rectTransform.sizeDelta;
        previewRect.localScale = rectTransform.localScale;
        previewRect.localRotation = rectTransform.localRotation;
        previewRect.SetSiblingIndex(originalSiblingIndex);

        // 防止受布局组件影响导致尺寸异常
        LayoutElement layoutElement = dragPreview.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        
        // 添加半透明图像
        var image = dragPreview.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.12f);
        image.raycastTarget = false;
    }
    
    /// <summary>
    /// 显示所有可用格子
    /// </summary>
    private void ShowAvailableBlocks()
    {
        GameObject[] blocks = GetAvailableBlocks();
        if (blocks.Length == 0) return;
        
        foreach (var blockObj in blocks)
        {
            if (blockObj == null) continue;
            Block block = blockObj.GetComponent<Block>();
            if (block == null || block.card != null)
            {
                continue;
            }

            GameObject summonBlockObj = block.GetSummonBlockObject();
            if (summonBlockObj != null)
            {
                blockBaseScales[block] = summonBlockObj.transform.localScale;

                Image image = summonBlockObj.GetComponent<Image>();
                if (image != null)
                {
                    blockBaseColors[block] = image.color;
                }

                summonBlockObj.SetActive(true);
                
                // 播放提示动画
                summonBlockObj.transform.DOScale(blockBaseScales[block] * 1.05f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad);
            }
        }
    }
    
    /// <summary>
    /// 隐藏所有格子提示
    /// </summary>
    private void HideAllBlocks()
    {
        GameObject[] blocks = GetAvailableBlocks();
        if (blocks.Length == 0) return;
        
        foreach (var blockObj in blocks)
        {
            if (blockObj == null) continue;
            Block block = blockObj.GetComponent<Block>();
            if (block == null)
            {
                continue;
            }

            GameObject summonBlockObj = block.GetSummonBlockObject();
            if (summonBlockObj != null)
            {
                DOTween.Kill(summonBlockObj.transform);

                if (blockBaseScales.TryGetValue(block, out var baseScaleValue))
                {
                    summonBlockObj.transform.localScale = baseScaleValue;
                }

                Image image = summonBlockObj.GetComponent<Image>();
                if (image != null && blockBaseColors.TryGetValue(block, out var baseColorValue))
                {
                    image.color = baseColorValue;
                }

                summonBlockObj.SetActive(false);
            }
        }

        blockBaseColors.Clear();
        blockBaseScales.Clear();
    }

    private GameObject[] GetAvailableBlocks()
    {
        if (CombatManager.Instance == null)
        {
            return System.Array.Empty<GameObject>();
        }

        return CombatManager.Instance.GetBlocksSafe();
    }
    
    /// <summary>
    /// 检测指针下的格子
    /// </summary>
    private void DetectBlockUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        Block newTarget = null;
        
        foreach (var result in results)
        {
            Block block = result.gameObject.GetComponentInParent<Block>();
            if (block != null && block.card == null)
            {
                newTarget = block;
                break;
            }
        }
        
        // 目标变化
        if (newTarget != targetBlock)
        {
            // 取消高亮旧目标
            if (targetBlock != null)
            {
                HighlightBlock(targetBlock, false);
            }
            
            // 高亮新目标
            if (newTarget != null)
            {
                HighlightBlock(newTarget, true);
            }
            
            targetBlock = newTarget;
        }
    }
    
    /// <summary>
    /// 高亮格子
    /// </summary>
    private void HighlightBlock(Block block, bool highlight)
    {
        GameObject summonBlockObj = block.GetSummonBlockObject();
        if (summonBlockObj == null) return;
        
        DOTween.Kill(summonBlockObj.transform);

        Image image = summonBlockObj.GetComponent<Image>();
        if (image != null && !blockBaseColors.ContainsKey(block))
        {
            blockBaseColors[block] = image.color;
        }

        Vector3 defaultScale = summonBlockObj.transform.localScale;
        if (blockBaseScales.TryGetValue(block, out var cachedScale))
        {
            defaultScale = cachedScale;
        }
        
        if (highlight)
        {
            summonBlockObj.transform.DOScale(defaultScale * 1.15f, 0.2f).SetEase(Ease.OutQuad);
            
            // 改变颜色
            if (image != null)
            {
                Color baseColor = blockBaseColors.TryGetValue(block, out var c) ? c : image.color;
                Color targetColor = new Color(0.35f, 1f, 0.35f, baseColor.a);
                image.DOColor(targetColor, 0.2f);
            }
        }
        else
        {
            summonBlockObj.transform.DOScale(defaultScale, 0.2f).SetEase(Ease.OutQuad);

            if (image != null)
            {
                Color baseColor = blockBaseColors.TryGetValue(block, out var c) ? c : image.color;
                image.DOColor(baseColor, 0.2f);
            }
        }
    }
    
    /// <summary>
    /// 放置到格子上
    /// </summary>
    private void DropOnBlock(Block block)
    {
        BattleCard battleCard = GetComponent<BattleCard>();
        
        // 调用 CombatManager 的 Summon 方法
        CombatManager.Instance.Summon(battleCard.playerOwner, gameObject, block.transform);
        
        // 播放放置动画
        transform.DOScale(normalScale, 0.3f).SetEase(Ease.OutBack);
        canvasGroup.alpha = 1f;

        // 锁定到格子上
        canDrag = false;
        enabled = false;
        
        OnDropOnBlock?.Invoke(this, block);
        
        Debug.Log($"卡牌已放置到格子");
    }
    
    /// <summary>
    /// 返回原位
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        transform.DOKill();
        canvasGroup.alpha = 1f;
        transform.localScale = normalScale;

        if (originalParent == null)
        {
            Debug.LogWarning("原始父节点为空，无法执行手牌回收动画");
            return;
        }

        // 保持当前释放位置作为起点，交给手牌整理系统进行自然回收
        transform.SetParent(originalParent, true);

        CombatManager combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            combatManager.RearrangeHandAfterReturn(originalParent, rectTransform, originalSiblingIndex, returnDuration * 1.8f);
        }
        else
        {
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }
        
        Debug.Log("卡牌触发手牌整理回收");
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 设置是否可拖动
    /// </summary>
    public void SetDraggable(bool draggable)
    {
        canDrag = draggable;
    }
    
    /// <summary>
    /// 是否正在拖动
    /// </summary>
    public bool IsDragging => isDragging;
    
    #endregion
    
    void OnDestroy()
    {
        DOTween.Kill(transform);
        if (dragPreview != null)
            Destroy(dragPreview);
    }
}
