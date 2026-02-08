using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 接取动作任务动作，Target为对应任务id
    /// </summary>
    public class OnAcceptTaskAction : GameAction
    {
        public OnAcceptTaskAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is not int id)
            {
                Debug.LogError("[OnAccpetTaskAction]传入的Context的Target不是int！");
                IsValid = false;
                return UniTask.CompletedTask;
            }
            else if (!TaskManager.Instance.AcceptTask(id))
            {
                IsValid = false;
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }
    }

}