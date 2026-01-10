using BetCity.Data.ConfigModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace BetCity.GamePlay.Souvenir
{
    /// <summary>
    /// 允许修改Souvenir的接口（目前设计除SouvenirManger应无人继承）
    /// </summary>
    public interface IModifySouvenir
    {
        bool LoseSouvenirById(int id, out Souvenir souvenir, out string errorMsg);
        bool OwnSouvenirById(int id, out Souvenir souvenir, out string errorMsg);
    }

    /// <summary>
    /// 对应的纪念品包装
    /// </summary>
    public class Souvenir
    {
        private readonly SouvenirData souvenirData;

        /// <summary>
        /// id
        /// </summary>
        public int Id => souvenirData.Id;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => souvenirData.Name;
        /// <summary>
        /// 信息
        /// </summary>
        public string Info => souvenirData.Info;
        /// <summary>
        /// 美术Id
        /// </summary>
        public int ArtworkID => souvenirData.ArtworkID;
        /// <summary>
        /// 精灵
        /// </summary>
        public Sprite Image => souvenirData.Image;
        /// <summary>
        /// 槽数
        /// </summary>
        public int Slot => souvenirData.Slot;
        /// <summary>
        /// 类别
        /// </summary>
        public SouvenirCategory Category => souvenirData.Category;
        /// <summary>
        /// 稀有度
        /// </summary>
        public SouvenirQuality Quality => souvenirData.Quality;
        /// <summary>
        /// 价格（可被修改）
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// 玩家是否拥有？
        /// </summary>
        public bool IsOwned { get; private set; }
        /// <summary>
        /// 是否在背包中
        /// </summary>
        public bool IsInBag { get; private set; }
        /// <summary>
        /// 效果配置
        /// </summary>
        public List<EffectConfig> Effects => souvenirData.Effects;
        /// <summary>
        /// 额外信息
        /// </summary>
        public Dictionary<string, object> ExtraData { get; set; } = new Dictionary<string, object>();

        public Souvenir(SouvenirData souvenirData, Dictionary<string, object> extraData, int price, bool isInBag, bool isOwned = false)
        {
            this.souvenirData = souvenirData;
            this.Price = price;
            this.IsOwned = isOwned;
            this.ExtraData = extraData == null ? null : extraData;
        }

        public Souvenir(SouvenirData souvenirData)
        {
            this.souvenirData = souvenirData;
            this.Price = souvenirData.Price;
            this.IsOwned = false;
        }

        /// <summary>
        /// 修改IsOwned属性，只限实现IModifySouvenir的SouvenirManger访问
        /// <param name="caller">允许修改Souvenir的接口</param>
        /// </summary>
        public void SetIsOwned(bool isOwned, IModifySouvenir caller)
        {
            // 关键：校验调用者必须是SouvenirManager的实例（防止其他类伪造接口）
            if (caller is not SouvenirManager)
            {
                throw new InvalidOperationException("仅SouvenirManager类可修改Souvenir的Price属性");
            }
            this.IsOwned = isOwned;
        }

        /// <summary>
        /// 修改价格属性，只限实现IModifySouvenir的SouvenirManger访问
        /// <param name="caller">允许修改Souvenir的接口</param>
        /// </summary>
        public void SetPrice(bool isOwned, IModifySouvenir caller)
        {
            // 关键：校验调用者必须是SouvenirManager的实例（防止其他类伪造接口）
            if (caller is not SouvenirManager)
            {
                throw new InvalidOperationException("仅SouvenirManager类可修改Souvenir的Price属性");
            }
        }
    }

}

