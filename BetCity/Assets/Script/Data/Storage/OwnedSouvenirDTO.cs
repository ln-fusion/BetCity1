using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 存储玩家拥有的纪念品
    /// </summary>
    [Serializable]
    public class OwnedSouvenirDTO
    {
        /// <summary>
        /// 关联原型ID
        /// </summary>
        public int Id { get;}   
        /// <summary>
        /// 玩家修改后的价格（无修改则等于原型）
        /// </summary>
        public int CustomPrice { get;}
        /// <summary>
        /// 是否在背包中
        /// </summary>
        public bool IsInBag { get;}
        /// <summary>
        /// 额外信息，注意序列化后的内容需要强转！
        /// </summary>
        public Dictionary<string, object> ExtraData { get;}

        public OwnedSouvenirDTO() { }

        [JsonConstructor]
        public OwnedSouvenirDTO(int id, int customPrice, bool isInBag, Dictionary<string, object> extraData)
        {
            Id = id;
            CustomPrice = customPrice;
            IsInBag = isInBag;
            ExtraData = extraData;
        }
    }
}