using BetCity.Core.ActionSystem;
using BetCity.Core.EffectSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BetCity.Core.EffectSystem 
{
    public class EffectManager : MonoSingleton<EffectManager>
    {
        [Label("效果Id-执行函数名称映射字典"), SerializeField] private SerializableDictionary<int, string> passiveEffectNameDict;
        //存放被动效果id-订阅guid映射
        private Dictionary<int, Guid> passiveEffectGuidDict = new();
        //存放限时被动效果id-结束回合订阅guid映射
        private Dictionary<int, Guid> timedEffectEndTurnGuidDict = new();
        //存放被动效果id-触发条件映射
        private Dictionary<int, TriggerCondition> passiveEffectTriggerConditionDict = new();
        //存放限时被动效果id-结束回合类型映射
        private Dictionary<int, Type> timedEffectEndTurnTypeDict = new();
        //存放限时效果持续时间，结束回合类型-（id+持续时间）映射
        private Dictionary<Type, List<(int, int)>> timedEffectDurationDict = new();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 激活永久的被动效果
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="carrier">效果载体类型</param>
        /// <param name="condition">触发条件</param>
        /// <returns>成功与否</returns>
        public bool ActivatePermanentPassiveEffect(int id, EffectCarrier carrier, TriggerCondition condition)
        {
            Guid guid;
            Type conditionType = condition.TargetActionType;
            bool isSub = typeof(GameAction).IsAssignableFrom(conditionType);
            if (!isSub)
            {
                Debug.LogWarning($"[EffectManager]非法的触发条件，不是GameAction的子类！");
                return false;
            }
            if (passiveEffectGuidDict.ContainsKey(id) || passiveEffectTriggerConditionDict.ContainsKey(id))
            {
                //重复获得直接返回true
                Debug.LogWarning($"[EffectManager]试图获取一个已经拥有的永久被动效果！");
                return true;
            }

            // 获取泛型方法的定义（SubscribeReaction<T>）
            var methodDef = typeof(ActionManager)
                .GetMethod("SubscribeReaction", BindingFlags.Static | BindingFlags.Public);
            // 绑定泛型类型（传入conditionType）
            var genericMethod = methodDef.MakeGenericMethod(conditionType);
            bool val = passiveEffectNameDict.TryGetValue(id, out string passiveEffectName);
            if (!val)
            {
                Debug.LogWarning($"[EffectManager]非法的Id:{id}调用被动效果失败，请检查效果Id-执行函数名称映射字典（passiveEffectNameDict）字典");
                return false;
            }

            switch (carrier)
            {
                //纪念品效果
                case EffectCarrier.Souvenir:
                    var action = SouvenirEffectFactory.GetPassiveEffect(passiveEffectName);
                    if(action == null)
                    {
                        Debug.LogWarning($"[EffectManager]SouvenirEffecfFactory中未找到对应效果名为{passiveEffectName}的一次性被动效果执行函数！");
                        return false;
                    }
                    // 调用方法
                    guid = (Guid)genericMethod.Invoke(null, new object[] { action, condition.Timing, condition.Priority });
                    if (guid == null)
                    {
                        Debug.LogWarning($"[EffectManager]非法的Id:{id}对应的函数名称无法在中查找到，请检查passiveEffectDict");
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            passiveEffectGuidDict.Add(id, guid);
            passiveEffectTriggerConditionDict.Add(id, condition);
            return true;
        }

        /// <summary>
        /// 激活时效性的被动效果
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="carrier">效果载体类型</param>
        /// <param name="condition">触发条件</param>
        /// <param name="endTurnType">结束回合判定的游戏动作类型</param>
        /// <param name="duration">持续回合数</param>
        /// <returns>成功与否</returns>
        public bool ActivateTimedPassiveEffect(int id, EffectCarrier carrier, TriggerCondition condition, int duration, EndTurnReaction endTurnReaction)
        {
            Type endTurnType = endTurnReaction.TargetActionType;
            int endTurnSubPriority = endTurnReaction.Priority;
            if (duration < 0)
            {
                Debug.LogWarning($"[EffectManager]试图激活一个持续时间小于1回合的被动效果！");
                return false;
            }

            if (passiveEffectGuidDict.ContainsKey(id))
            {
                if (!timedEffectEndTurnGuidDict.ContainsKey(id) || !timedEffectEndTurnTypeDict.ContainsKey(id) || !timedEffectDurationDict.ContainsKey(endTurnType))
                {
                    Debug.LogWarning($"[EffectManager]试图激活的限时被动效果字典注册出现问题！");
                    return false;
                }
                List<(int, int)> pairs = timedEffectDurationDict[endTurnType];
                for (int i = 0; i < pairs.Count; i++)
                {
                    if (pairs[i].Item1 == id)
                    {
                        duration = pairs[i].Item2 + duration;
                        pairs[i] = (id, duration);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                //先当永久注册
                bool val = ActivatePermanentPassiveEffect(id, carrier, condition);

                //注册回合结束动作回调
                var methodDef = typeof(ActionManager)
                     .GetMethod("SubscribeReaction", BindingFlags.Static | BindingFlags.Public);
                var genericMethod = methodDef.MakeGenericMethod(endTurnType);
                  
                //转化为Action<endTurnType>
                var endTurnMethodDef = typeof(EffectManager)
                .GetMethod(
                    nameof(EndTurnSub),
                    BindingFlags.NonPublic | BindingFlags.Instance, // 私有实例方法需指定BindingFlags
                    Type.DefaultBinder,
                    new[] { Type.MakeGenericMethodParameter(0) }, // 泛型参数占位符
                    null
                );
                endTurnMethodDef = endTurnMethodDef.MakeGenericMethod(endTurnType);
                var actionDelegateType = typeof(Action<>).MakeGenericType(endTurnType);

                Delegate actionDelegate = Delegate.CreateDelegate(
                    actionDelegateType, // 委托类型：Action<具体T>
                    this,               // 当前实例（因为EndTurnSub是实例方法）
                    endTurnMethodDef    // 绑定后的具体方法
                );

                Guid guid = (Guid)genericMethod.Invoke(null, new object[] { actionDelegate, ReactionTiming.POST, endTurnSubPriority });
                if (guid == null)
                {
                    Debug.LogWarning($"[EffectManager]限时的动作订阅回合结束动作回调时出现未知问题");
                    return false;
                }

                timedEffectEndTurnGuidDict.Add(id, guid);
                timedEffectEndTurnTypeDict.Add(id, endTurnType);
                //存放限时效果持续时间
                if (timedEffectDurationDict.ContainsKey(endTurnType))
                {
                    List<(int, int)> durations = timedEffectDurationDict[endTurnType];
                    durations.Add((id, duration));
                }
                else
                {
                    List<(int, int)> durations = new List<(int, int)>();
                    durations.Add((id, duration));
                    timedEffectDurationDict[endTurnType] = durations;                   
                }
            }
            return true;
        }

        /// <summary>
        /// 激活一次性效果
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="carrier">效果持有者</param>
        /// <returns>成功与否</returns>
        public bool ActivateOneShotPassiveEffect(int id, EffectCarrier carrier)
        {
            bool val = passiveEffectNameDict.TryGetValue(id, out var effectName);
            if (!val)
            {
                Debug.LogWarning($"[EffectManager]非法的Id:{id}调用被动效果失败，请检查效果Id-执行函数名称映射字典（passiveEffectNameDict）字典");
                return false;
            }

            //想要使用一次性被动效果，请在factory中实现public static bool ActivateOneShotEffect(string effectName)
            switch (carrier)
            {
                //纪念品效果
                case EffectCarrier.Souvenir:
                    val = SouvenirEffectFactory.ActivateOneShotEffect(effectName);
                    Debug.LogWarning($"[EffectManager]纪念品暂时不支持一次性效果");
                    return false;
            }
            if (!val)
            {
                Debug.LogWarning($"[EffectManager]执行Id:{id}一次性效果失败，请检查效果Id-执行函数名称映射字典（passiveEffectNameDict）字典是否正确并确保执行函数是否无误");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取消激活被动效果
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="condition">触发条件</param>
        /// <returns>成功与否</returns>
        public bool DeActivatePassiveEffect(int id)
        {
            if (!passiveEffectGuidDict.ContainsKey(id) || !passiveEffectTriggerConditionDict.ContainsKey(id))
            {
                Debug.LogWarning($"[EffectManager]非法的Id:{id}对应的函数未订阅或是不存在");
                return false;
            }
            TriggerCondition condition = passiveEffectTriggerConditionDict[id];
            Guid guid = passiveEffectGuidDict[id];
            var methodDef = typeof(ActionManager)
                   .GetMethod("UnsubscribeReaction", BindingFlags.Static | BindingFlags.Public);
            var genericMethod = methodDef.MakeGenericMethod(condition.TargetActionType);
            bool val = (bool)genericMethod.Invoke(null, new object[] { guid, condition.Timing });
            passiveEffectGuidDict.Remove(id);
            passiveEffectTriggerConditionDict.Remove(id);
            return val;
        }

        /// <summary>
        /// 取消激活限时效果的结束回合订阅
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>成功与否</returns>
        public bool DeActivateTimedEffectEndTurnSubscribe(int id)
        {
            bool val = timedEffectEndTurnTypeDict.TryGetValue(id, out Type endTurnType);
            if (!val)
            {
                Debug.LogWarning($"[EffectManager]对应Id:{id}的限时被动效果结束回合动作未找到");
                return false;
            }
            timedEffectEndTurnTypeDict.Remove(id);
            val = timedEffectEndTurnGuidDict.TryGetValue(id, out Guid guid);
            if (!val)
            {
                Debug.LogWarning($"[EffectManager]对应Id:{id}的限时被动效果结束回合动作注册的guid未找到");
                return false;
            }
            timedEffectEndTurnGuidDict.Remove(id);
            var methodDef = typeof(ActionManager)
                       .GetMethod("UnsubscribeReaction", BindingFlags.Static | BindingFlags.Public);
            var genericMethod = methodDef.MakeGenericMethod(endTurnType);
            return (bool)genericMethod.Invoke(null, new object[] { guid, ReactionTiming.POST });
        }

        private void EndTurnSub<T>(T action) where T : GameAction
        {
            List<(int, int)> list = timedEffectDurationDict[typeof(T)];
            for (int i = 0; i < list.Count; i++)
            {
                int duration = list[i].Item2 - 1;
                if (duration == 0)
                {
                    int id = list[i].Item1;
                    list.Remove(list[i]);
                    i--;

                    //删除结束回合订阅
                    bool val1 = DeActivateTimedEffectEndTurnSubscribe(id);
                    if (!val1) Debug.LogWarning("取消被动效果的回合结束动作的订阅失败");

                    //删除效果订阅
                    val1 = DeActivatePassiveEffect(id);
                    if (!val1) Debug.LogWarning("取消激活被动效果失败！");
                }
                else
                {
                    list[i] = (list[i].Item1, duration);
                }
            }
            if (list.Count == 0)
            {
                timedEffectDurationDict.Remove(typeof(T));
            }
        }
    }
}
