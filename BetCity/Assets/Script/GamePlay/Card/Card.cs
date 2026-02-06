 using BetCity.Data.ConfigModels;
using BetCity.GamePlay.CardOrg;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Card
{
    public interface IModifyCard
    {
        bool OwnCardById(int id, out Card card, out string errorMsg);
        bool LoseCardById(int id, out Card card, out string errorMsg);
    }

    public class Card
    {
        private readonly CardData cardData;

        // 原型映射（只读）
        public int Id => cardData.Id;
        public string CardName => cardData.CardName;
        public string Description => cardData.Description;
        public int ArtworkID => cardData.ArtworkID;
        public Sprite Image => cardData.Image;
        public Data.ConfigModels.CardSeries Series => cardData.Series;
        public CardType Type => cardData.Type;
        public List<EffectConfig> Effects => cardData.Effects;

        // 实例可变状态（可被 CardManager 修改）
        public int Price { get; set; }
        public CardOwner Owner { get; private set; }
        public bool IsOwned { get; private set; }
        public bool IsInDeck { get; private set; }   // 对应“在背包/牌堆”的状态
        public Dictionary<string, object> ExtraData { get; set; } = new Dictionary<string, object>();

        // 从原型创建（未拥有）
        public Card(CardData cardData)
        {
            this.cardData = cardData;
            this.Price = cardData.Price;
            this.IsOwned = false;
            this.IsInDeck = false;
        }

        // 从存档/外部创建（保留自定义价格/额外数据/状态）
        public Card(CardData cardData, Dictionary<string, object> extraData, int customPrice, bool isInDeck, bool isOwned = false, CardOwner owner = CardOwner.None)
        {
            this.cardData = cardData;
            this.Price = customPrice;
            this.IsOwned = isOwned;
            this.IsInDeck = isInDeck;
            this.Owner = owner;
            this.ExtraData = extraData == null ? null : extraData;
        }

        // 只允许 CardManager 通过 IModifyCard 修改状态
        public void SetIsOwned(bool isOwned, IModifyCard caller)
        {
            if (caller is not CardManager) throw new InvalidOperationException("仅 CardManager 可以修改 Card 的 IsOwned");
            this.IsOwned = isOwned;
        }

        public void SetIsInDeck(bool isInDeck, IModifyCard caller)
        {
            if (caller is not CardManager) throw new InvalidOperationException("仅 CardManager 可以修改 Card 的 IsInDeck");
            this.IsInDeck = isInDeck;
        }

        public void SetOwner(CardOwner owner, IModifyCard caller)
        {
            if (caller is not CardManager) throw new InvalidOperationException("仅 CardManager 可以修改 Card 的 Owner");
            this.Owner = owner;
        }

        public void SetPrice(int price, IModifyCard caller)
        {
            if (caller is not CardManager) throw new InvalidOperationException("仅 CardManager 可以修改 Card 的 Price");
            this.Price = price;
        }
    }
}