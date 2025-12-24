using BetCity.Core.ActionSystem;
using System.Collections;
using BetCity.Core.Tools;

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BetCity.GamePlay.Explorer
{
    public class CurrentActionPointChangeAction : ResourceChangeAction
    {
        public CurrentActionPointChangeAction(GameActionContext context, int changeAmount)
            : base(context, ResourceType.ActionPoint, changeAmount) { }

        public override async UniTask Perform()
        {
            if (Context.Target is IHasCurrentActionPoint target)
            {
                if (target.ChangeCurrentActionPoint(ChangeAmount, this))
                    HasChanged = true;
            }
            await UniTask.CompletedTask;
        }
    }
}
