using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 经济系统单例管理
/// </summary>
public class EconomySystem : MonoSingleton<EconomySystem>
{
    /// <summary>
    /// 金币
    /// </summary>
    public int Coin {  get; private set; }

    #region 接口
    /// <summary>
    /// 变化金币，消费变为负数
    /// </summary>
    /// <param name="amount">数量</param>
    /// <returns>成功与否</returns>
    public bool ChangeCoin(int amount)
    {
        if(Coin + amount >= 0)
        {
            Coin += amount;
            return true;
        }
        return false;
    }
    #endregion
}
