using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetCity.GamePlay.Souvenir
{
    /// <summary>
    ///  负责管理所有的SouvernirData即纪念品原型数据
    /// </summary>
    public partial class SouvenirDataManager : MonoSingleton<SouvenirDataManager>
    {
        /// <summary>
        /// 所有纪念品数据列表（只读对外暴露）
        /// </summary>
        public IReadOnlyList<SouvenirData> Data => _data;
        private List<SouvenirData> _data = new List<SouvenirData>();
        // 数据字典（用于快速通过ID查询）
        private Dictionary<int, SouvenirData> _dataDict = new Dictionary<int, SouvenirData>();

        [Header("数据配置")]
        [SerializeField] private string souvenirDataResourcesPath;

        /// <summary>
        /// 初始化数据
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            LoadAllSouvenirData();
            // 验证数据合法性
            ValidateData();

            // 标记为不销毁
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 从Resources目录加载所有SouvenirData资源（加载部分后期可以优化）
        /// </summary>
        private void LoadAllSouvenirData()
        {
            try
            {
                // 暂时采用同步LoadAll
                SouvenirData[] loadedDatas = Resources.LoadAll<SouvenirData>(souvenirDataResourcesPath);

                if (loadedDatas == null || loadedDatas.Length == 0)
                {
                    Debug.LogWarning($"[SouvenirDataManager] 未在Resources/{souvenirDataResourcesPath}路径下找到任何SouvenirData资源");
                    return;
                }

                _data.Clear();
                _dataDict.Clear();

                // 填充数据列表和字典
                foreach (SouvenirData data in loadedDatas)
                {
                    if (data == null) continue;

                    _data.Add(data);
                    _dataDict[data.Id] = data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SouvenirDataManager] 加载数据失败：{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 验证数据合法性（目前校验id字段是否重复,价格数据是否>0,纪念品名字是否为空）
        /// </summary>
        private void ValidateData()
        {
            HashSet<int> idSet = new HashSet<int>();
            List<string> errorMessages = new List<string>();

            for (int i = 0; i < _data.Count; i++)
            {
                SouvenirData data = _data[i];

                // 校验ID唯一性
                if (!idSet.Add(data.Id))
                {
                    errorMessages.Add($"重复的ID：{data.Id}（数据名称：{data.Name}）");
                }

                // 校验必填字段
                if (string.IsNullOrEmpty(data.Name))
                {
                    errorMessages.Add($"ID为{data.Id}的纪念品名称为空");
                }

                if (data.Price < 0)
                {
                    errorMessages.Add($"ID为{data.Id}的纪念品价格无效：{data.Price}，价格不能为负数");
                }
            }
            if (errorMessages.Count > 0)
            {
                string errorMsg = $"[SouvenirDataManager] 数据校验失败，共{errorMessages.Count}个错误：\n{string.Join("\n", errorMessages)}";
                Debug.LogError(errorMsg);
            }
        }

        /// <summary>
        /// 通过ID查询纪念品数据
        /// </summary>
        /// <param name="id">纪念品ID</param>
        /// <returns>对应的SouvenirData，不存在则返回null</returns>
        public SouvenirData GetDataById(int id)
        {
            if (_dataDict.TryGetValue(id, out SouvenirData result))
            {
                return result;
            }

            return null;
        }
    }

}
