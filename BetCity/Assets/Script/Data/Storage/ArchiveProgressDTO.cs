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
        public Dictionary<int, int> EventProgress {  get; }

        [JsonConstructor]
        public ArchiveProgressDTO(Dictionary<int, int> eventProgress)
        {
            this.EventProgress = eventProgress;
        }
    }
}