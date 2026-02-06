using BetCity.Card;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.Data.Storage;
using BetCity.GamePlay.Souvenir;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        [SerializeField]private GameObject cardViewPrefab;

        protected override void Awake()
        {
            base.Awake();
            CacheOwnedCardInstances();
            LoadNotOwnedCards();
        }
        /// <summary>
        /// 取消注册
        /// </summary>
        private void OnDisable()
        {
            foreach (var id in deckCards)
            {
                UnregisterEffect(ownedCards[id]);
            }
            foreach (var id in libraryCards)
            {
                UnregisterEffect(ownedCards[id]);
            }
        }
        private void LoadNotOwnedCards()
        {
            // Guard against CardDataManager not initialized yet
            if (CardDataManager == null || CardDataManager.Data == null)
            {
                UnityEngine.Debug.LogWarning("[CardManager] CardDataManager not ready when loading notOwnedCards.");
                notOwnedCards = new List<int>();
                return;
            }
            notOwnedCards = AllCardDatas.Where(d => !ownedCards.ContainsKey(d.Id)).Select(d => d.Id).ToList();
        }

        private void CacheOwnedCardInstances()
        {
            ownedCards.Clear();
            // Guard against StorageManager or archive not ready
            var storage = StorageManager;
            if (storage == null || storage.ArchiveDataContainer == null)
            {
                UnityEngine.Debug.LogWarning("[CardManager] StorageManager or ArchiveDataContainer is null when caching owned cards.");
                return;
            }
            var dtos = storage.ArchiveDataContainer.OwnedCardDTOs;
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
                Card card = new Card(cardData, dto.ExtraData, dto.CustomPrice, dto.IsInDeck, true, dto.Owner);

                ownedCards.Add(dto.Id, card);
                if (dto.IsInDeck) deckCards.Add(dto.Id); else libraryCards.Add(dto.Id);
            }
        }

        /// <summary>
        /// 注册效果
        /// </summary>
        private void RegisterEffect(Card card)
        {
            foreach (var effect in card.Effects)
            {
                if (effect.Lifetime != EffectLifetime.OneShot)
                {
                    effect.Activate();
                }
                effect.Source = card;
            }
        }
        /// <summary>
        /// 根据字典生成当前存档并提交
        /// </summary>
        private void SaveArchive()
        {
            List<OwnedCardDTO> saveData = new List<OwnedCardDTO>();
            foreach (var kv in ownedCards)
            {
                Card c = kv.Value;
                OwnedCardDTO dto = new OwnedCardDTO(kv.Key, c.Owner, c.Price, c.IsInDeck, c.ExtraData);
                saveData.Add(dto);
            }
            SubmitArchive(saveData);
        }
        private void UnregisterEffect(Card card)
        {
            foreach (var effect in card.Effects)
            {
                effect.Deactivate();
            }
        }

        // 对外接口：以 id 拥有卡牌（遵循 IModifyCard 签名）
        public void ManualSave()
        {
            SaveArchive();
        }
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
        public GameObject SpawnCardView(int cardId, Vector3 position, Transform parent = null)
        {
            // Guard against missing CardDataManager
            var dataMgr = CardDataManager;
            if (dataMgr == null)
            {
                UnityEngine.Debug.LogError("[CardManager] CardDataManager.Instance is null. Cannot spawn card view.");
                return null;
            }
            var data = dataMgr.GetDataById(cardId);
            if (data == null)
            {
                UnityEngine.Debug.LogError($"[CardManager] CardData with id={cardId} not found.");
                return null;
            }

            Card card = GetOwnedCardById(cardId) ?? new Card(data);

            if (cardViewPrefab == null)
            {
                UnityEngine.Debug.LogError("[CardManager] cardViewPrefab is not assigned in Inspector.");
                return null;
            }

            GameObject go = Instantiate(cardViewPrefab, parent);
            if (go == null)
            {
                UnityEngine.Debug.LogError("[CardManager] Instantiate returned null for cardViewPrefab.");
                return null;
            }
            go.transform.position = position;
            var view = go.GetComponent<CardView>();
            view?.Bind(card);

            return go;
        }
    }
}