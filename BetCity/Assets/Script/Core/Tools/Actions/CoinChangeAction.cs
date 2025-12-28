using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

namespace BetCity.Core.Tools
{
    /// <summary>
    /// 钱币变化动作
    /// </summary>
    public class CoinChangeAction : ResourceChangeAction
    {
        public CoinChangeAction(GameActionContext context, int changeAmount)
            : base(context, ResourceType.Coin, changeAmount) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is IHasCoin target)
            {
                if (target.ChangeCoin(ChangeAmount, this))
                    HasChanged = true;
            }
            return UniTask.CompletedTask;
        }
    }

}