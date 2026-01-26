using BetCity.Core.Tools;
using BetCity.GamePlay.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using BetCity.Card;
using BetCity.GamePlay.Souvenir;
using BetCity.Core.ProgressSystem;
using BetCity.GamePlay.Task;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 管理所有存储内容，存档元数据查看，选择-读取存档
    /// </summary>
    public class StorageManager : MonoSingleton<StorageManager>, IModifyArchive
    {
        /// <summary>
        /// 当前选择的存档路径
        /// </summary>
        public string ArchiveDataSavePath => CurrentArchiveMeta?.SavePath;
        /// <summary>
        /// 存档元数据路径
        /// </summary>
        public string ArchiveMetaSavePath => Path.Combine(Application.persistentDataPath, "ArchiveMetaContainer.json");
        /// <summary>
        /// 存档数据
        /// </summary>
        public ArchiveDataContainer ArchiveDataContainer { get; private set; } = new ArchiveDataContainer();
        /// <summary>
        /// 当前选择的存档的元数据
        /// </summary>
        public ArchiveMeta CurrentArchiveMeta {  get; private set; }
        /// <summary>
        /// 存档元数据
        /// </summary>
        public ArchiveMetaContainer ArchiveMetaContainer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            LoadArchiveMeta();
            LoadArchiveData();
        }

        //暂时用来存档的地方
        private void OnDisable()
        {
            SaveArchive();
        }

        /// <summary>
        /// 从ArchiveMetaSavePath加载存档元数据,并将
        /// </summary>
        private void LoadArchiveMeta()
        {
            try
            {
                if (File.Exists(ArchiveMetaSavePath))
                {
                    ArchiveMetaContainer = new ArchiveMetaContainer();
                    string json = File.ReadAllText(ArchiveMetaSavePath);
                    ArchiveMetaContainer = JsonConvert.DeserializeObject<ArchiveMetaContainer>(json);
                    if (ArchiveMetaContainer == null)
                    {
                        ArchiveMetaContainer = new ArchiveMetaContainer();
                    }
                    else
                    {
                        CurrentArchiveMeta = ArchiveMetaContainer.ArchiveMetaList.Find(meta => meta.Id == ArchiveMetaContainer.CurrentArchiveId);
                    }
                }
                else
                {
                    ArchiveMetaContainer = new ArchiveMetaContainer();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载存档失败，重置：{e.Message}");
                ArchiveDataContainer = new ArchiveDataContainer();
            }
        }

        /// <summary>
        /// 加载存档（无存档则初始化空数据）
        /// </summary>
        private void LoadArchiveData()
        {
            try
            {
                if (ArchiveDataSavePath != null && File.Exists(ArchiveDataSavePath))
                {
                    string json = File.ReadAllText(ArchiveDataSavePath);
                    ArchiveDataContainer = JsonConvert.DeserializeObject<ArchiveDataContainer>(json);
                    if (ArchiveDataContainer == null)
                    {
                        ArchiveDataContainer = new ArchiveDataContainer();
                    }
                }
                else
                {
                    CurrentArchiveMeta = new ArchiveMeta("PlayerArchive");
                    ArchiveMetaContainer.ArchiveMetaList.Add(CurrentArchiveMeta);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载存档失败，重置：{e.Message}");
                ArchiveDataContainer = new ArchiveDataContainer();
            }
        }

        /// <summary>
        /// 保存存档包括存档元数据
        /// </summary>
        private void SaveArchive()
        {
            try
            {
                //先写入再保存
                SouvenirManager.Instance.ManualSave();

                string json = JsonConvert.SerializeObject(ArchiveDataContainer, Formatting.Indented);

                // 写入持久化路径
                File.WriteAllText(ArchiveDataSavePath, json);

                CurrentArchiveMeta.LastModifyTime = DateTime.Now;

                Debug.Log($"[StorageManager]存档保存成功 → 路径：{ArchiveDataSavePath}");
                ArchiveMetaContainer.CurrentArchiveId = CurrentArchiveMeta.Id;

                json = JsonConvert.SerializeObject(ArchiveMetaContainer, Formatting.Indented);
                File.WriteAllText(ArchiveMetaSavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager]存档保存失败：{e.Message}");
            }
        }

        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
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
                    ArchiveDataContainer.ModifyOwnedSouvenir(t.Cast<OwnedSouvenirDTO>().ToList(), this);
                }
                else
                {
                    throw new InvalidOperationException(caller + "传入错误类型信息" + typeof(T));
                }
            }
            else if (caller is ExplorerPlayerController)
            {
                if (typeof(T) == typeof(PlayerDataDTO))
                {

                    ArchiveDataContainer.ModifyExplorerPlayerData(t.Cast<PlayerDataDTO>().ToList(), this);
                }
                else
                {
                    throw new InvalidOperationException(caller + "传入错误类型信息" + typeof(T));
                }
            }
            else if (caller is CardManager)
            {
                if (typeof(T) == typeof(OwnedCardDTO))
                {
                    ArchiveDataContainer.ModifyOwnedCard(t.Cast<OwnedCardDTO>().ToList(), this);
                }
                else
                {
                    throw new InvalidOperationException(caller + "传入错误类型信息" + typeof(T));
                }
            }
            else if(caller is ProgressManager)
            {
                if (typeof(T) == typeof(ArchiveProgressDTO))
                {
                    ArchiveDataContainer.ModifyArchiveProgress(t.Cast<ArchiveProgressDTO>().ToList(), this);
                }
                else
                {
                    throw new InvalidOperationException(caller + "传入错误类型信息" + typeof(T));
                }
            }
            else if(caller is TaskManager)
            {
                if (typeof(T) == typeof(AcceptedTaskDTO))
                {
                    ArchiveDataContainer.ModifyAcceptedTask(t.Cast<AcceptedTaskDTO>().ToList(), this);
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
