using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    public class CurrentNodeNumChangeAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            ExplorerPlayerController.Instance.PlayerData.ChangeCurrentNodeNum(TargetNum, this);
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
