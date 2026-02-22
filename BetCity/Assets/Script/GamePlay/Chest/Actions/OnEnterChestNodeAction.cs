using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Store;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Chest
{
    /// <summary>
    /// 进入宝箱节点动作,SourceTarget均为节点
    /// </summary>
    public class OnEnterChestNodeAction : OnEnterNodeAction
    {

        public OnEnterChestNodeAction(GameActionContext context)
    : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);
            if (Context.Target is Node node)
            {
                if (node.EventType != TypeOfEvent.City)
                {
                    ChestEventManager.Instance.EnterEvent(cancellationToken, node.EventId);
                    if(ChestEventManager.Instance.CheckChestOption(cancellationToken))
                    {
                        IsValid = false;
                        Debug.LogWarning("[OnEnterChestNodeAction]此宝箱结点没有可以选择的项！");
                    }
                }
                else
                {
                    //城市加载逻辑
                    throw new NotImplementedException();
                }
            }
            else
            {
                IsValid = false;
                Debug.LogWarning("[OnEnterChestNodeAction]传入错误Context信息！");
            }
            return UniTask.CompletedTask;
        }
    }
}
