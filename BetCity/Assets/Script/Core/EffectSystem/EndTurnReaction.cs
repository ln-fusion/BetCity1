using BetCity.Core.ActionSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.EffectSystem
{
    /// <summary>
    /// 触发条件
    /// </summary>
    [Serializable]
    public class EndTurnReaction
    {
        /// <summary>
        ///监听的GameAction类型（用string为了可被序列化）
        /// </summary>
        [field: SerializeField] public string TargetActionTypeString { get; private set; }
        public Type TargetActionType => TargetActionTypeString == null ? null : Type.GetType(TargetActionTypeString);
        /// <summary>
        /// 订阅触发的优先级
        /// </summary>
        [field: SerializeField] public int Priority { get; private set; }

        public EndTurnReaction(string targetActionTypeString, int priority = 0)
        {
            TargetActionTypeString = targetActionTypeString;
            Priority = priority;
        }
    }
}