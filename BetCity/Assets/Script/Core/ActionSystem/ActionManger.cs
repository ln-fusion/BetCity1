using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
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

        private PriorityQueue<GameAction, int> reactions = null;
        //全局订阅
        private static Dictionary<Type, List<DelegateWrapper>> preSubs = new();
        //全局订阅
        private static Dictionary<Type, List<DelegateWrapper>> postSubs = new();
        //执行者
        private static Dictionary<Type, List<DelegateWrapper>> performers = new();
        //最大允许递归层数
        private const int MAX_RECURSION_DEPTH = 10;
        //排队队列
        private PriorityQueue<(GameAction action, Action OnFinished), int> actionQueue = new();
        // CTS（外部/其他方法可操作），CT由它生成
        private CancellationTokenSource actionCts = new();
        //是否是暂停状态
        private bool isPaused = false;
        //暂停锁（临界资源）
        private readonly object pauseLock = new object();
        /// <summary>
        /// 是否有事件在运行
        /// </summary>
        public bool IsPerforming { get; private set; } = false;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        #region 接口
        /// <summary>
        /// 暂停所有正在执行的动作（可恢复）
        /// </summary>
        public void PauseAllActions()
        {
            lock (pauseLock)
            {
                if (isPaused) return;
                isPaused = true;
                Debug.Log("[ActionManager] 所有动作已暂停");
            }
        }

        /// <summary>
        /// 恢复所有暂停的动作
        /// </summary>
        public void ResumeAllActions()
        {
            lock (pauseLock)
            {
                if (!isPaused) return;
                isPaused = false;
                Debug.Log("[ActionManager] 所有动作已恢复");
            }
        }

        /// <summary>
        /// 等待暂停状态解除（供异步流程调用）
        /// </summary>
        public async UniTask WaitIfPaused(CancellationToken cancellationToken)
        {
            while (true)
            {
                lock (pauseLock)
                {
                    if (!isPaused) break; // 未暂停则退出等待
                }
                // 每帧检查一次，同时响应取消信号（如切场景）
                await UniTask.Yield(cancellationToken);
            }
        }

        /// <summary>
        /// 仅执行PreSub进行模拟
        /// </summary>
        public void ExecuteOnlyPreSub(GameAction action)
        {
            if (action == null || !action.IsValid) return;
            // 只执行 PreSub 订阅（核心计算逻辑），跳过所有其他步骤
            PerformSubscribers(action, preSubs, CancellationToken.None);
        }

        /// <summary>
        /// 行为演出
        /// </summary>
        /// <param name="action">行为</param>
        /// <param name="isParallel">是否并行</param>
        /// <param name="OnPerformFinished">完成执行委托</param>
        public void Perform(GameAction action, System.Action OnPerformFinished = null, bool isParallel = false)
        {
            if (action == null)
            {
                Debug.LogError("[ActionManager]Perform: GameAction cannot be null!");
                OnPerformFinished?.Invoke();
                return;
            }

            // 并行执行
            if (isParallel)
            {
                Flow(action, () =>
                {
                    OnPerformFinished?.Invoke();
                }, 0, actionCts.Token).Forget(e =>
                {
                    Debug.LogError($"并行Flow执行异常: {e}");
                });
                return;
            }

            if (IsPerforming)
            {
                actionQueue.Enqueue((action, OnPerformFinished), action.Priority);
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
            }, 0, actionCts.Token).Forget(e =>
            {
                Debug.LogError($"Flow执行异常: {e}");
                IsPerforming = false; // 确保状态重置
                                      // 处理队列中的下一个任务
                if (actionQueue.Count > 0)
                {
                    var (nextAction, nextOnFinished) = actionQueue.Dequeue();
                    Perform(nextAction, nextOnFinished);
                }
            });
        }
        #endregion

        private async UniTask Flow(GameAction action, Action OnFlowFinished = null, int depth = 0, CancellationToken cancellationToken = default)
        {
            action.Depth = depth;
            Debug.Log($"[ActionManager] 开始执行: {action.GetType().Name} (Priority: {action.Priority})");

            if (depth > MAX_RECURSION_DEPTH)
            {
                Debug.LogWarning($"[ActionManager] 递归深度超限，终止执行：{action.GetType().Name}");
                OnFlowFinished?.Invoke();
                return;
            }
            await WaitIfPaused(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            if (!action.IsValid)
            {
                OnFlowFinished?.Invoke();
                return;
            }

            PerformSubscribers(action, preSubs, cancellationToken);
            reactions = action.PreReactions;
            GameAction reaction;

            while(reactions.Count > 0 && action.IsValid && !cancellationToken.IsCancellationRequested)
            {
                await WaitIfPaused(cancellationToken);
                reaction = reactions.Dequeue();
                await Flow(reaction, null, depth + 1, cancellationToken);
            }

            await WaitIfPaused(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            if (!action.IsValid)
            {
                OnFlowFinished?.Invoke();
                return;
            }

            await action.Perform(cancellationToken); // 等待主行为执行
            await PerformPerformer(action);

            await WaitIfPaused(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            if (!action.IsValid)
            {
                OnFlowFinished?.Invoke();
                return;
            }

            PerformSubscribers(action, postSubs, cancellationToken);
            reactions = action.PostReactions;

            while (reactions.Count > 0 && action.IsValid && !cancellationToken.IsCancellationRequested)
            {
                await WaitIfPaused(cancellationToken);
                reaction = reactions.Dequeue();
                await Flow(reaction, null, depth + 1, cancellationToken);
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

        private void PerformSubscribers(GameAction action, Dictionary<Type, List<DelegateWrapper>> subs, CancellationToken cancellationToken)
        {
            //订阅删除其他订阅无法第一时间生效，只会在下一次生效
            List<Type> inheritChain = GetInheritChain(action.GetType());
            PriorityQueue<DelegateWrapper, int> delegateWrappers = new();
            foreach (Type type in inheritChain)
            {
                if (subs.TryGetValue(type, out var wrapperList))
                {
                    wrapperList = wrapperList.OrderBy(w => w.Priority).ToList();
                    foreach (var wrapper in wrapperList)
                    {
                        delegateWrappers.Enqueue(wrapper, wrapper.Priority);
                    }
                }
            }
            while(delegateWrappers.Count > 0 && action.IsValid && !cancellationToken.IsCancellationRequested)
            {
                var wrapper = delegateWrappers.Dequeue();
                wrapper.WrappedAction?.Invoke(action, action.Context);
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

        /// <summary>
        /// 立刻执行子动作，阻塞主动作直到子动作完成（兼容递归深度限制、暂停、取消逻辑）
        /// </summary>
        /// <param name="depth">当前深度（不需要+1）</param>
        /// <param name="childAction">子动作实例</param>
        /// <param name="cancellationToken">取消令牌（透传主动作的取消信号）</param>
        /// <returns>异步任务</returns>
        public async UniTask PerformChildActionAsync(GameAction childAction, int depth, CancellationToken cancellationToken)
        {
            if (childAction == null)
            {
                Debug.LogError("[ActionManager] ExecuteChildActionAsync: 子动作不能为空！");
                return;
            }

            // 合并取消令牌（主动作取消 + 全局动作取消）
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, actionCts.Token);
            var linkedToken = linkedCts.Token;

            await WaitIfPaused(linkedToken);
            if (linkedToken.IsCancellationRequested)
                return;

            await Flow(childAction, null, depth + 1, linkedToken);
        }
    }

}