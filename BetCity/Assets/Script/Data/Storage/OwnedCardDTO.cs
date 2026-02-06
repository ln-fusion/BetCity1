using BetCity.GamePlay.CardOrg;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 存储玩家拥有的卡牌信息
    /// </summary>
    [Serializable]
    public class OwnedCardDTO
    {
        /// <summary>
        /// 关联原型ID
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// 玩家修改后的价格（无修改则等于原型）
        /// </summary>
        public int CustomPrice { get; }
        /// <summary>
        /// 卡牌所有者
        /// </summary>
        public CardOwner Owner { get; }
        /// <summary>
        /// 是否在背包中
        /// </summary>
        public bool IsInDeck { get; }
        /// <summary>
        /// 额外信息，注意序列化后的内容需要强转！
        /// </summary>
        public Dictionary<string, object> ExtraData { get; }

        [JsonConstructor]
        public OwnedCardDTO(int id, CardOwner owner, int customPrice, bool isInBag, Dictionary<string, object> extraData)
        {
            Id = id;
            Owner = owner;
            CustomPrice = customPrice;
            IsInDeck = isInBag;
            ExtraData = extraData;
        }
    }
}