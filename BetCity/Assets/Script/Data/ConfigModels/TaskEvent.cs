using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 任务板事件（非任务本身）
    /// </summary>
    public class TaskEvent : BaseEvent
    {
        /// <summary>
        /// 存储任务
        /// </summary>
        [field: SerializeField] public List<int> TaskIds { get; private set; }
    }
}
