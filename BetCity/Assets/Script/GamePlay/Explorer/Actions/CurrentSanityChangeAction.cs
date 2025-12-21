using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    public class CurrentSanityChangeAction : ResourceChangeAction
    {
        public CurrentSanityChangeAction(GameActionContext context, int changeAmount)
            : base(context, ResourceType.CurrentSanity, changeAmount) { }

        public override IEnumerator Perform()
        {
            if (Context.Target is IHasCurrentSanity target)
            {
                if (target.ChangeCurrentSanity(ChangeAmount, this))
                    HasChanged = true;
            }
            yield break;
        }
    }
}
