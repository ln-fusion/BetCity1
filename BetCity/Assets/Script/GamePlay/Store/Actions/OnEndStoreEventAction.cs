using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 结束商店事件
    /// </summary>
    public class OnEndStoreEventAction : GameAction
    {
        public OnEndStoreEventAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            StoreEventManager.Instance.EndEvent(cancellationToken);
            return UniTask.CompletedTask;
        }
    }
}
