using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    public class CurrentNodeNumChangeAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is ExplorerPlayerController playerController)
            {
                playerController.PlayerData.ChangeCurrentNodeNum(TargetNum, this);
                playerController.RenewScreen();

            }
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 变化量（正数增加，负数减少）
        /// </summary>
        public int TargetNum { get; }

        public CurrentNodeNumChangeAction(GameActionContext context, int targetNum)
            : base(context)
        {
            TargetNum = targetNum;
        }
    }
}
