using BetCity.Storage;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Storage
{
    /// <summary>
    /// 需要修改存档的Manager（如纪念品Manager）提交给StorageManager处理时需要继承此接口
    /// </summary>
    public interface ISubmitArchive<T>
    {
        /// <summary>
        /// 上传对应修改到StorageManager,StorageManager会根据对应类型调用函数
        /// </summary>
        void SubmitArchive(List<T> t);
    }

    /// <summary>
    /// 修改存档，StorageManager继承此接口
    /// </summary>
    public interface IModifyArchive
    {
        /// <summary>
        /// 修改存档
        /// </summary>
        public void ModifyArchive<T>(List<T> t, ISubmitArchive<T> caller);
    }

    /// <summary>
    /// 存档容器
    /// </summary>
    [Serializable]
    public class ArchiveContainer
    {
        /// <summary>
        /// 玩家纪念品数据
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<OwnedSouvenirDTO> OwnedSouvenirDTOs => ownedSouvenirDTOs;
        [SerializeField]
        [JsonProperty("OwnedSouvenirDTOs")]
        private List<OwnedSouvenirDTO> ownedSouvenirDTOs = new List<OwnedSouvenirDTO>();
        //[JsonIgnore]
        //public PlayerDTO PlayerDTO => playerDTO;
        //[SerializeField]
        //[JsonProperty("PlayerDTO")]
        public PlayerDTO PlayerDTO;
        /// <summary>
        /// 玩家拥有的卡牌数据
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<OwnedCardDTO> OwnedCardDTOs => ownedCardDTOs;
        [SerializeField]
        [JsonProperty("OwnedCardDTOs")]
        private List<OwnedCardDTO> ownedCardDTOs = new List<OwnedCardDTO>();
        /// <summary>
        /// 版本兼容
        /// </summary>
        public string SaveVersion { get; private set; } = "v0.1";

        /// <summary>
        /// 修改玩家拥有纪念品存档信息，仅供StorageManager使用
        /// </summary>
        /// <param name="ownedSouvenirDTOs">玩家拥有纪念品列表</param>
        /// <param name="caller">发起人</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ModifyOwnedSouvenir(List<OwnedSouvenirDTO> ownedSouvenirDTOs, IModifyArchive caller)
        {
            if(caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.ownedSouvenirDTOs = ownedSouvenirDTOs;
        }
        public void ModifyExplorerPlayerData(List<PlayerDTO> playerDTO, IModifyArchive caller)
        {
            if (caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.PlayerDTO = playerDTO[0];
        }
        /// <summary>
        /// 修改玩家拥有的卡牌存档信息
        /// </summary>
        public void ModifyOwnedCard(List<OwnedCardDTO> ownedCardDTOs, IModifyArchive caller)
        {
            if (caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.ownedCardDTOs = ownedCardDTOs;
        }
    }
}
