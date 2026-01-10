using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BetCity.Data.Storage;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.Core.EffectSystem;

namespace BetCity.GamePlay.Souvenir
{
    /// <summary>
    /// 纪念品管理器（单例），维护当前拥有/背包/仓库中藏品，同时提供接口修改，已拥有纪念品通过存档读取，未拥有纪念品直接从原型数据读出
    /// </summary>
    public class SouvenirManager : MonoSingleton<SouvenirManager>, IModifySouvenir, ISubmitArchive<OwnedSouvenirDTO>
    {
        private SouvenirDataManager SouvenirDataManager => SouvenirDataManager.Instance;
        private StorageManager StorageManager => StorageManager.Instance;
        // 所有藏品原型
        private IReadOnlyList<SouvenirData> AllSouvenirDatas => SouvenirDataManager.Data;
        //所有拥有藏品实例
        private Dictionary<int, Souvenir> ownedSouvenirs = new Dictionary<int, Souvenir>();
        //仅背包中的藏品（不包括剧情藏品）
        private List<int> bagSouvenirs = new List<int>();
        //仓库中的藏品（不包括剧情藏品）
        private List<int> warehouseSouvenirs = new List<int>();
        //特殊剧情藏品
        private List<int> specialSouvenirs = new List<int>();
        //未拥有藏品id列表
        private List<int> notOwnedSouvenirs = new List<int>();
        //存档数据
        private ArchiveDataContainer ArchiveData => StorageManager.ArchiveDataContainer;
        private IReadOnlyList<OwnedSouvenirDTO> OwnedSouvenirDTOs => ArchiveData.OwnedSouvenirDTOs;
        /// <summary>
        /// 当前最大槽数（待改动
        /// </summary>
        public int Max_Slots { get; private set; } = 10;
        /// <summary>
        /// 当前槽数
        /// </summary>
        public int Current_Slots {  get; private set; }
        /// <summary>
        /// 所有已拥有纪念品
        /// </summary>
        public IReadOnlyDictionary<int, Souvenir> OwnedSouvenirs => ownedSouvenirs;
        /// <summary>
        /// 背包中纪念品只读字典（不包括剧情藏品）
        /// </summary>
        public IReadOnlyList<int> BagSouvenirs => bagSouvenirs;
        /// <summary>
        /// 仓库中纪念品只读字典（不包括剧情藏品）
        /// </summary>
        public IReadOnlyList<int> WarehouseSouvenirs => warehouseSouvenirs;
        /// <summary>
        /// 特殊剧情纪念品只读字典
        public IReadOnlyList<int> SpecialSouvenirs => specialSouvenirs;
        /// <summary>
        /// 没有的纪念品的id列表
        /// </summary>
        public IReadOnlyList<int> NotOwnedSouvenirs => NotOwnedSouvenirs;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            CacheOwnedSouvenirInstances();
            LoadNotOwnedSouvenirs();
        }

        /// <summary>
        /// 暂时用来保存/取消注册的地方
        /// </summary>
        private void OnDisable()
        {
            SaveArchive();
            foreach (var id in bagSouvenirs)
            {
                UnregisterEffect(ownedSouvenirs[id]);
            }
            foreach (var id in specialSouvenirs)
            {
                UnregisterEffect(ownedSouvenirs[id]);
            }
        }

        #region 初始化/保存相关
        /// <summary>
        /// 加载未拥有纪念品id列表
        /// </summary>
        private void LoadNotOwnedSouvenirs()
        {
            notOwnedSouvenirs = AllSouvenirDatas.Where(s => !ownedSouvenirs.ContainsKey(s.Id)).Select(s => s.Id).ToList();
        }

        /// <summary>
        /// 缓存已拥有的藏品实例（从存档数据创建）
        /// </summary>
        private void CacheOwnedSouvenirInstances()
        {
            bagSouvenirs.Clear();
            if (OwnedSouvenirDTOs == null) return;

            foreach (var dto in OwnedSouvenirDTOs)
            {
                SouvenirData souvenirData = SouvenirDataManager.GetDataById(dto.Id);
                if (souvenirData == null)
                {
                    Debug.LogError($"发现不存在的纪念品，非法Id为：{dto.Id}");
                }

                //创建已拥有的藏品实例（价格用存档的自定义值）
                Souvenir souvenir = new Souvenir(souvenirData, dto.ExtraData, dto.CustomPrice, dto.IsInBag, true);

                if (souvenir.Quality == SouvenirQuality.Special)
                {
                    RegisterEffect(souvenir);
                    ownedSouvenirs.Add(dto.Id, souvenir);
                    specialSouvenirs.Add(dto.Id);
                    continue;
                }
                else if (dto.IsInBag)
                {
                    RegisterEffect(souvenir);
                    ownedSouvenirs.Add(dto.Id, souvenir);
                    bagSouvenirs.Add(dto.Id);
                    Current_Slots += souvenir.Slot;
                    continue;
                }
                ownedSouvenirs.Add(dto.Id, souvenir);
                warehouseSouvenirs.Add(dto.Id);
            }

            if(Current_Slots > Max_Slots)
            {
                Debug.LogWarning("存档内拥有的槽数>最大槽数!");
            }
        }

        /// <summary>
        /// 注册效果
        /// </summary>
        private void RegisterEffect(Souvenir souvenir)
        {
            foreach (EffectConfig effect in souvenir.Effects)
            {
                if (effect.Lifetime != EffectLifetime.OneShot)
                {
                    effect.Activate();
                }
                effect.Source = souvenir;
            }
        }

