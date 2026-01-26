using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 当前任务进度信息
    /// </summary>
    public class TaskData : MonoBehaviour
    {
        private readonly TaskConfig taskConfig;
        /// <summary>
        /// 任务Id
        /// </summary>
        public int Id => taskConfig.Id;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name => taskConfig.Name;
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description => taskConfig.Description;
        /// <summary>
        /// 是否是主线任务
        /// </summary>
        public bool IsMain => taskConfig.IsMain;
        /// <summary>
        /// 任务阶段存储
        /// </summary>
        public List<TaskPhaseData> Phases => taskConfig.Phases;
        /// <summary>
        /// 任务奖励（任务完成后获得的所有奖励）
        /// </summary>
        public List<TaskRewardData> Rewards => taskConfig.Rewards;
        /// <summary>
        /// 当前任务进行阶段索引
        /// </summary>
        public int CurrentPhaseIndex { get; private set; }
        /// <summary>
        /// 当前任务阶段达成的数量
        /// </summary>
        public int CurrentPhaseCurrentCount {  get; private set; }
        

        public TaskData()
        {
            CurrentPhaseIndex = 0;
        }

        public TaskData(int currentPhaseIndex, int currentPhaseCurrentCount )
        {
            CurrentPhaseIndex = currentPhaseIndex;
            CurrentPhaseCurrentCount = currentPhaseCurrentCount;
        }
    }
}
