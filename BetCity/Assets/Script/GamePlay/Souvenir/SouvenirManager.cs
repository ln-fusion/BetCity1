using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BetCity.Storage;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.Core.EffectSystem;


/// <summary>
/// 纪念品管理器（单例），为所有纪念品提供一个实例供外界访问，同时提供接口修改，已拥有纪念品通过存档读取，未拥有纪念品直接从原型数据读出
/// </summary>
public class SouvenirManager : MonoSingleton<SouvenirManager>, IModifySouvenir, ISubmitArchive<OwnedSouvenirDTO>
{
    public SouvenirDataManager SouvenirDataManager => SouvenirDataManager.Instance;
    public StorageManager StorageManager => StorageManager.Instance;

    // 所有藏品原型
    private IReadOnlyList<SouvenirData> allSouvenirDatas => SouvenirDataManager.Data;
    //仅已拥有的藏品实例（Key=藏品ID）
    private Dictionary<int, Souvenir> ownedSouvenirs = new Dictionary<int, Souvenir>();
    //全部藏品实例
    private  Dictionary<int, Souvenir> allSouvenirs = new Dictionary<int, Souvenir>();
    private ArchiveContainer ArchiveData => StorageManager.ArchiveData;
    private IReadOnlyList<OwnedSouvenirDTO> OwnedSouvenirDTOs => ArchiveData.OwnedSouvenirDTOs;
    /// <summary>
    /// 已拥有纪念品只读字典
    /// </summary>
    public IReadOnlyDictionary<int, Souvenir> OwnedSouvenirs => ownedSouvenirs;
    /// <summary>
    /// 所有纪念品只读字典
    /// </summary>
    public IReadOnlyDictionary<int, Souvenir> AllSouvenirs => allSouvenirs;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        CacheOwnedSouvenirInstances();
        LoadNotOwnedData();
    }

    /// <summary>
    /// 暂时用来保存/取消注册的地方
    /// </summary>
    private void OnDisable()
    {
        SaveArchive();
        foreach (var keyValuePair in ownedSouvenirs)
        {
            UnregisterEffect(keyValuePair.Value);
        }
    }

    #region 初始化/保存相关
    /// <summary>
    /// 缓存已拥有的藏品实例（从存档数据创建）
    /// </summary>
    private void CacheOwnedSouvenirInstances()
    {
        ownedSouvenirs.Clear();
        if (OwnedSouvenirDTOs == null) return;

        foreach (var dto in OwnedSouvenirDTOs)
        {
            SouvenirData souvenirData = SouvenirDataManager.GetDataById(dto.Id);
            if (souvenirData == null)
            {
                Debug.LogError($"发现不存在的纪念品，非法Id为：{dto.Id}");
            }

            //创建已拥有的藏品实例（价格用存档的自定义值）
            Souvenir souvenir = new Souvenir(souvenirData, dto.ExtraData, dto.CustomPrice, true)
            {
                Price = dto.CustomPrice
            };
            //注册效果
            RegisterEffect(souvenir);
            ownedSouvenirs.Add(dto.Id, souvenir);
            allSouvenirs.Add(dto.Id, souvenir);
        }
    }

    /// <summary>
    /// 注册效果
    /// </summary>
    private void RegisterEffect(Souvenir souvenir)
    {
        foreach (PassiveEffectConfig effect in souvenir.Effects)
        {
            effect.Activate();
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
            OwnedSouvenirDTO ownedSouvenirDTO = new OwnedSouvenirDTO(kt.Key, s.Price, s.ExtraData);
            saveData.Add(ownedSouvenirDTO);
        }
        SubmitArchive(saveData);
    }

    /// <summary>
    /// 加载所有未获得的收藏品实例
    /// </summary>
    private void LoadNotOwnedData()
    {
        // 找出“未拥有”且“还没实例化”的 ID
        var missingIds = allSouvenirDatas
            .Select(d => d.Id)               // 全部配表ID
            .Except(ownedSouvenirs.Keys)     // 去掉已拥有
            .Except(allSouvenirs.Keys);      // 去掉已实例化（保险）

        foreach (int id in missingIds)
        {
            SouvenirData data = SouvenirDataManager.GetDataById(id);
            var souvenir = new Souvenir(data);
            allSouvenirs[id] = souvenir;
        }
    }

    /// <summary>
    /// 将效果取消注册
    /// </summary>
    private void UnregisterEffect(Souvenir souvenir)
    {
        foreach (PassiveEffectConfig config in souvenir.Effects)
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
    /// 根据纪念品的Id来获得对应的纪念品，如果是已拥有的纪念品会返回false但是会输出对应的纪念品
    /// </summary>
    /// <param name="id"></param>
    /// <param name="souvenir">返回纪念品</param>
    /// <param name="errorMsg">返回错误信息</param>
    /// <returns>操作成功与否</returns>
    public bool OwnSouvenirById(int id, out Souvenir souvenir, out string errorMsg)
    {
        if (!allSouvenirs.ContainsKey(id))
        {
            errorMsg = "该id所对应的纪念品不存在";
            souvenir = null;
            return false;
        }
        else if (ownedSouvenirs.ContainsKey(id))
        {
            errorMsg = "该id所对应的纪念品已拥有";
            souvenir = ownedSouvenirs[id];
            return false;
        }
        souvenir = allSouvenirs[id];
        ownedSouvenirs[id] = souvenir;
        souvenir.SetIsOwned(true, this);
        RegisterEffect(souvenir);
        errorMsg = null;
        return true;
    }

    /// <summary>
    /// 根据纪念品的Id来失去对应的纪念品，如果是未拥有的纪念品会返回false但是会输出对应的纪念品
    /// </summary>
    /// <param name="id"></param>
    /// <param name="souvenir">返回纪念品</param>
    /// <param name="errorMsg">返回错误信息</param>
    /// <returns>操作成功与否</returns>
    public bool LoseSouvenirById(int id, out Souvenir souvenir, out string errorMsg)
    {
        if (!allSouvenirs.ContainsKey(id))
        {
            errorMsg = "该id所对应的纪念品不存在";
            souvenir = null;
            return false;
        }
        else if (!ownedSouvenirs.ContainsKey(id))
        {
            errorMsg = "该id所对应的纪念品未拥有";
            souvenir = ownedSouvenirs[id];
            return false;
        }
        souvenir = allSouvenirs[id];
        UnregisterEffect(souvenir);
        ownedSouvenirs.Remove(id);
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
    /// 通过ID查询纪念品
    /// </summary>
    /// <param name="id">纪念品ID</param>
    /// <returns>对应的Souvenir，不存在则返回null</returns>
    public Souvenir GetSouvenirById(int id)
    {
        if (allSouvenirs.TryGetValue(id, out Souvenir result))
        {
            return result;
        }
        return null;
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
    /// 上传对应的存档
    /// </summary>
    public void SubmitArchive(List<OwnedSouvenirDTO> dTOs) 
    {
        StorageManager.ModifyArchive(dTOs, this);
    }
    #endregion
}
