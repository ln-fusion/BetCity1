using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    public class DisplayRefreshAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            Debug.LogWarning("REFRESH");
            ExplorerPlayerController.Instance.RenewScreen();
            return UniTask.CompletedTask;
        }


        public DisplayRefreshAction(GameActionContext context)
            : base(context)
        {

        }
    }
}
