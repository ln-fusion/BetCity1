using BetCity.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 当前存档进度
    /// </summary>
    [Serializable]
    public class ArchiveProgressDTO
    {
        /// <summary>
        /// 事件进展，事件Id-事件已触发次数
        /// </summary>
        public Dictionary<int, int> EventProgress { get;}
        /// <summary>
        /// 任务进展
        /// </summary>
        public HashSet<int> TaskProgress { get; }
        /// <summary>
        /// 零散数据存储字典
        /// </summary>
        public Dictionary<string, object> KeyValuePairs { get; }

        [JsonConstructor]
        public ArchiveProgressDTO(Dictionary<int, int> eventProgress, HashSet<int> taskProgress, Dictionary<string, object> keyValuePairs)
        {
            EventProgress = eventProgress ?? new Dictionary<int, int>();
            TaskProgress = taskProgress ?? new HashSet<int>();
            KeyValuePairs = keyValuePairs ?? new Dictionary<string, object>();
        }
    }
}