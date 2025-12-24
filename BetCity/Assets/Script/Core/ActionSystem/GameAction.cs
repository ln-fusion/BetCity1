using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Cysharp.Threading.Tasks;

namespace BetCity.Core.ActionSystem
{
    /// <summary>
    /// 一个通用游戏行为
    /// </summary>
    public abstract class GameAction
    {
        private List<GameAction> preReactions = new List<GameAction>();
        private List<GameAction> postReactions = new List<GameAction>();
        private List<GameAction> performReactions =  new List<GameAction>();

        /// <summary>
        /// 上下文信息
        /// </summary>
        public GameActionContext Context { get; protected set; }
        /// <summary>
        /// 该行为的前置连锁行为
        /// </summary>
        public IReadOnlyList<GameAction> PreReactions => preReactions;
        /// <summary>
        /// 该行为的后置连锁行为
        /// </summary>
        public IReadOnlyList<GameAction> PostReactions => postReactions;
        /// <summary>
        /// 该行为逻辑内带的连锁行为
        /// </summary>
        public IReadOnlyList<GameAction> PerformReactions => performReactions;
        /// <summary>
        /// 该行为的优先级，越低越优先，会影响该行为作为别的行为的前置或是后置连锁行为时的执行顺序
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 该行为是否有效
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 反应演出逻辑
        /// </summary>
        public abstract UniTask Perform(); 

        /// <summary>
        /// 为反应创建上下文信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual GameActionContext CreateContextForReactions<T>() where T : GameAction
        {
            return new GameActionContext(Context.Source, Context.Target, this);
        }

        /// <summary>
        /// 增加一个连锁反应
        /// </summary>
        /// <param name="action">反应类型</param>
        /// <param name="timing">前置/后置反应</param>
        public void AddReaction(GameAction action, ReactionTiming timing)
        {
            List<GameAction> reactions = timing == ReactionTiming.PRE ? preReactions : postReactions;
            reactions.Add(action);
        }

        /// <summary>
        /// 移除一个特定游戏行为，如果有多个同种游戏行为，只会被删除一个
        /// </summary>
        /// <param name="reaction">反应类型</param>
        /// <param name="timing">前置/后置反应</param>
        public void RemoveReactions(GameAction reaction, ReactionTiming timing)
        {
            List<GameAction> reactions = timing == ReactionTiming.PRE ? preReactions : postReactions;
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
            List<GameAction> reactions = timing == ReactionTiming.PRE ? preReactions : postReactions;
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate), "筛选条件不能为空");
            var toRemove = reactions.Where(predicate).ToList();
            foreach (var reaction in toRemove)
            {
                reactions.Remove(reaction);
            }
        }

        /// <summary>
        /// 刷新优先级，除有必要没必要刷新（因为会在执行前统一刷新以节省资源）
        /// </summary>
        public void RefreshPriority(ReactionTiming timing)
        {
            if (timing == ReactionTiming.PRE)
                preReactions = preReactions.OrderBy(r => r.Priority).ToList();
            else
                postReactions = postReactions.OrderBy(r => r.Priority).ToList();
        }

        /// <summary>
        /// 设定优先级
        /// </summary>
        public void SetPriority(int priority)
        {
            Priority = priority;
        }

        public GameAction(GameActionContext context)
        {
            Context = context;
        }
    }
}