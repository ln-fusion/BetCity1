using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 退出任务板节点动作
    /// </summary>
    public class OnExitTaskNodeAction : OnExitNodeAction
    {
        public OnExitTaskNodeAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);
            TaskEventManager.Instance.ExitEvent(cancellationToken);
            return UniTask.CompletedTask;
        }
    }

}