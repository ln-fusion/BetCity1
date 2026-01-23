using BetCity.Core.Tools;
using DG.Tweening.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.CheckSystem
{
    /// <summary>
    /// 条件判断器
    /// </summary>
    public class ConditionChecker : MonoSingleton<ConditionChecker>
    {
        //条件判断字符串-委托映射字典
        private Dictionary<string, Func<string[], bool>> conditionDict;

        protected void Awake()
        {
            base.Awake();
            //初始化字典
            conditionDict = new()
            {
                {"ForeverFalse", ForeverFalse},
                {"ForeverTrue", ForeverTrue }
            };
        }
        
        /// <summary>
        /// 检查条件
        /// </summary>
        /// <param name="conditions">条件判断字典</param>
        /// <returns>是否合法</returns>
        public bool Check(Dictionary<string, List<string>> conditions)
        {
            foreach(var kp in conditions)
            {
                if (!conditionDict.ContainsKey(kp.Key))
                {
                    Debug.LogWarning($"[ConditionChecker]condtionDict中并不存在{kp.Key}的判断函数，请检查！");
                    return false;
                }
                else if(!conditionDict[kp.Key](kp.Value.ToArray()))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 永错判断，主要用于调试
        /// </summary>
        public bool ForeverFalse(string[] args)
        {
            return false;
        }

        /// <summary>
        /// 永对判断，主要用于调试
        /// </summary>
        public bool ForeverTrue(string[] args)
        {
            return true;
        }
    }
}

