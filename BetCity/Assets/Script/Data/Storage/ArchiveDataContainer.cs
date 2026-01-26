using BetCity.Data.Storage;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
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
    public class ArchiveDataContainer
    {
        /// <summary>
        /// 玩家纪念品数据
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<OwnedSouvenirDTO> OwnedSouvenirDTOs => ownedSouvenirDTOs;
        [SerializeField, JsonProperty("OwnedSouvenirDTOs")]
        private List<OwnedSouvenirDTO> ownedSouvenirDTOs = new List<OwnedSouvenirDTO>();

        /// <summary>
        /// 玩家数据
        /// </summary>
        public PlayerDataDTO PlayerDataDTO { get; private set; }

        /// <summary>
        /// 玩家进度数据
        /// </summary>
        public ArchiveProgressDTO ArchiveProgressDTO { get; private set; }

        /// <summary>
        /// 玩家拥有的卡牌数据
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<OwnedCardDTO> OwnedCardDTOs => ownedCardDTOs;
        [SerializeField, JsonProperty("OwnedCardDTOs")]
        private List<OwnedCardDTO> ownedCardDTOs = new List<OwnedCardDTO>();

        [JsonIgnore]
        public IReadOnlyList<AcceptedTaskDTO> AcceptedTaskDTOs => acceptedTaskDTOs;
        [SerializeField, JsonProperty("AcceptedTaskDTOs")]
        private List<AcceptedTaskDTO> acceptedTaskDTOs = new List<AcceptedTaskDTO>();

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

        /// <summary>
        /// 修改玩家信息
        /// </summary>
        public void ModifyExplorerPlayerData(List<PlayerDataDTO> playerDataDTO, IModifyArchive caller)
        {
            if (caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.PlayerDataDTO = playerDataDTO[0];
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

        /// <summary>
        /// 修改存档进度信息
        /// </summary>
        public void ModifyArchiveProgress(List<ArchiveProgressDTO> archiveProgressDTO, IModifyArchive caller)
        {
            if (caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.ArchiveProgressDTO = archiveProgressDTO[0];
        }

        /// <summary>
        /// 修改玩家接受任务列表
        /// </summary>
        public void ModifyAcceptedTask(List<AcceptedTaskDTO> acceptedTaskDTOs, IModifyArchive caller)
        {
            if (caller is not StorageManager)
            {
                throw new InvalidOperationException("仅StorageManager类可修改存档信息");
            }
            this.acceptedTaskDTOs = acceptedTaskDTOs;
        }
    }
}
