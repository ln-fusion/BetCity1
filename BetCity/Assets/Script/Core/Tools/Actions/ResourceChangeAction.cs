using BetCity.Core.ActionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    /// <summary>
    /// 资源变化基类
    /// </summary>
    public abstract class ResourceChangeAction : GameAction
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public ResourceType ResourceType { get; }

        /// <summary>
        /// 是否执行完修改，如果修改不合法一直为false
        /// </summary>
        public bool HasChanged { get; protected set; } = false;

        /// <summary>
        /// 变化量（正数增加，负数减少）
        /// </summary>
        public int ChangeAmount { get; }

        protected ResourceChangeAction(GameActionContext context, ResourceType resourceType, int changeAmount)
            : base(context)
        {
            ResourceType = resourceType;
            ChangeAmount = changeAmount;
        }
    }

    public enum ResourceType
    {
        Coin
    }
}
