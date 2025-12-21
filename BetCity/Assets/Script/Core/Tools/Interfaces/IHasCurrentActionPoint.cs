using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    public interface IHasCurrentActionPoint
    {
        /// <summary>
        /// AP点
        /// </summary>
        int CurrentActionPoint { get; }
        /// <summary>
        /// 变换AP点
        /// </summary>
        bool ChangeCurrentActionPoint(int amount, CurrentActionPointChangeAction caller);
    }
}