        /// <summary>
        /// 根据字典生成当前存档并提交
        /// </summary>
        private void SaveArchive()
        {
            List<OwnedSouvenirDTO> saveData = new List<OwnedSouvenirDTO>();
            foreach (var kt in ownedSouvenirs)
            {
                Souvenir s = kt.Value;
                OwnedSouvenirDTO ownedSouvenirDTO = new OwnedSouvenirDTO(kt.Key, s.Price, s.IsInBag, s.ExtraData);
                saveData.Add(ownedSouvenirDTO);
            }
            SubmitArchive(saveData);
        }

        /// <summary>
        /// 将效果取消注册
        /// </summary>
        private void UnregisterEffect(Souvenir souvenir)
        {
            foreach (EffectConfig config in souvenir.Effects)
            {
                config.Deactivate();
            }
        }
        #endregion

        #region 接口
        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
        }

        /// <summary>
        /// 根据纪念品的Id来获得对应的纪念品(获取后槽数不够会进入仓库)，如果是已拥有的纪念品会返回false但是会输出对应的纪念品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="souvenir">返回纪念品</param>
        /// <param name="errorMsg">返回错误信息</param>
        /// <returns>操作成功与否</returns>
        public bool OwnSouvenirById(int id, out Souvenir souvenir, out string errorMsg)
        {
            if (ownedSouvenirs.ContainsKey(id))
            {
                errorMsg = "该id所对应的纪念品已拥有";
                souvenir = ownedSouvenirs[id];
                return false;
            }
            else if (!notOwnedSouvenirs.Contains(id))
            {
                errorMsg = "该id所对应的纪念品不存在"; ;
                souvenir = null;
                return false;
            }
            souvenir = new Souvenir(AllSouvenirDatas.Where(s => s.Id == id).First());
            if (souvenir.Quality == SouvenirQuality.Special)
            {
                ownedSouvenirs[id] = souvenir;
                specialSouvenirs.Add(id);
            }
            else
            {
                ownedSouvenirs[id] = souvenir;
                if (Current_Slots + souvenir.Slot <= Max_Slots)
                {
                    bagSouvenirs.Add(id);
                    Current_Slots += souvenir.Slot;
                }
                else warehouseSouvenirs.Add(id);
            }
            notOwnedSouvenirs.Remove(id);
            souvenir.SetIsOwned(true, this);
            RegisterEffect(souvenir);
            errorMsg = null;
            return true;
        }

        /// <summary>
        /// 根据纪念品的Id来失去对应的纪念品(只能失去背包中的)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="souvenir">返回纪念品</param>
        /// <param name="errorMsg">返回错误信息</param>
        /// <returns>操作成功与否</returns>
        public bool LoseSouvenirById(int id, out Souvenir souvenir, out string errorMsg)
        {
            if (!bagSouvenirs.Contains(id) && !specialSouvenirs.Contains(id))
            {
                errorMsg = "该id所对应的纪念品未拥有或未在背包";
                souvenir = null;
                return false;
            }

            souvenir = ownedSouvenirs[id];
            UnregisterEffect(souvenir);
            ownedSouvenirs.Remove(id);
            if (souvenir.Quality == SouvenirQuality.Special) specialSouvenirs.Remove(id);
            else
            {
                bagSouvenirs.Remove(id);
                Current_Slots -= souvenir.Slot;
            }

            notOwnedSouvenirs.Add(id);
            souvenir.SetIsOwned(false, this);
            errorMsg = null;
            return true;
        }

        /// <summary>
        /// 查询纪念品是否拥有
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>是否拥有</returns>
        public bool IsOwned(int id)
        {
            return ownedSouvenirs.ContainsKey(id);
        }

        /// <summary>
        /// 通过ID查询已拥有纪念品
        /// </summary>
        /// <param name="id">已拥有纪念品ID</param>
        /// <returns>对应的Souvenir，不存在则返回null</returns>
        public Souvenir GetOwnedSouvenirById(int id)
        {
            if (ownedSouvenirs.TryGetValue(id, out Souvenir result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 将纪念品从仓库到背包
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>成功与否</returns>
        public bool WarehouseToBag(int id)
        {
            if (warehouseSouvenirs.Contains(id))
            {
                int slot = ownedSouvenirs[id].Slot;
                if(Current_Slots + slot > Max_Slots)
                {
                    Debug.LogWarning("[SouvenirManager]槽数超过上限！");
                    return false;
                }
                bagSouvenirs.Add(id);
                warehouseSouvenirs.Remove(id);
                Current_Slots += slot;
                return true;
            }
            Debug.LogWarning($"[SouvenirManager]对应id为{id}的纪念品不在仓库中");
            return false;
        }

        /// <summary>
        /// 将纪念品从背包到仓库
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>成功与否</returns>
        public bool BagToWarehouse(int id)
        {
            if (bagSouvenirs.Contains(id))
            {
                Souvenir souvenir = ownedSouvenirs[id];
                if(souvenir.Quality == SouvenirQuality.Special)
                {
                    Debug.LogWarning($"[SouvenirManager]对应id为{id}的纪念品不能放入仓库，因为其为特殊纪念品");
                    return false;
                }
                Current_Slots -= ownedSouvenirs[id].Slot;
                warehouseSouvenirs.Add(id);
                bagSouvenirs.Remove(id);
                return true;
            }
            Debug.LogWarning($"[SouvenirManager]对应id为{id}的纪念品不在背包中");
            return false;
        }

        /// <summary>
        /// 上传对应的存档
        /// </summary>
        public void SubmitArchive(List<OwnedSouvenirDTO> dTOs)
        {
            StorageManager.ModifyArchive(dTOs, this);
        }
        #endregion
    }
}
