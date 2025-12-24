using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace BetCity.Core.ActionSystem
{

    /// <summary>
    /// 游戏行为单例管理者
    /// </summary>
    public class ActionManager : MonoSingleton<ActionManager>
    {
        // 委托包装类
        private class DelegateWrapper
        {
            public int Priority { get; }
            public Guid Guid { get; }
            public Delegate OriginalDelegate { get; }
            public Action<GameAction, GameActionContext> WrappedAction { get; }
            public Func<GameAction, UniTask> WrappedPerformer { get; }

            // 订阅回调包装
            public DelegateWrapper(Guid guid, Delegate original, Action<GameAction, GameActionContext> wrapped, int priority)
            {
                this.Guid = guid;
                OriginalDelegate = original;
                WrappedAction = wrapped;
                Priority = priority;
            }

            // 演出逻辑包装
            public DelegateWrapper(Guid guid, Delegate original, Func<GameAction, UniTask> wrapped, int priority)
            {
                this.Guid = guid;
                OriginalDelegate = original;
                WrappedPerformer = wrapped;
                Priority = priority;
            }
        }

        private IReadOnlyList<GameAction> reactions = null;
        //全局订阅
        private static Dictionary<Type, List<DelegateWrapper>> preSubs = new();
        //全局订阅
        private static Dictionary<Type, List<DelegateWrapper>> postSubs = new();
        //执行者
        private static Dictionary<Type, List<DelegateWrapper>> performers = new();
        //最大允许递归层数
        private const int MAX_RECURSION_DEPTH = 10;
        //排队队列
        private Queue<(GameAction action, Action OnFinished)> actionQueue = new Queue<(GameAction, Action)>();
        /// <summary>
        /// 是否有事件在运行
        /// </summary>
        public bool IsPerforming { get; private set; } = false;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 行为演出
        /// </summary>
        /// <param name="action">行为</param>
        /// <param name="OnPerformFinished">完成执行委托</param>
        public void Perform(GameAction action, System.Action OnPerformFinished = null)
        {
            if (action == null)
            {
                Debug.LogError("[ActionManager]Perform: GameAction cannot be null!");
                OnPerformFinished?.Invoke();
                return;
            }
            if (IsPerforming)
            {
                actionQueue.Enqueue((action, OnPerformFinished));
                return;
            }

            IsPerforming = true;
            // 改为 UniTask 执行
            Flow(action, () =>
            {
                IsPerforming = false;
                OnPerformFinished?.Invoke();
                if (actionQueue.Count > 0)
                {
                    var (nextAction, nextOnFinished) = actionQueue.Dequeue();
                    Perform(nextAction, nextOnFinished);
                }
            }).Forget(); // 使用 Forget() 忽略返回值，避免警告
        }

        private async UniTask Flow(GameAction action, Action OnFlowFinished = null, int depth = 0)
        {
            Debug.Log($"[ActionManager] 开始执行: {action.GetType().Name} (Priority: {action.Priority})");

            if (depth > MAX_RECURSION_DEPTH)
            {
                Debug.LogWarning($"[ActionManager] 递归深度超限（当前{depth}层，最大{MAX_RECURSION_DEPTH}层），终止执行行为：{action.GetType().Name}");
                OnFlowFinished?.Invoke();
                return;
            }

            if (!action.IsValid)
            {
                OnFlowFinished?.Invoke();
                return;
            }

            PerfromSubscribers(action, preSubs);
            reactions = action.PreReactions;

            foreach (var reaction in reactions)
            {
                if (!action.IsValid)
                {
                    OnFlowFinished?.Invoke();
                    return;
                }
                if (reaction.IsValid)
                    await Flow(reaction, null, depth + 1); // 改为 await
            }

            reactions = action.PerformReactions;
            await action.Perform(); // 等待主行为执行
            await PerformPerformer(action); // 等待执行者执行
            foreach (var reaction in reactions)
            {
                if (!action.IsValid)
                {
                    OnFlowFinished?.Invoke();
                    return;
                }
                if (reaction.IsValid)
                    await Flow(reaction, null, depth + 1); // 改为 await
            }

            PerfromSubscribers(action, postSubs);
            reactions = action.PostReactions;
            foreach (var reaction in reactions)
            {
                if (!action.IsValid)
                {
                    OnFlowFinished?.Invoke();
                    return;
                }
                if (reaction.IsValid)
                    await Flow(reaction, null, depth + 1); // 改为 await
            }

            Debug.Log($"[ActionManager] 完成执行: {action.GetType().Name}");
            OnFlowFinished?.Invoke();
        }

        private async UniTask PerformPerformer(GameAction action)
        {
            Type type = action.GetType();
            if (performers.TryGetValue(type, out var wrapperList))
            {
                foreach (var wrapper in wrapperList)
                {
                    if (wrapper.WrappedPerformer != null)
                        await wrapper.WrappedPerformer(action); 
                }
            }
        }

        private void PerfromSubscribers(GameAction action, Dictionary<Type, List<DelegateWrapper>> subs)
        {
            List<Type> inheritChain = GetInheritChain(action.GetType());

            foreach (Type type in inheritChain)
            {
                if (subs.TryGetValue(type, out var wrapperList))
                {
                    wrapperList = wrapperList.OrderBy(w => w.Priority).ToList();
                    //tolist产生快照，允许subscriber删除subscriber，尽量不要增加订阅，否则需要在该轮手动执行，但是会失去优先级判断
                    foreach (var wrapper in wrapperList.ToList())
                    {
                        if (wrapperList.Contains(wrapper))
                        {
                            wrapper.WrappedAction?.Invoke(action, action.Context);
                        }
                    }
                }
            }
        }

        // 辅助方法：获取类型的继承链（自身 → 父类 → ... → GameAction）
        private List<Type> GetInheritChain(Type targetType)
        {
            List<Type> chain = new List<Type>();
            Type currentType = targetType;

            // 遍历直到GameAction
            while (currentType != null && currentType != typeof(GameAction).BaseType)
            {
                if (typeof(GameAction).IsAssignableFrom(currentType))
                {
                    chain.Add(currentType);
                }
                currentType = currentType.BaseType;
            }

            // 执行顺序：父类 → 子类（通用逻辑先执行，子类逻辑后执行）
            // 若想子类→父类，反转列表即可：chain.Reverse();
            return chain;
        }

        /// <summary>
        /// 为游戏行为类型T新增演出逻辑
        /// </summary>
        /// <typeparam name="T">游戏行为类型</typeparam>
        /// <param name="performer">演出逻辑</param>
        /// <param name="priority">优先级</param>
        /// <returns>唯一标识GUID（用于取消订阅）</returns>
        public static Guid AttachPerformer<T>(Func<T, UniTask> performer, int priority = 0) where T : GameAction
        {
            if (performer == null)
            {
                Debug.LogError($"[ActionManager]AttachPerformer<{typeof(T).Name}>: Performer cannot be null!");
                return Guid.Empty;
            }

            Type type = typeof(T);
            Guid guid = Guid.NewGuid();
            UniTask wrappedPerformer(GameAction action) => performer((T)action);
            // 创建包装类实例
            var wrapper = new DelegateWrapper(guid, performer, wrappedPerformer, priority);
            if (!performers.ContainsKey(type))
            {
                performers[type] = new List<DelegateWrapper>();
            }
            performers[type].Add(wrapper);
            return guid;
        }

        /// <summary>
        /// 移除指定GUID的演出逻辑
        /// </summary>
        /// <typeparam name="T">游戏行为类型</typeparam>
        /// <param name="guid">订阅时返回的唯一标识</param>
        /// <returns>是否移除成功</returns>
        public static bool DetachPerformer<T>(Guid guid) where T : GameAction
        {
            Type type = typeof(T);
            if (!performers.ContainsKey(type))
                return false;

            var wrapperToRemove = performers[type].FirstOrDefault(w => w.Guid == guid);
            if (wrapperToRemove != null)
            {
                performers[type].Remove(wrapperToRemove);

                // 清理空列表
                if (performers[type].Count == 0)
                    performers.Remove(type);
                return true;
            }

            Debug.LogWarning($"[ActionManager]DetachPerformer<{typeof(T).Name}>: GUID {guid} not found!");
            return false;
        }

        /// <summary>
        /// 全局订阅游戏行为回调
        /// </summary>
        /// <typeparam name="T">游戏行为类型</typeparam>
        /// <param name="reaction">回调逻辑</param>
        /// <param name="timing">执行时机</param>
        /// <param name="priority">优先级</param>
        /// <returns>唯一标识GUID（用于取消订阅）</returns>
        public static Guid SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing, int priority = 0) where T : GameAction
        {
            if (reaction == null)
            {
                Debug.LogError($"[ActionManager]SubscribeReaction<{typeof(T).Name}>: Reaction cannot be null!");
                return Guid.Empty;
            }

            Type type = typeof(T);
            Guid guid = Guid.NewGuid();
            Dictionary<Type, List<DelegateWrapper>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;

            // 包装委托，适配通用类型
            void wrappedReaction(GameAction action, GameActionContext context) => reaction((T)action);

            // 创建包装类实例
            var wrapper = new DelegateWrapper(guid, reaction, wrappedReaction, priority);

            // 添加到订阅列表
            if (!subs.ContainsKey(type))
            {
                subs[type] = new List<DelegateWrapper>();
            }
            subs[type].Add(wrapper);

            return guid;
        }

        /// <summary>
        /// 全局取消订阅游戏行为回调
        /// </summary>
        /// <typeparam name="T">游戏行为类型</typeparam>
        /// <param name="guid">订阅时返回的唯一标识</param>
        /// <param name="timing">执行时机</param>
        /// <returns>是否取消成功</returns>
        public static bool UnsubscribeReaction<T>(Guid guid, ReactionTiming timing) where T : GameAction
        {
            Type type = typeof(T);
            Dictionary<Type, List<DelegateWrapper>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;

            if (!subs.ContainsKey(type))
                return false;

            var wrapperToRemove = subs[type].FirstOrDefault(w => w.Guid == guid);
            if (wrapperToRemove != null)
            {
                subs[type].Remove(wrapperToRemove);

                // 清理空列表
                if (subs[type].Count == 0)
                    subs.Remove(type);

                return true;
            }

            Debug.LogWarning($"[ActionManager]UnsubscribeReaction<{typeof(T).Name}>: GUID {guid} not found!");
            return false;
        }
       
    }

}