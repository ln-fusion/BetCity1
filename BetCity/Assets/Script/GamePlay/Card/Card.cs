using BetCity.GamePlay.CardOrg;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BetCity.Data.ConfigModels;
/// <summary>
/// 允许修改Card的接口（仅限CardManager使用）
/// </summary>
namespace BetCity.Card
{
    public interface IModifyCard
    {
        bool OwnCardById(int id, out Card card, out string errorMsg);
        bool LoseCardById(int id, out Card card, out string errorMsg);
    }
    /// <summary>
    /// 卡牌实例类，包含原型数据引用和状态信息
    /// </summary>
    public class Card
    {
        private readonly CardData cardData;

        // 只读属性（映射原型数据）
        public int Id => cardData.Id;
        public string CardName => cardData.CardName;
        public string Description => cardData.Description;
        public int ArtworkID => cardData.ArtworkID;
        public UnityEngine.Sprite Image => cardData.Image;
        public Data.ConfigModels.CardSeries Series => cardData.Series;
        public CardType Type => cardData.Type;
        public int MonsterScore => cardData.MonsterScore; // 仅怪兽卡有效

        // 状态属性
        public CardOwner Owner { get; private set; }
        public bool IsActive { get; private set; } // 仅怪兽卡有效
        public bool IsOwned { get; private set; }

        public Card(CardData cardData, bool isOwned = false, CardOwner owner = CardOwner.None)
        {
            this.cardData = cardData;
            this.IsOwned = isOwned;
            this.Owner = owner;
            this.IsActive = false;
        }

        /// <summary>
        /// 修改卡牌状态，仅限实现IModifyCard的CardManager访问
        /// </summary>
        public void SetCardState(bool isOwned, CardOwner owner, bool isActive, IModifyCard caller)
        {
            if (caller is not CardManager)
            {
                throw new InvalidOperationException("仅CardManager类可修改Card的状态属性");
            }
            this.IsOwned = isOwned;
            this.Owner = owner;
            this.IsActive = isActive;
        }
    }
}

