using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 接受的任务
    /// </summary>
    [Serializable]
    public class AcceptedTaskDTO
    {
        /// <summary>
        /// 关联任务ID
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// 当前任务进行阶段索引
        /// </summary>
        public int CurrentPhaseIndex { get;}
        /// <summary>
        /// 当前任务阶段达成的数量
        /// </summary>
        public int CurrentPhaseCurrentCount { get; }

        [JsonConstructor]
        public AcceptedTaskDTO(int id, int currentPhaseIndex, int currentPhaseCurrentCount)
        {
            id = Id;
            CurrentPhaseIndex = currentPhaseIndex;
            CurrentPhaseCurrentCount = currentPhaseCurrentCount;
        }
    }
}
