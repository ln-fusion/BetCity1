using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Store;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    /// <summary>
    /// 进入城市节点动作,SourceTarget均为节点
    /// </summary>
    public class OnEnterCityNodeAction : OnEnterNodeAction
    {

        public OnEnterCityNodeAction(GameActionContext context) : base(context) { }
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);
            if (Context.Target is Node node)
            {
                if (node.EventType == TypeOfEvent.City)
                {
                    CityEventManager.Instance.EnterEvent(cancellationToken,node.EventId);
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
                Debug.LogWarning("[OnEnterCityNodeAction]传入错误Context信息！");
            }
            return UniTask.CompletedTask;
        }
    }
}
