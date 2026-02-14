using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Chest
{
    /// <summary>
    /// 宝箱选择动作,context的source和target是默认的就行
    /// </summary>
    public class ChooseChestAction : GameAction
    {
        public ChooseChestAction(GameActionContext context)
            : base(context) { }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            await ChestEventManager.Instance.WaitChoose(cancellationToken);

            await UniTask.Delay(2000);
            ChestEventManager.Instance.NextChoose();
            return;
        }
    }
}