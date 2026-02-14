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
    /// 更改当前AP数值的动作，source和target均随意，传入的int是变化值
    /// </summary>
    public class CurrentActionPointChangeAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            ExplorerPlayerController.Instance.PlayerData.ChangeCurrentActionPoint(ChangeAmount, this);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 变化量（正数增加，负数减少）
        /// </summary>
        public int ChangeAmount { get; }

        public CurrentActionPointChangeAction(GameActionContext context,int changeAmount)
            : base(context)
        {
            ChangeAmount = changeAmount;
        }
    }
}
