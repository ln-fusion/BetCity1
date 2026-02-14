using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using BetCity.Card;

namespace BetCity.Card
{
    /// <summary>
    /// 拥有卡牌动作，Source = 动作发起人，Target = 卡牌 id
    /// </summary>
    public class OnOwnCardAction : GameAction
    {
        public OnOwnCardAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is not int id)
            {
                Debug.LogError("[OnOwnCardAction]Context.Target不是卡牌!");
                IsValid = false;
                return UniTask.CompletedTask;
            }
            if (!CardManager.Instance.OwnCardById(id, out Card card, out string errorMsg))
            {
                Debug.LogError(errorMsg);
                IsValid = false;
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }
    }
}
