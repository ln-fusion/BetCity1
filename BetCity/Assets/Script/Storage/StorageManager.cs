using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BetCity.Storage
{
    /// <summary>
    /// 管理所有存储内容，目前包含存档数据（后续可以增加其他json文件如配置文件）
    /// </summary>
    public class StorageManager : MonoSingleton<StorageManager>, IModifyArchive
    {
        /// <summary>
        /// 存档路径，后续更改实现多存档
        /// </summary>
        public string ArchiveSavePath => Path.Combine(Application.persistentDataPath, "PlayerArchive.json");
        /// <summary>
        /// 存档数据
        /// </summary>
        public ArchiveContainer ArchiveData {get; private set;}

        protected override void Awake()
        {
            base.Awake();
            LoadArchiveData();
        }

        //暂时用来存档的地方
        private void OnDisable()
        {
            SaveArchiveData();
        }

        /// <summary>
        /// 加载存档（无存档则初始化空数据）
        /// </summary>
        private void LoadArchiveData()
        {
            try
            {
                if (File.Exists(ArchiveSavePath))
                {
                    string json = File.ReadAllText(ArchiveSavePath);
                    ArchiveData = JsonConvert.DeserializeObject<ArchiveContainer>(json);
                }
                else
                {
                    ArchiveData = new ArchiveContainer();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载存档失败，重置：{e.Message}");
                ArchiveData = new ArchiveContainer();
            }
        }

        /// <summary>
        /// 保存存档到本地文件（内部逻辑）重新改写ArchiveData
        /// </summary>
        private void SaveArchiveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(ArchiveData, Formatting.Indented);
                // 写入持久化路径
                File.WriteAllText(ArchiveSavePath, json);
                Debug.Log($"存档保存成功 → 路径：{ArchiveSavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"存档保存失败：{e.Message}");
            }
        }

        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchiveData();
        }

        /// <summary>
        /// 根据调用者的身份修改相应的存档
        /// </summary>
        public void ModifyArchive<T>(List<T> t, ISubmitArchive<T> caller)
        {
            if (caller is SouvenirManager)
            {
                if (typeof(T) == typeof(OwnedSouvenirDTO))
                {
                    ArchiveData.ModifyOwnedSouvenir(t.Cast<OwnedSouvenirDTO>().ToList(), this);
                }
                else
                {
                    throw new InvalidOperationException(caller + "传入错误类型信息" + typeof(T));
                }
            }
            else
            {
                throw new InvalidOperationException("非法的ISubmitArchive接口继承者试图修改存档");
            }
        }
    }
}
