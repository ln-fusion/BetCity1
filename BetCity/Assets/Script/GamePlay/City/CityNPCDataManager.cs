using BetCity.Card;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.City
{
    public class CityNPCDataManager : MonoSingleton<CardDataManager>
    {

        /// <summary>
        /// 所有角色数据列表（只读对外暴露）
        /// </summary>
        public IReadOnlyList<CityNPCData> Data => _data;
        private List<CityNPCData> _data = new List<CityNPCData>();
        // 数据字典（用于快速通过ID查询）
        private Dictionary<int, CityNPCData> _dataDict = new Dictionary<int, CityNPCData>();
        /// <summary>
        /// CardData 资源路径（Resources 下的子目录名）
        /// </summary>
        public const string CHARACTER_DATA_RESOURCES_PATH = "City/Character";

        [Header("数据配置")]
        [SerializeField] private string characterDataResourcesPath = CHARACTER_DATA_RESOURCES_PATH;




        /// <summary>
        /// 初始化数据
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            LoadAllCityCharacterData();

            DontDestroyOnLoad(gameObject);
        }


        /// <summary>
        /// 从Resources目录加载所有CityCharacterData资源
        /// </summary>
        private void LoadAllCityCharacterData()
        {
            try
            {
                string path = string.IsNullOrEmpty(characterDataResourcesPath) ? CHARACTER_DATA_RESOURCES_PATH : characterDataResourcesPath;
                CityNPCData[] loadedDatas = Resources.LoadAll<CityNPCData>(path);

                if (loadedDatas == null || loadedDatas.Length == 0)
                {
                    Debug.LogWarning($"[CityCharacterDataManager] 未在Resources/{path}路径下找到任何CityCharacterData资源");
                    return;
                }

                _data.Clear();
                _dataDict.Clear();

                foreach (CityNPCData data in loadedDatas)
                {
                    if (data == null) continue;

                    _data.Add(data);
                    _dataDict[data.Id] = data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CityCharacterDataManager] 加载数据失败：{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
