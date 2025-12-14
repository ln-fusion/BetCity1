using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责管理所有的CardData即卡牌原型数据
/// </summary>
public class CardDataManager : MonoSingleton<CardDataManager>
{
    /// <summary>
    /// 所有卡牌数据列表（只读对外暴露）
    /// </summary>
    public IReadOnlyList<CardData> Data => _data;
    private List<CardData> _data = new List<CardData>();
    // 数据字典（用于快速通过ID查询）
    private Dictionary<int, CardData> _dataDict = new Dictionary<int, CardData>();

    [Header("数据配置")]
    [SerializeField] private string cardDataResourcesPath;

    /// <summary>
    /// 初始化数据
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        LoadAllCardData();
        ValidateData();

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 从Resources目录加载所有CardData资源
    /// </summary>
    private void LoadAllCardData()
    {
        try
        {
            CardData[] loadedDatas = Resources.LoadAll<CardData>(cardDataResourcesPath);

            if (loadedDatas == null || loadedDatas.Length == 0)
            {
                Debug.LogWarning($"[CardDataManager] 未在Resources/{cardDataResourcesPath}路径下找到任何CardData资源");
                return;
            }

            _data.Clear();
            _dataDict.Clear();

            foreach (CardData data in loadedDatas)
            {
                if (data == null) continue;

                _data.Add(data);
                _dataDict[data.Id] = data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CardDataManager] 加载数据失败：{e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 验证数据合法性
    /// </summary>
    private void ValidateData()
    {
        HashSet<int> idSet = new HashSet<int>();
        List<string> errorMessages = new List<string>();

        foreach (var data in _data)
        {
            // 校验ID唯一性
            if (!idSet.Add(data.Id))
            {
                errorMessages.Add($"重复的卡牌ID：{data.Id}（卡牌名称：{data.CardName}）");
            }

            // 校验必填字段
            if (string.IsNullOrEmpty(data.CardName))
            {
                errorMessages.Add($"ID为{data.Id}的卡牌名称为空");
            }

            // 怪兽卡必须有分数
            if (data.Type == CardType.Monster && data.MonsterScore < 0)
            {
                errorMessages.Add($"ID为{data.Id}的怪兽卡分数无效：{data.MonsterScore}");
            }
        }

        if (errorMessages.Count > 0)
        {
            string errorMsg = $"[CardDataManager] 数据校验失败，共{errorMessages.Count}个错误：\n{string.Join("\n", errorMessages)}";
            Debug.LogError(errorMsg);
        }
    }

    /// <summary>
    /// 通过ID查询卡牌原型数据
    /// </summary>
    public CardData GetDataById(int id)
    {
        _dataDict.TryGetValue(id, out var result);
        return result;
    }
}