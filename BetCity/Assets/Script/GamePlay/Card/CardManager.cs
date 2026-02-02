using System;
using System.Collections.Generic;
using System.Linq;
using BetCity.Core.Tools;
using BetCity.Card;
using BetCity.Data.Storage;
using BetCity.Data.ConfigModels;

namespace BetCity.Card
{
    public class CardManager : MonoSingleton<CardManager>, ISubmitArchive<OwnedCardDTO>, IModifyCard
    {
        private CardDataManager CardDataManager => CardDataManager.Instance;
        private StorageManager StorageManager => StorageManager.Instance;

        // 所有原型（只读）
        private IReadOnlyList<CardData> AllCardDatas => CardDataManager.Data;

        // 已拥有实例（按 id 映射）
        private Dictionary<int, Card> ownedCards = new Dictionary<int, Card>();

        // 图书馆/牌组
        private List<int> deckCards = new List<int>();
        private List<int> libraryCards = new List<int>();
        private List<int> notOwnedCards = new List<int>();

        protected override void Awake()
        {
            base.Awake();
            CacheOwnedCardInstances();
            LoadNotOwnedCards();
        }

        private void LoadNotOwnedCards()
        {
            notOwnedCards = AllCardDatas.Where(d => !ownedCards.ContainsKey(d.Id)).Select(d => d.Id).ToList();
        }

        private void CacheOwnedCardInstances()
        {
            ownedCards.Clear();
            var dtos = StorageManager.ArchiveDataContainer.OwnedCardDTOs;
            if (dtos == null) return;

            foreach (var dto in dtos)
            {
                var cardData = CardDataManager.GetDataById(dto.Id);
                if (cardData == null)
                {
                    UnityEngine.Debug.LogError($"发现不存在的卡牌，非法Id为：{dto.Id}");
                    continue;
                }

                // OwnedCardDTO 构造顺序: (id, owner, customPrice, isInBag, extraData)
                Card card = new Card(cardData, dto.ExtraData, dto.CustomPrice, dto.IsInBag, true, dto.Owner);

                ownedCards.Add(dto.Id, card);
                if (dto.IsInBag) deckCards.Add(dto.Id); else libraryCards.Add(dto.Id);
            }
        }

        // 对外接口：以 id 拥有卡牌（遵循 IModifyCard 签名）
        public bool OwnCardById(int id, out Card card, out string errorMsg)
        {
            card = null;
            errorMsg = string.Empty;

            if (ownedCards.ContainsKey(id))
            {
                card = ownedCards[id];
                errorMsg = $"已拥有ID为{id}的卡牌";
                return false;
            }

            var cardData = CardDataManager.GetDataById(id);
            if (cardData == null)
            {
                errorMsg = $"未找到ID为{id}的卡牌原型";
                return false;
            }

            // 从原型创建实例
            card = new Card(cardData);
            ownedCards[id] = card;

            // 默认放入仓库（或根据规则放入 deck）
            libraryCards.Add(id);

            // 标记为拥有
            card.SetIsOwned(true, this);

            // 更新未拥有列表、存档
            notOwnedCards.Remove(id);
            SubmitArchiveToStorage();
            return true;
        }

        // 对外接口：失去卡牌
        public bool LoseCardById(int id, out Card card, out string errorMsg)
        {
            card = null;
            errorMsg = string.Empty;

            if (!ownedCards.ContainsKey(id))
            {
                errorMsg = $"未拥有ID为{id}的卡牌";
                return false;
            }

            card = ownedCards[id];

            // 从集合移除并更新状态
            ownedCards.Remove(id);
            deckCards.Remove(id);
            libraryCards.Remove(id);
            card.SetIsOwned(false, this);

            notOwnedCards.Add(id);
            SubmitArchiveToStorage();
            return true;
        }

        public bool IsOwned(int id) => ownedCards.ContainsKey(id);

        public Card GetOwnedCardById(int id)
        {
            if (ownedCards.TryGetValue(id, out var c)) return c;
            return null;
        }

        private void SubmitArchiveToStorage()
        {
            var dtos = ownedCards.Select(kv => new OwnedCardDTO(
                kv.Key,
                kv.Value.Owner,
                kv.Value.Price,
                kv.Value.IsInDeck,
                kv.Value.ExtraData
            )).ToList();

            SubmitArchive(dtos);
        }

        public void SubmitArchive(List<OwnedCardDTO> t)
        {
            StorageManager.ModifyArchive(t, this);
        }
    }
}