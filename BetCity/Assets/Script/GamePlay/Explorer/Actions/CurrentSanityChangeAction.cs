using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 更改当前理智值的动作，source和target均随意
    /// </summary>
    public class CurrentSanityChangeAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            ExplorerPlayerController.Instance.PlayerData.ChangeCurrentSanity(ChangeAmount, this);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 变化量（正数增加，负数减少）
        /// </summary>
        public int ChangeAmount { get; }

        public CurrentSanityChangeAction(GameActionContext context, int changeAmount)
            : base(context)
        {
            ChangeAmount = changeAmount;
        }
    }
}
