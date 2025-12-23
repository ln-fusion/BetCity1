using BetCity.Core.ActionSystem;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BetCity.Core.EffectSystem
{
    /// <summary>
    /// 触发条件
    /// </summary>
    [Serializable]
    public class TriggerCondition
    {
        /// <summary>
        ///监听的GameAction类型（用string为了可被序列化）
        /// </summary>
        [field: SerializeField] public string TargetActionTypeString { get; private set; }
        public Type TargetActionType  => TargetActionTypeString == null ? null : Type.GetType(TargetActionTypeString);
        /// <summary>
        /// 触发时机
        /// </summary>
        [field: SerializeField] public ReactionTiming Timing { get; private set; }
        /// <summary>
        /// 订阅触发的优先级
        /// </summary>
        [field: SerializeField] public int Priority { get; private set; }

        public TriggerCondition(string targetActionTypeString, ReactionTiming timing, int priority = 0)
        {
            TargetActionTypeString = targetActionTypeString;
            Timing = timing;
            Priority = priority;
        }
    }
}
