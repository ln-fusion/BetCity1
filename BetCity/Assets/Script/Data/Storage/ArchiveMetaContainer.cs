using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 存档元数据容器（用于序列化存档列表）
    /// </summary>
    [Serializable]
    public class ArchiveMetaContainer
    {
        /// <summary>
        /// 所有存档的元数据列表
        /// </summary>
        public List<ArchiveMeta> ArchiveMetaList { get; private set; } = new List<ArchiveMeta>();

        /// <summary>
        /// 当前激活的存档ID
        /// </summary>
        public Guid CurrentArchiveId { get; set; }

        /// <summary>
        /// 版本兼容
        /// </summary>
        public string SaveVersion { get; private set; } = "v0.1";
    }
}
