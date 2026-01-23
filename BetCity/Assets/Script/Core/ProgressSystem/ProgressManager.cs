using BetCity.Core.Tools;
using BetCity.Data.Storage;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// 事件Id-事件已触发次数
        /// </summary>
        public IReadOnlyDictionary<int, int> EventProgress => eventProgress;

        protected override void Awake()
        {
            base.Awake();
            eventProgress = StorageManager.Instance.ArchiveDataContainer.ArchiveProgressDTO.EventProgress;
        }

        //保存存档
        private void SaveArchive()
        {
            List<ArchiveProgressDTO> t = new List<ArchiveProgressDTO>();
            ArchiveProgressDTO dto = new(eventProgress);
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
        /// 上传对应的存档
        /// </summary>
        public void SubmitArchive(List<ArchiveProgressDTO> t)
        {
            StorageManager.Instance.ModifyArchive(t, this);
        }
        #endregion
    }
}
