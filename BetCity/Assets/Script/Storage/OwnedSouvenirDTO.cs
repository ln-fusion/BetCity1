using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Storage
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

        public OwnedSouvenirDTO() { }

        [JsonConstructor]
        public OwnedSouvenirDTO(int id, int customPrice)
        {
            Id = id;
            CustomPrice = customPrice;
        }
    }
}