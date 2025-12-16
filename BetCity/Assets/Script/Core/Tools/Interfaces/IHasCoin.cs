using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    public interface IHasCoin
    {
        /// <summary>
        /// 金币
        /// </summary>
        int Coin { get;} 
        /// <summary>
        /// 变换金币
        /// </summary>
        bool ChangeCoin(int amount, CoinChangeAction caller);
    }
}
