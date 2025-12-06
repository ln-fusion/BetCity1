using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 纪念品管理器（单例），为所有纪念品提供一个实例供外界访问，同时提供接口修改，已拥有纪念品通过存档读取，未拥有纪念品直接从原型数据读出
/// </summary>
public class SouvenirManager : MonoSingleton<SouvenirManager>, IModifySouvenir
{
    public SouvenirDataManager souvenirDataManager; //在Inspector绑定

    // 所有藏品原型
    private IReadOnlyList<SouvenirData> allSouvenirDatas => souvenirDataManager.Data;
    // 存档路径
    private string SavePath => Path.Combine(Application.persistentDataPath, "OwnedSouvenirs.json");
    //仅已拥有的藏品实例（Key=藏品ID）
    private Dictionary<int, Souvenir> ownedSouvenirs = new Dictionary<int, Souvenir>();
    //全部藏品实例
    private Dictionary<int, Souvenir> allSouvenirs = new Dictionary<int, Souvenir>();
    // 存档数据（内存缓存）
    private OwnedSouvenirContainer saveData;
    /// <summary>
    /// 已拥有纪念品只读字典
    /// </summary>
    public Dictionary<int, Souvenir> OwnedSouvenirs => ownedSouvenirs;
    /// <summary>
    /// 所有纪念品只读字典
    /// </summary>
    public Dictionary<int, Souvenir> AllSouvenirs => allSouvenirs;

    protected override void Awake()
    {
        base.Awake();
        LoadSaveData();
        CacheOwnedSouvenirInstances();
        LoadNotOwnedData();
    }

    /// <summary>
    /// 暂时用来保存的地方
    /// </summary>
    private void OnDisable()
    {
        SaveData();
    }

    #region 初始化/保存相关
    /// <summary>
    /// 加载存档（无存档则初始化空数据）
    /// </summary>
    private void LoadSaveData()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                saveData = JsonConvert.DeserializeObject<OwnedSouvenirContainer>(json);

                /* 版本兼容
                if (saveData.SaveVersion < 1)
                {
                    UpgradeSaveData(_saveData);
                }*/
            }
            else
            {
                saveData = new OwnedSouvenirContainer();    //{ SaveVersion = 1 };
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载存档失败，重置：{e.Message}");
            saveData = new OwnedSouvenirContainer(); // { SaveVersion = 1 };
        }
    }

    /// <summary>
    /// 缓存已拥有的藏品实例（从存档数据创建）
    /// </summary>
    private void CacheOwnedSouvenirInstances()
    {
        ownedSouvenirs.Clear();
        if (saveData == null) return;

        foreach (var dto in saveData.OwnedSouvenirs)
        {
            SouvenirData souvenirData = souvenirDataManager.GetDataById(dto.Id);
            if (souvenirData == null)
            {
                Debug.LogError($"发现不存在的纪念品，非法Id为：{dto.Id}");
            }

            // 创建已拥有的藏品实例（价格用存档的自定义值）
            Souvenir souvenir = new Souvenir(souvenirData, true)
            {
                Price = dto.CustomPrice
            };
            ownedSouvenirs.Add(dto.Id, souvenir);
            allSouvenirs.Add(dto.Id, souvenir);
        }
    }

    /// <summary>
    /// 保存存档到本地文件（内部逻辑）重新改写savedata
    /// </summary>
    private void SaveData()
    {
        try
        {
            saveData = new OwnedSouvenirContainer();
            foreach(var kt in ownedSouvenirs)
            {
                Souvenir s = kt.Value;
                OwnedSouvenirDTO ownedSouvenirDTO = new OwnedSouvenirDTO(kt.Key, s.Price);
                saveData.OwnedSouvenirs.Add(ownedSouvenirDTO);
            }
            // 序列化存档数据为JSON
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            // 写入持久化路径
            File.WriteAllText(SavePath, json);
            Debug.Log($"存档保存成功 → 路径：{SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"存档保存失败：{e.Message}");
        }
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
            SouvenirData data = souvenirDataManager.GetDataById(id);
            var souvenir = new Souvenir(data);
            allSouvenirs[id] = souvenir;
        }
    }
    #endregion

    #region 接口
    /// <summary>
    /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
    /// </summary>
    public void ManualSave()
    {
        SaveData();
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
        ownedSouvenirs.Remove(id);
        souvenir.SetIsOwned(false, this);
        errorMsg = null;
        return true;
    }

    /// <summary>
    /// 查询纪念品是否拥有
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
    #endregion
}
