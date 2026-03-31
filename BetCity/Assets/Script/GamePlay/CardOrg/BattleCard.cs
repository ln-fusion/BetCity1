using BetCity.GamePlay.CardOrg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BattleCardState
{
    inHand, inBlock, inTemp, inGrave
}

public enum CardInteractionMode
{
    Click,  // 点击模式（兼容保留，不再执行召唤）
    Drag    // 拖动模式（新增）
}

public class BattleCard : MonoBehaviour, IPointerDownHandler
{
    public CardOwner playerOwner; 
    public BattleCardState state = BattleCardState.inHand;
    
    [Header("交互模式")]
    [SerializeField] private CardInteractionMode interactionMode = CardInteractionMode.Drag;
    
    private CardDragHandler dragHandler;

    void Awake()
    {
        // 根据交互模式添加组件
        UpdateInteractionMode();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 只在点击模式下处理
        if (interactionMode != CardInteractionMode.Click)
            return;

        Debug.Log("点击召唤已弃用，请使用拖动卡牌到目标格子进行召唤");
    }

    void Start()
    {
        // 初始化时设置玩家归属
        CardDisplay display = GetComponent<CardDisplay>();
        if (display != null && display.card != null)
        {
            playerOwner = display.card.owner;
        }
    }
    
    /// <summary>
    /// 设置交互模式
    /// </summary>
    public void SetInteractionMode(CardInteractionMode mode)
    {
        if (interactionMode == mode) return;
        
        interactionMode = mode;
        UpdateInteractionMode();
    }
    
    /// <summary>
    /// 更新交互模式组件
    /// </summary>
    private void UpdateInteractionMode()
    {
        switch (interactionMode)
        {
            case CardInteractionMode.Click:
                // 移除拖动组件
                if (dragHandler != null)
                {
                    Destroy(dragHandler);
                    dragHandler = null;
                }
                break;
                
            case CardInteractionMode.Drag:
                // 添加拖动组件
                if (dragHandler == null)
                {
                    dragHandler = gameObject.AddComponent<CardDragHandler>();
                }
                break;
        }
    }
    
    /// <summary>
    /// 获取当前交互模式
    /// </summary>
    public CardInteractionMode GetInteractionMode() => interactionMode;
}