using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 任务配置
    /// </summary>
    [CreateAssetMenu(fileName = "Task", menuName = "Task/TaskConfig")]
    public class TaskConfig : ScriptableObject
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        [field:SerializeField] public int Id { get; private set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        [field: SerializeField] public string Name { get; private set; }
        /// <summary>
        /// 任务描述
        /// </summary>
        [field: SerializeField, TextArea] public string Description { get; private set; }
        /// <summary>
        /// 是否是主线任务
        /// </summary>
        [field: SerializeField] public bool IsMain { get; private set; }
        /// <summary>
        /// 任务出现条件
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, List<string>> Conditions { get; private set; }
        /// <summary>
        /// 任务阶段存储
        /// </summary>
        [field: SerializeField] public List<TaskPhaseData> Phases {  get; private set; }
        /// <summary>
        /// 完成任务时的奖励提示文本
        /// </summary>
        [field: SerializeField] public string FinishTaskDescription {  get; private set; }
        /// <summary>
        /// 任务奖励（任务完成后获得的所有奖励）
        /// </summary>
        [field: SerializeField] public List<TaskRewardData> Rewards { get; private set; }
    }

    /// <summary>
    /// 任务阶段数据（单个阶段的目标）
    /// </summary>
    [Serializable]
    public class TaskPhaseData
    {
        /// <summary>
        /// 阶段目标描述
        /// </summary>
        [field: SerializeField] public string PhaseDescription { get; private set; }
        /// <summary>
        /// 阶段目标类型
        /// </summary>
        [field: SerializeField] public TaskGoalType GoalType { get; private set; }
        /// <summary>
        /// 目标ID（NPCID/怪物ID/物品ID）- 完成目标所需要的数量
        /// </summary>
        [field: SerializeField] public Dictionary<int, int> TargetIdAndCount { get; private set; }
    }

    /// <summary>
    /// 任务奖励
    /// </summary>
    [Serializable]
    public class TaskRewardData
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        [field: SerializeField] public RewardType RewardType { get; private set; }
        /// <summary>
        /// 奖励ID（不需要Id的可以设置-1）
        /// </summary>
        [field: SerializeField] public int RewardId { get; private set; }
        /// <summary>
        /// 奖励数量
        /// </summary>
        [field: SerializeField] public int RewardCount { get; private set; }
    }


    /// <summary>
    /// 任务目标类型（单个阶段的目标）
    /// </summary>
    public enum TaskGoalType
    {
        TalkToNPC,   // 与NPC对话
        KillMonster, // 击杀怪物
    }

    /// <summary>
    /// 奖励类型
    /// </summary>
    public enum RewardType
    {
        Coin, //钱币  
        Souvenir, //纪念品
        Card //卡牌
    }
}