using System;
using System.Collections.Generic;
using System.Linq;
using BetCity.Core.Tools;
using BetCity.Card;
using BetCity.Storage;
using BetCity.Data.ConfigModels;


namespace BetCity.Card
{
    public class CardManager : MonoSingleton<CardManager>, ISubmitArchive<OwnedCardDTO>
    {
        private List<Card> _ownedCards = new List<Card>();

        /// <summary>
        /// 初始化时从存档加载卡牌数据
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            LoadCardsFromArchive();
        }

        /// <summary>
        /// 从存档加载卡牌数据
        /// </summary>
        private void LoadCardsFromArchive()
        {
            var dtos = StorageManager.Instance.ArchiveData.OwnedCardDTOs;
            foreach (var dto in dtos)
            {
                var cardData = CardDataManager.Instance.GetDataById(dto.Id);
                if (cardData != null)
                {
                    var card = new Card(cardData, true, dto.Owner)
                    {
                        //  实际应通过SetCardState设置
                    };
                    _ownedCards.Add(card);
                }
            }
        }

        /// <summary>
        /// 获得卡牌
        /// </summary>
        public bool OwnCardById(int id, out Card card, out string errorMsg)
        {
            errorMsg = string.Empty;
            card = null;

            var cardData = CardDataManager.Instance.GetDataById(id);
            if (cardData == null)
            {
                errorMsg = $"未找到ID为{id}的卡牌原型";
                return false;
            }

            if (_ownedCards.Exists(c => c.Id == id))
            {
                errorMsg = $"已拥有ID为{id}的卡牌";
                return false;
            }

            card = new Card(cardData, true);
            _ownedCards.Add(card);
            SubmitArchiveToStorage();
            return true;
        }

        /// <summary>
        /// 失去卡牌
        /// </summary>
        public bool LoseCardById(int id, out Card card, out string errorMsg)
        {
            errorMsg = string.Empty;
            card = _ownedCards.Find(c => c.Id == id);

            if (card == null)
            {
                errorMsg = $"未拥有ID为{id}的卡牌";
                return false;
            }

            _ownedCards.Remove(card);
            SubmitArchiveToStorage();
            return true;
        }

        /// <summary>
        /// 提交存档
        /// </summary>
        public void SubmitArchive(List<OwnedCardDTO> t)
        {
            StorageManager.Instance.ModifyArchive(t, this);
        }

        /// <summary>
        /// 转换为DTO并提交存档
        /// </summary>
        private void SubmitArchiveToStorage()
        {
            var dtos = _ownedCards.Select(c => new OwnedCardDTO(
                c.Id,
                c.Owner,
                c.IsActive
            )).ToList();

            SubmitArchive(dtos);
        }
    }
}
    