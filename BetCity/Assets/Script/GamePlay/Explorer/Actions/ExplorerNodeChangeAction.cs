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
    /// 玩家结点移动类，在本类中，context的source是当前结点，target是目标节点
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
            if (context.Target is not Node||context.Source is not Node)
            {
                Debug.LogError("ExplorerNodeChangeAction:Context.Target/Source不是一个节点！");
                IsValid = false;
                return;
            }

            if (!ExplorerPlayerController.Instance.NodeJudge((Node)context.Source, (Node)context.Target))
            {
                IsValid = false;
                return;
            }
        }
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            
            await ActionManager.Instance.PerformChildActionAsync(new OnExitNodeAction(Context), Depth, cancellationToken);

            if (Context.Target is Node a)
            {
                await ExplorerPlayerController.Instance.Move(a,cancellationToken);
            }

            await ActionManager.Instance.PerformChildActionAsync(new OnEnterNodeAction(Context), Depth, cancellationToken);
            IsValid = true;
            return;
        }
    }

}
