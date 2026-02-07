using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Souvenir
{
    /// <summary>
    /// 失去纪念品动作，Source = 动作发起人，Target = 纪念品id
    /// </summary>
    public class OnLoseSouvenirAction : GameAction
    {
        public OnLoseSouvenirAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is not int id)
            {
                Debug.LogError("[OnLoseSouvenirAction]Context.Target不是纪念品!");
                IsValid = false;
                return UniTask.CompletedTask;
            }
            if (!SouvenirManager.Instance.LoseSouvenirById(id, out Souvenir souvenir, out string errorMsg))
            {
                Debug.LogError(errorMsg);
                IsValid = false;
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }
    }

}
