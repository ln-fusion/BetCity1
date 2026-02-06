using BetCity.Core.ActionSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.Data.Storage;
using BetCity.GamePlay.Explorer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BetCity.GamePlay.Task 
{
    /// <summary>
    /// 任务管理器,负责玩家当前任务生命周期管理,所有任务原型加载
    /// </summary>
    public class TaskManager : MonoSingleton<TaskManager>, ISubmitArchive<AcceptedTaskDTO>
    {
        private IReadOnlyList<AcceptedTaskDTO> AcceptedTaskDTOs => StorageManager.Instance.ArchiveDataContainer.AcceptedTaskDTOs; 
        private List<TaskData> taskDataList = new List<TaskData>();
        private Dictionary<int, TaskConfig> taskConfigs = new Dictionary<int, TaskConfig>();
        /// <summary>
        /// 已接任务数据
        /// </summary>
        public IReadOnlyList<TaskData> TaskDataList => taskDataList;
        /// <summary>
        /// 所有任务原型数据
        /// </summary>
        public IReadOnlyDictionary<int, TaskConfig> TaskConfigs => taskConfigs; 
        /// <summary>
        /// 纪念品资源路径
        /// </summary>
        public const string TASK_CONFIG_RESOURCES_PATH = "Task";

        protected override void Awake()
        {
            base.Awake();
            LoadTaskConfig();
            LoadTaskData();
        }

        /// <summary>
        /// 暂时用来保存的地方
        /// </summary>
        private void OnDestroy()
        {
            ManualSave(); 
        }

        //加载存档数据
        private void LoadTaskData()
        {
            foreach(var acceptedTaskDTO in AcceptedTaskDTOs)
            {
                taskDataList.Add(new TaskData(taskConfigs[acceptedTaskDTO.Id], acceptedTaskDTO.CurrentPhaseIndex, acceptedTaskDTO.CurrentPhaseCurrentCounts));
            }
        }

        //加载任务原型数据
        private void LoadTaskConfig()
        {
            try
            {
                TaskConfig[] configs = Resources.LoadAll<TaskConfig>(TASK_CONFIG_RESOURCES_PATH);

                if(configs == null || configs.Length == 0)
                {
                    Debug.LogWarning($"[TaskManager] 未在Resources/{TASK_CONFIG_RESOURCES_PATH}路径下找到任何SouvenirData资源");
                    return;
                }
                foreach (var config in configs)
                {
                    taskConfigs[config.Id] = config;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskManager] 加载数据失败：{e.Message}\n{e.StackTrace}");
            }
        }

        //保存存档
        private void SaveArchive()
        {
            List<AcceptedTaskDTO> acceptedTaskDTOs = new List<AcceptedTaskDTO>();
            foreach (var taskdata in taskDataList)
            {
                acceptedTaskDTOs.Add(new AcceptedTaskDTO(taskdata.Id, taskdata.CurrentPhaseIndex, taskdata.CurrentPhaseCurrentCounts));
            }
            SubmitArchive(acceptedTaskDTOs);
        }

        private void GetTaskReward(TaskData taskData)
        {
            if (taskData == null || taskData.Rewards == null || taskData.Rewards.Count == 0)
            {
                Debug.LogWarning($"[TaskManager] 任务[{taskData?.Id}]无奖励可发放");
                return;
            }

            foreach (var reward in taskData.Rewards)
            {
                switch (reward.RewardType)
                {
                    case RewardType.Coin:
                        CoinChangeAction coinChangeAction = new(new GameActionContext(this, ExplorerPlayerController.Instance.PlayerData, null), reward.RewardCount);
                        ActionManager.Instance.Perform(coinChangeAction);
                        break;
                    case RewardType.Souvenir:
                        throw new NotImplementedException();
                    case RewardType.Card:
                        throw new NotImplementedException();
                }
            }

        }

        #region 接口
        /// <summary>
        /// 提交存档
        /// </summary>
        public void SubmitArchive(List<AcceptedTaskDTO> t)
        {
            StorageManager.Instance.ModifyArchive<AcceptedTaskDTO>(t, this);
        }

        /// <summary>
        /// 接受任务,订阅第一阶段目标
        /// </summary>
        public bool AcceptTask(int id)
        {
            if (!taskConfigs.ContainsKey(id))
            {
                Debug.LogError($"[TaskManager]找不到该Id{id}所对应任务！");
                return false;
            }
            else if (taskDataList.Find(t => t.Id == id) != null)
            {
                Debug.LogError($"[TaskManager]该id:{id}所定义任务正在执行！");
                return false;
            }
            else if (ProgressManager.Instance.TaskProgress.Contains(id))
            {
                Debug.LogError($"[TaskManager]该id:{id}所定义任务已完成！");
                return false;
            }

            TaskData taskData = new TaskData(taskConfigs[id]);
            taskDataList.Add(taskData);
            taskData.SubscribeCurrentPhaseAction();
            return true;
        }

        /// <summary>
        /// 完成任务,获取任务奖励
        /// </summary>
        public bool FinishTask(int id)
        {
            TaskData taskData = taskDataList.Find(t => t.Id == id);
            if(taskData == null)
            {
                Debug.LogError($"[TaskManager]该id:{id}不在任务列表里！");
                return false;
            }
            else if (!taskData.IsTaskFinished())
            {
                Debug.LogError($"[TaskManager]该id:{id}任务尚未完成！");
                return false;
            }
            GetTaskReward(taskData);
            ProgressManager.Instance.TaskProgress.Add(id);
            taskDataList.Remove(taskData);
            return true;
        }

        /// <summary>
        /// 保存接口
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
        }
        #endregion
    }
}
