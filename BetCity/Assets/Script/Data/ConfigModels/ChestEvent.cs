using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 商店事件
    /// </summary>
    [CreateAssetMenu(fileName = "Event", menuName = "Event/ChestEvent")]
    public class ChestEvent : BaseEvent
    {
        /// <summary>
        /// 每轮奖励列表
        /// </summary>
        [field: SerializeField]
        public List<ChestOptionSet> ChoiceOptionSets;
    }
    [Serializable]
    public class ChestOptionSet
    {
        [field: SerializeField]
        public List<ChestOption> ChoiceOptions;
    }
        [Serializable]
    public class ChestOption
    {
        /// <summary>
        /// id，与对应类别的id相关联
        /// </summary>
        [field: SerializeField] public int ChestId { get; private set; }
        /// <summary>
        /// 物品种类
        /// </summary>
        [field: SerializeField] public ItemType ItemType { get; private set; }
        /// <summary>
        /// 宝箱出现条件
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, List<string>> Conditions { get; private set; }
    }
}
