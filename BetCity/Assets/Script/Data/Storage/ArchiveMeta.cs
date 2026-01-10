using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 存档元数据（描述存档的基础信息）
    /// </summary>
    [Serializable]
    public class ArchiveMeta
    {
        /// <summary>
        /// 存档唯一ID（GUID）
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 存档路径
        /// </summary>
        public string SavePath => Path.Combine(Application.persistentDataPath, "PlayerArchive" + Id.ToString() + ".json");

        /// <summary>
        /// 存档名称（玩家自定义）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifyTime { get; set; }

        /// <summary>
        /// 构造新存档元数据
        /// </summary>
        /// <param name="name">存档名称</param>
        public ArchiveMeta(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            CreateTime = DateTime.Now;
            LastModifyTime = DateTime.Now;
        }
    }
}