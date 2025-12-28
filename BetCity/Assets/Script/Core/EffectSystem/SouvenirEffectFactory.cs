using BetCity.Core.ActionSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.EffectSystem
{
    /// <summary>
    /// 纪念品效果工厂
    /// </summary>
    public static class SouvenirEffectFactory
    {
        /// <summary>
        /// 获取被动效果
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns></returns>
        public static Action<GameAction> GetPassiveEffect(string effectName)
        {
            switch (effectName)
            {
                default:
                    return Test;
            }
        }

        public static bool ActivateOneShotEffect(string effectName)
        {
            switch (effectName)
            {
                default:
                    return false;
            }
        }

        private static void Test(GameAction action)
        {
            Debug.Log("Test已执行！");
        }
    }
}

