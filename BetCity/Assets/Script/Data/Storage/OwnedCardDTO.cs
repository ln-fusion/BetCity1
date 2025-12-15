using BetCity.GamePlay.CardOrg;
using Newtonsoft.Json;
using System;

namespace BetCity.Storage
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
        /// 卡牌所有者
        /// </summary>
        public CardOwner Owner { get; }

        /// <summary>
        /// 是否激活（仅怪兽卡）
        /// </summary>
        public bool IsActive { get; }

        [JsonConstructor]
        public OwnedCardDTO(int id, CardOwner owner, bool isActive)
        {
            Id = id;
            Owner = owner;
            IsActive = isActive;
        }
    }
}