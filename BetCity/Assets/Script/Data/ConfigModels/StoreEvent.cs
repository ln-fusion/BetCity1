using BetCity.Core.CheckSystem;
using BetCity.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 商店事件
    /// </summary>
    [CreateAssetMenu(fileName = "Event", menuName = "Event/StoreEvent")]
    public class StoreEvent : BaseEvent
    {
        /// <summary>
        /// 商品列表
        /// </summary>
        [field: SerializeField] public List<Product> Products { get; private set; }
        /// <summary>
        /// 一次上架卡牌品数量
        /// </summary>
        [field: SerializeField] public int CardAmount { get; private set; }
        /// <summary>
        /// 一次上架纪念品数量
        /// </summary>
        [field: SerializeField] public int SouvenirAmount { get; private set; }
        /// <summary>
        /// 理智购买商品数量
        /// </summary>
        [field: SerializeField] public int SanityPurchaseAmount {  get; private set; }
    }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Serializable]
    public class Product
    {
        /// <summary>
        /// id，与对应类别的id相关联
        /// </summary>
        [field: SerializeField] public int ProductId { get; private set; }
        /// <summary>
        /// 物品种类
        /// </summary>
        [field: SerializeField] public ItemType ItemType { get; private set; }
        /// <summary>
        /// 刷新权重
        /// </summary>
        [field: SerializeField] public int Weight { get; private set; }
        /// <summary>
        /// 商品出现条件
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, List<string>> Conditions { get; private set; }
        /// <summary>
        /// 用理智的价格
        /// </summary>
        [field: SerializeField] public int SanityPrice { get; private set; }
    }
}
