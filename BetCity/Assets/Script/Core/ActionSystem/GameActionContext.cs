using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.ActionSystem 
{
    /// <summary>
    /// 上下文信息
    /// </summary>
    public class GameActionContext
    {
        /// <summary>
        /// 事件触发源（如玩家、敌人、某个物品）
        /// </summary>
        public object Source { get;}
        /// <summary>
        /// 事件目标
        /// </summary>
        public object Target { get; set; }
        /// <summary>
        /// 事件关联的主动作(仅一层）
        /// </summary>
        public GameAction SourceAction { get;}
        /// <summary>
        /// 动态扩展字段
        /// </summary>
        public Dictionary<string, object> ExtraData { get; set; } = new();

        public GameActionContext(object source, object target, GameAction sourceAction)
        {
            Source = source;
            Target = target;
            SourceAction = sourceAction;
        }

        public GameActionContext(object source, object target, GameAction sourceAction, Dictionary<string, object> extraData)
        {
            Source = source;
            Target = target;
            SourceAction = sourceAction;
            ExtraData = extraData;
        }
    }
}