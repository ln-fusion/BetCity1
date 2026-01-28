using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 买商品的动作，Target为Product
    /// </summary>
    public class OnPurchaseAction : GameAction
    {
        /// <summary>
        /// 为true用金币否则用理智
        /// </summary>
        public bool UseCoin {  get;}

        public OnPurchaseAction(GameActionContext context, bool useCoin) : base(context)
        {
            UseCoin = useCoin;
        }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (UseCoin)
            {
                
            }
            throw new System.NotImplementedException();
        }
    }
}
