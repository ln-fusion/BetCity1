using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BetCity.GamePlay.Chest
{
    /// <summary>
    /// 创建一个宝箱事件,context里面的source不重要，target是List<IRoundConfig>
    /// </summary>
    public class EnterChestAction : GameAction
    {

        public EnterChestAction(GameActionContext context)
    : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is List<IRoundConfig> configs)
            {
                ChestEventManager.Instance.StartChest(configs);
            }
            return UniTask.CompletedTask;
        }
    }
}
