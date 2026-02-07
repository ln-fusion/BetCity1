using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Souvenir
{
    /// <summary>
    /// 拥有纪念品动作，Source = 动作发起人，Target = 纪念品id
    /// </summary>
    public class OnOwnSouvenirAction : GameAction
    {
        public OnOwnSouvenirAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if(Context.Target is not int id)
            {
                Debug.LogError("[OnOwnSouvenirAction]Context.Target不是纪念品!");
                IsValid = false;
                return UniTask.CompletedTask;
            }
            if(!SouvenirManager.Instance.OwnSouvenirById(id, out Souvenir souvenir, out string errorMsg))
            {
                Debug.LogError(errorMsg);
                IsValid = false;
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }
    }

}
