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
    private SouvenirDataManager SouvenirDataManager => SouvenirDataManager.Instance;
    private StorageManager StorageManager => StorageManager.Instance;
    // 所有藏品原型
    private IReadOnlyList<SouvenirData> AllSouvenirDatas => SouvenirDataManager.Data;
    //仅已拥有的藏品实例（Key=藏品ID）
    private Dictionary<int, Souvenir> ownedSouvenirs = new Dictionary<int, Souvenir>();
    //未拥有藏品id列表
    private List<int> notOwnedSouvenirs = new List<int>();
    //存档数据
    private ArchiveContainer ArchiveData => StorageManager.ArchiveData;
    private IReadOnlyList<OwnedSouvenirDTO> OwnedSouvenirDTOs => ArchiveData.OwnedSouvenirDTOs;
    /// <summary>
    /// 已拥有纪念品只读字典
    /// </summary>
    public IReadOnlyDictionary<int, Souvenir> OwnedSouvenirs => ownedSouvenirs;
    /// <summary>
    /// 所有纪念品只读字典
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
        foreach (var keyValuePair in ownedSouvenirs)
        {
            UnregisterEffect(keyValuePair.Value);
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
        }
    }

    /// <summary>
    /// 注册效果
    /// </summary>
    private void RegisterEffect(Souvenir souvenir)
    {
        foreach (EffectConfig effect in souvenir.Effects)
        {
            if(effect.Lifetime != EffectLifetime.OneShot)
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
            OwnedSouvenirDTO ownedSouvenirDTO = new OwnedSouvenirDTO(kt.Key, s.Price, s.ExtraData);
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
    /// 根据纪念品的Id来获得对应的纪念品，如果是已拥有的纪念品会返回false但是会输出对应的纪念品
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
            errorMsg = "该id所对应的纪念品不存在";;
            souvenir = null;
            return false;
        }
        souvenir = new Souvenir(AllSouvenirDatas.Where(s => s.Id == id).First());
        ownedSouvenirs[id] = souvenir;
        notOwnedSouvenirs.Remove(id);
        souvenir.SetIsOwned(true, this);
        RegisterEffect(souvenir);
        errorMsg = null;
        return true;
    }

    /// <summary>
    /// 根据纪念品的Id来失去对应的纪念品
    /// </summary>
    /// <param name="id"></param>
    /// <param name="souvenir">返回纪念品</param>
    /// <param name="errorMsg">返回错误信息</param>
    /// <returns>操作成功与否</returns>
    public bool LoseSouvenirById(int id, out Souvenir souvenir, out string errorMsg)
    {
        if (!ownedSouvenirs.ContainsKey(id))
        {
            errorMsg = "该id所对应的纪念品未拥有";
            souvenir = null;
            return false;
        }

        souvenir = ownedSouvenirs[id];
        UnregisterEffect(souvenir);
        ownedSouvenirs.Remove(id);
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
    /// 上传对应的存档
    /// </summary>
    public void SubmitArchive(List<OwnedSouvenirDTO> dTOs) 
    {
        StorageManager.ModifyArchive(dTOs, this);
    }
    #endregion
}
