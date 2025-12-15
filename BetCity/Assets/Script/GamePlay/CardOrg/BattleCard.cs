using BetCity.GamePlay.CardOrg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BattleCardState
{
    inHand, inBlock, inTemp, inGrave
}

public class BattleCard : MonoBehaviour, IPointerDownHandler
{
    public CardOwner playerOwner; 
    public BattleCardState state = BattleCardState.inHand;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 检查是否是怪物卡且处于临时区域
        if (GetComponent<CardDisplay>()?.card is MonsterCard)
        {
            if (state == BattleCardState.inTemp)
            {
                CombatManager.Instance.SummonRequest(playerOwner, gameObject);
            }
            else
            {
                Debug.Log($"卡牌状态为{state}，无法召唤");
            }
        }
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
}