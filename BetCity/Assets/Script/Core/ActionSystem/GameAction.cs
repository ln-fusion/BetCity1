using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace BetCity.Core.ActionSystem
{
    /// <summary>
    /// 一个通用游戏行为
    /// </summary>
    public abstract class GameAction
    {
        /// <summary>
        /// 该行为的前置连锁行为
        /// </summary>
        public PriorityQueue<GameAction, int> PreReactions { get; private set; } = new();
        /// <summary>
        /// 该行为的后置连锁行为
        /// </summary>
        public PriorityQueue<GameAction, int> PostReactions { get; private set; } = new();
        /// <summary>
        /// 该行为逻辑内带的连锁行为
        /// </summary>
        public PriorityQueue<GameAction, int> PerformReactions { get; private set; } = new();
        /// <summary>
        /// 上下文信息
        /// </summary>
        public GameActionContext Context { get; protected set; }
        /// <summary>
        /// 该行为的优先级，越低越优先，会影响该行为作为别的行为的前置或是后置连锁行为时的执行顺序
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 该行为是否有效
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 反应演出代码（带检查状态）多帧执行函数需要循环检查取消令牌状态，以及暂停状态
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public abstract UniTask Perform(CancellationToken cancellationToken);

        /// <summary>
        /// 为反应创建上下文信息
        /// </summary>
        /// <returns>上下文信息</returns>
        public virtual GameActionContext CreateContextForReactions<T>() where T : GameAction
        {
            return new GameActionContext(Context.Source, Context.Target, this);
        }

        /// <summary>
        /// 增加一个连锁反应
        /// </summary>
        /// <param name="action">反应类型</param>
        /// <param name="timing">前置/后置反应</param>
        public void EnqueueReaction(GameAction action, ReactionTiming timing)
        {
            PriorityQueue<GameAction, int> reactions = timing == ReactionTiming.PRE ? PreReactions : PostReactions;
            reactions.Enqueue(action, action.Priority);
        }

        /// <summary>
        /// 移除一个特定游戏行为，如果有多个同种游戏行为，只会被删除一个
        /// </summary>
        /// <param name="reaction">反应类型</param>
        /// <param name="timing">前置/后置反应</param>
        public void RemoveReactions(GameAction reaction, ReactionTiming timing)
        {
            PriorityQueue<GameAction, int> reactions = timing == ReactionTiming.PRE ? PreReactions : PostReactions;
            reactions.Remove(reaction);
        }

        /// <summary>
        /// 移除一种游戏行为，如果有多个同种游戏行为，都会被删除
        /// </summary>
        /// <param name="reactionType">反应类型</param>
        /// <param name="timing">前置/后置反应</param>
        public void RemoveReactions(Type reactionType, ReactionTiming timing) 
        {
            RemoveReactions(timing, r => reactionType.IsAssignableFrom(r.GetType()));
        }

        /// <summary>
        /// 移除一个游戏行为(LINQ风格)
        /// </summary>
        /// <param name="timing">前置/后置</param>
        /// <param name="predicate">筛选条件（返回true则删除）</param>
        /// <returns></returns>
        public void RemoveReactions(ReactionTiming timing, Func<GameAction, bool> predicate)
        {
            PriorityQueue<GameAction, int> reactions = timing == ReactionTiming.PRE ? PreReactions : PostReactions;
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate), "筛选条件不能为空");
            var toRemove = reactions.Where(predicate).ToList();
            foreach (var reaction in toRemove)
            {
                reactions.Remove(reaction);
            }
        }

        public GameAction(GameActionContext context)
        {
            Context = context;
        }
    }
}