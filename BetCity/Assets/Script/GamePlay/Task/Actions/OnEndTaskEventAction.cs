using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Store;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 结束任务板事件
    /// </summary>
    public class OnEndTaskEventAction : GameAction
    {
        public OnEndTaskEventAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            TaskEventManager.Instance.EndEvent(cancellationToken);
            return UniTask.CompletedTask;
        }
    }
}
