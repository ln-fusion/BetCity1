using BetCity.Core.Tools;
using BetCity.Data.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.GamePlay.Task 
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskManager : MonoSingleton<TaskManager>, ISubmitArchive<AcceptedTaskDTO>
    {
        private IReadOnlyList<AcceptedTaskDTO> AcceptedTaskDTOs => StorageManager.Instance.ArchiveDataContainer.AcceptedTaskDTOs; 
        private List<TaskData> taskData = new List<TaskData>();
        /// <summary>
        /// 已接任务数据
        /// </summary>
        public IReadOnlyList<TaskData> TaskData => taskData;

        protected override void Awake()
        {
            base.Awake();
            LoadTaskData();
        }

        //加载存档数据
        private void LoadTaskData()
        {
            foreach(var acceptedTaskDTO in AcceptedTaskDTOs)
            {
                taskData.Add(new TaskData(acceptedTaskDTO.CurrentPhaseIndex, acceptedTaskDTO.CurrentPhaseCurrentCount));
            }
        }


        #region 接口
        /// <summary>
        /// 提交存档
        /// </summary>
        public void SubmitArchive(List<AcceptedTaskDTO> t)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
