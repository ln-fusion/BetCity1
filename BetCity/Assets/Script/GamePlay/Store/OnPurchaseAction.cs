using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 买商品的动作，Source为P
    /// </summary>
    public class OnPurchaseAction : GameAction
    {
        public OnPurchaseAction(GameActionContext context) : base(context)
        {

        }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
