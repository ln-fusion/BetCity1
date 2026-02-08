using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 进入商店节点动作,SourceTarget均为节点
    /// </summary>
    public class OnEnterStoreNodeAction : OnEnterNodeAction
    {
        public OnEnterStoreNodeAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);
            if(Context.Target is Node node)
            {
                if(node.EventType != TypeOfEvent.City)
                {
                    StoreEventManager.Instance.EnterEvent(cancellationToken, node.EventId);
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
                Debug.LogWarning("[OnEnterStoreNodeAction]传入错误Context信息！");
            }
            return UniTask.CompletedTask;
        }
    }
}
