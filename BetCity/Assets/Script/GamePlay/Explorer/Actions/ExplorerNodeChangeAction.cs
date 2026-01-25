using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using BetCity.Data.ConfigModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 玩家结点移动类
    /// </summary>
    public class ExplorerNodeChangeAction : GameAction
    {
        /// <summary>
        /// 是否执行完修改，如果修改不合法一直为false
        /// </summary>
        public bool HasChanged { get; protected set; } = false;


        public ExplorerNodeChangeAction(GameActionContext context)
            : base(context)
        {
            EnqueueReaction(new OnExitNodeAction(context),ReactionTiming.PRE);
            EnqueueReaction(new OnEnterNodeAction(context),ReactionTiming.POST);
        }
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if(Context.Target is Node a)
            {
                await ExplorerPlayerController.Instance.Move(a);
            }
            return;
        }
    }

}
