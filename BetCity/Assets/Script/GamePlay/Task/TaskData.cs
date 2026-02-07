using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 当前任务进度信息
    /// </summary>
    public class TaskData
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
        /// 完成任务时的奖励提示文本
        /// </summary>
        public string FinishTaskDescription => taskConfig.FinishTaskDescription;
        /// <summary>
        /// 当前任务进行阶段索引
        /// </summary>
        public int CurrentPhaseIndex { get; private set; }
        /// <summary>
        /// 目标ID（NPCID/怪物ID/物品ID）- 完成目标所需要的数量
        /// </summary>
        public Dictionary<int, int> TargetIdAndCount => Phases[CurrentPhaseIndex].TargetIdAndCount;
        /// <summary>
        /// 当前任务阶段达成的数量
        /// </summary>
        public Dictionary<int, int> CurrentPhaseCurrentCounts {  get; private set; }
        
        public TaskData(TaskConfig taskConfig)
        {
            this.taskConfig = taskConfig;
            CurrentPhaseIndex = 0;
            CurrentPhaseCurrentCounts = TargetIdAndCount.ToDictionary(
            kvp => kvp.Key, 
            kvp => 0         
            );
        }

        public TaskData(TaskConfig taskConfig, int currentPhaseIndex, Dictionary<int, int> currentPhaseCurrentCounts )
        {
            this.taskConfig = taskConfig; 
            CurrentPhaseIndex = currentPhaseIndex;
            CurrentPhaseCurrentCounts = currentPhaseCurrentCounts;
        }

        /// <summary>
        /// 增加当前阶段的完成数量,完成则推进到下一阶段
        /// </summary>
        /// <param name="addCount">增加的数量</param>
        /// <returns>是否完成当前阶段</returns>
        public bool AddPhaseCount(int id, int addCount = 1)
        {
            var currentPhase = Phases[CurrentPhaseIndex];
            CurrentPhaseCurrentCounts[id] += addCount;

            // 判断当前阶段是否完成
            bool isPhaseFinished = CurrentPhaseCurrentCounts.Keys.Intersect(TargetIdAndCount.Keys)
               .All(key => CurrentPhaseCurrentCounts[key] >= TargetIdAndCount[key]);

            if (isPhaseFinished)
            {
                CurrentPhaseIndex++;
                // 如果还有下一阶段
                if (Phases.Count > CurrentPhaseIndex + 1)
                {
                    CurrentPhaseCurrentCounts = TargetIdAndCount.ToDictionary(
                    kvp => kvp.Key,
                    kvp => 0
                    );
                }
            }
            return isPhaseFinished;
        }

        /// <summary>
        /// 判断任务是否全部完成
        /// </summary>
        public bool IsTaskFinished()
        {
            return CurrentPhaseIndex >= Phases.Count;
        }

        /// <summary>
        /// 订阅当前目标
        /// </summary>
        public void SubscribeCurrentPhaseAction()
        {
            TaskPhaseData taskPhaseData = Phases[CurrentPhaseIndex];

            throw new NotImplementedException();
        }

        public void UnsubscribreCurrentPhaseAction()
        {
            throw new NotImplementedException();
        }
    }
}
