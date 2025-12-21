using BetCity.Core.ActionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    public class CurrentActionPointChangeAction : ResourceChangeAction
    {
        public CurrentActionPointChangeAction(GameActionContext context, int changeAmount)
            : base(context, ResourceType.ActionPoint, changeAmount) { }

        public override IEnumerator Perform()
        {
            if (Context.Target is IHasCurrentActionPoint target)
            {
                if (target.ChangeCurrentActionPoint(ChangeAmount, this))
                    HasChanged = true;
            }
            yield break;
        }
    }
}
