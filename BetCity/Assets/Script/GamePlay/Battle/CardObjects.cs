using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 卡牌栏中的卡牌对象
/// </summary>
public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("卡牌数据")]
    public BetCity.GamePlay.CardOrg.Card cardData;
    
    [Header("动画设置")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private Vector3 hoverOffset = new Vector3(0, 30, 0);
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float clickDuration = 0.1f;
    
    [Header("高亮效果")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Color glowColor = Color.yellow;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isHovered = false;
    private bool isSelected = false;
    
    // 事件
    public System.Action<CardObject> OnCardClicked;
    public System.Action<CardObject> OnCardHoverEnter;
    public System.Action<CardObject> OnCardHoverExit;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        originalScale = transform.localScale;
    }
    
    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
    }
    
    #region 事件接口实现
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;
        
        isHovered = true;
        PlayHoverAnimation(true);
        OnCardHoverEnter?.Invoke(this);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;
        
        isHovered = false;
        PlayHoverAnimation(false);
        OnCardHoverExit?.Invoke(this);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickAnimation();
        OnCardClicked?.Invoke(this);
    }
    
    #endregion
    
    #region 动画方法
    
    /// <summary>
    /// 悬停动画
    /// </summary>
    private void PlayHoverAnimation(bool hover)
    {
        DOTween.Kill(transform);
        
        if (hover)
        {
            // 提升到最前
            transform.SetAsLastSibling();
            
            // 缩放和位移
            transform.DOScale(originalScale * hoverScale, hoverDuration).SetEase(Ease.OutQuad);
            rectTransform.DOAnchorPos(originalPosition + hoverOffset, hoverDuration).SetEase(Ease.OutQuad);
            
            // 发光效果
            if (enableGlow)
                PlayGlowEffect(true);
        }
        else
        {
            transform.DOScale(originalScale, hoverDuration).SetEase(Ease.OutQuad);
            rectTransform.DOAnchorPos(originalPosition, hoverDuration).SetEase(Ease.OutQuad);
            
            if (enableGlow)
                PlayGlowEffect(false);
        }
    }
    
    /// <summary>
    /// 点击动画
    /// </summary>
    private void PlayClickAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * clickScale, clickDuration));
        seq.Append(transform.DOScale(isHovered ? originalScale * hoverScale : originalScale, clickDuration));
    }
    
    /// <summary>
    /// 发光效果
    /// </summary>
    private void PlayGlowEffect(bool enable)
    {
        // 这里可以实现发光效果，例如修改 Outline 组件或添加发光材质
        // 示例：修改卡牌边框颜色
        var outline = GetComponent<UnityEngine.UI.Outline>();
        if (outline != null)
        {
            if (enable)
            {
                outline.effectColor = glowColor;
                outline.enabled = true;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// 进入动画（添加到卡牌栏时）
    /// </summary>
    public void PlayEnterAnimation(float delay = 0f)
    {
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.Append(transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack));
        seq.Join(canvasGroup.DOFade(1f, 0.3f));
    }
    
    /// <summary>
    /// 离开动画（从卡牌栏移除时）
    /// </summary>
    public void PlayExitAnimation(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        seq.Join(canvasGroup.DOFade(0f, 0.2f));
        seq.OnComplete(() => {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            transform.DOScale(originalScale * 1.1f, 0.2f);
            PlayGlowEffect(true);
        }
        else
        {
            if (!isHovered)
            {
                transform.DOScale(originalScale, 0.2f);
                PlayGlowEffect(false);
            }
        }
    }
    
    /// <summary>
    /// 禁用/启用交互
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
        
        if (!interactable)
        {
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 抖动效果（例如无法使用时）
    /// </summary>
    public void PlayShakeAnimation()
    {
        rectTransform.DOShakePosition(0.3f, strength: 10f, vibrato: 10);
    }
    
    #endregion
    
    void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
