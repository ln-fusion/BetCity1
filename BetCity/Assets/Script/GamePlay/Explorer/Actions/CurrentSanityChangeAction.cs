using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    public class CurrentSanityChangeAction : GameAction
    {
        public override async UniTask Perform()
        {
            if (Context.Target is ExplorerPlayerController playerController)
            {
                playerController.ChangeCurrentSanity(ChangeAmount, this);
            }
            await UniTask.CompletedTask;
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
