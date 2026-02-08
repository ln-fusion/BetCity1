using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Task;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    public class OnExitStoreNodeAction : OnExitNodeAction
    {
        public OnExitStoreNodeAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            base.Perform(cancellationToken);
            StoreEventManager.Instance.ExitEvent(cancellationToken);
            return UniTask.CompletedTask;
        }
    }
}
