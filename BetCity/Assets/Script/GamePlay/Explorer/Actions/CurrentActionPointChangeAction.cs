using BetCity.Core.ActionSystem;
using System.Collections;
using BetCity.Core.Tools;

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BetCity.GamePlay.Explorer
{
    public class CurrentActionPointChangeAction : GameAction
    {
        public override async UniTask Perform()
        {
            if (Context.Target is  ExplorerPlayerController playerController)
            {
                playerController.ChangeCurrentActionPoint(ChangeAmount, this);
            }
            await UniTask.CompletedTask;
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
