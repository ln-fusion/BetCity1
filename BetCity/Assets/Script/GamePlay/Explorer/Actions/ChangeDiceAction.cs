using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 骰子面值更改动作，source随意，target是更改的骰子面num，传入的changenum是更改的数值
    /// </summary>
    public class ChangeDiceAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is int i)
            {
                ExplorerPlayerController.Instance.PlayerData.ChangeDiceNum(i, ChangeNum);
                ExplorerDiceController.Instance.DisplayDice();
            }
            return UniTask.CompletedTask;
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
