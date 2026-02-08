using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 进入任务板节点动作,SourceTarget均为节点
    /// </summary>
    public class OnEnterTaskNodeAction : OnEnterNodeAction
    {
        public OnEnterTaskNodeAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);

            if (Context.Target is Node node)
            {
                TaskEventManager.Instance.EnterEvent(cancellationToken, node.EventId);
            }
            else
            {
                IsValid = false;
                Debug.LogWarning("[OnEnterStoreNodeAction]传入错误Context信息！");
            }
            return UniTask.CompletedTask;
        }
    }
}
