using BetCity.Core.ActionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.EffectSystem 
{
    /// <summary>
    /// 效果激活上下文（包含载体信息、来源、目标等）
    /// </summary>
    public class EffectContext
    {
        /// <summary>
        /// 载体,(如卡牌触发的效果就是卡牌的实例）
        /// </summary>
        public object Carrier {  get; }
        /// <summary>
        /// 关联的游戏行为上下文
        /// </summary>
        public GameActionContext ActionContext; 
    }
}

