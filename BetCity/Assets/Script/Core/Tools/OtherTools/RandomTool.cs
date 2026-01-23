using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 与随机相关的静态工具类
/// </summary>
public static class RandomTool 
{
    /// <summary>
    /// 按权重随机抽取【指定数量】的不重复下标
    /// </summary>
    /// <param name="weightList">权重数组，List<int> 下标=物品下标，值=对应权重</param>
    /// <param name="amount">需要抽取的数量</param>
    /// <returns>选中的下标列表（无重复）</returns>
    public static List<int> GetWeightRandomIndexNoRepeat(List<int> weightList, int amount)
    {
        List<int> result = new List<int>();
        if (weightList == null || weightList.Count == 0 || amount <= 0) return result;

        Dictionary<int, int> tempWeightDict = weightList.Select((value, index) => new { index, value })  
                                                        .ToDictionary(x => x.index, x => x.value);

        for (int i = 0; i < amount; i++)
        {
            if (tempWeightDict.Count == 0) break;

            int totalWeight = tempWeightDict.Sum(w => w.Value);
            int randomNum = UnityEngine.Random.Range(0, totalWeight);
            int sum = 0;
            int selectIndex = -1;

            foreach(var kVPair in tempWeightDict)
            {
                sum += kVPair.Value;
                if(sum > randomNum)
                {
                    selectIndex = kVPair.Key;
                    break;
                }
            }
            if(selectIndex != -1 && tempWeightDict.ContainsKey(selectIndex))
            {
                result.Add(selectIndex);
                tempWeightDict.Remove(selectIndex);
            }
        }
        return result;
    }
}
