using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    public class ChangeDiceAction : GameAction
    {
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is int i)
            {
                ExplorerPlayerController.Instance.playerData.ChangeDiceNum(i, ChangeNum);
                ExplorerDiceController.Instance.DisplayDice();
                //playerController.RenewScreen();
            }
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 变化量（正数增加，负数减少）
        /// </summary>
        public int ChangeNum { get; }

        public ChangeDiceAction(GameActionContext context, int changeNum)
            : base(context)
        {
            ChangeNum = changeNum;
        }
    }
}
