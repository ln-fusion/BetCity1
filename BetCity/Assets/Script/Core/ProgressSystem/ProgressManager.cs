using BetCity.Core.Tools;
using BetCity.Data.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BetCity.Core.ProgressSystem
{
    /// <summary>
    /// 玩家进度管理器
    /// </summary>
    public class ProgressManager : MonoSingleton<ProgressManager>, ISubmitArchive<ArchiveProgressDTO>
    {
        //事件Id-事件已触发次数
        private Dictionary<int, int> eventProgress;
        //零散数据存储字典
        private Dictionary<string, object> keyValuePairs;

        /// <summary>
        /// 事件Id-事件已触发次数
        /// </summary>
        public IReadOnlyDictionary<int, int> EventProgress => eventProgress;

        protected override void Awake()
        {
            base.Awake();
            ArchiveProgressDTO dto = StorageManager.Instance.ArchiveDataContainer.ArchiveProgressDTO;
            eventProgress = dto?.EventProgress == null ? new Dictionary<int, int>() : dto?.EventProgress;
            keyValuePairs = dto?.KeyValuePairs == null ? new Dictionary<string, object>() : dto?.KeyValuePairs;
        }

        //保存存档
        private void SaveArchive()
        {
            List<ArchiveProgressDTO> t = new List<ArchiveProgressDTO>();
            ArchiveProgressDTO dto = new(eventProgress, keyValuePairs);
            t.Add(dto);
            SubmitArchive(t);
        }

        #region 接口
        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
        }

        /// <summary>
        /// 进入指定Id Event次数+1，默认Trigger事件函数里会调用
        /// </summary>
        public void EnterEvent(int id)
        {
            if (eventProgress.ContainsKey(id))
            {
                eventProgress[id]++;
            }
            else eventProgress[id] = 1;
        }

        /// <summary>
        /// 写入零散数据
        /// </summary>
        public void SetKVData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[ProgressManager]存储数据失败：键不能为空！");
                return;
            }
            keyValuePairs[key] = value;
        }
        
        /// <summary>
        /// 读取零散数据,读取失败返回false并赋值默认值
        /// </summary>
        public bool GetKVData<T>(string key, out T value)
        {
            if (!keyValuePairs.ContainsKey(key))
            {
                value = default(T);
                return false;
            }
            try
            {
                value = (T)Convert.ChangeType(keyValuePairs[key], typeof(T));
            }
            catch 
            {
                Debug.LogError($"[ProgressManager]类型不匹配!");
                value = default(T);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 上传对应的存档
        /// </summary>
        public void SubmitArchive(List<ArchiveProgressDTO> t)
        {
            StorageManager.Instance.ModifyArchive(t, this);
        }
        #endregion
    }
}
