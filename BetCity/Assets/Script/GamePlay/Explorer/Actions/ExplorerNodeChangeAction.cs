using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
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

        /// <summary>
        /// 目标结点
        /// </summary>
        public Node TargetNode { get; }

        public ExplorerNodeChangeAction(GameActionContext context, Node targetNode)
            : base(context)
        {
            TargetNode=targetNode;
        }
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is ExplorerPlayerController playerController)
            {
                await playerController.Move(TargetNode);
            }
            return;
        }
    }

}
