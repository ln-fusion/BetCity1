using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using BetCity.Card;

namespace BetCity.Card
{
    /// <summary>
    /// 失去卡牌动作，Source = 动作发起人，Target = 卡牌 id
    /// </summary>
    public class OnLoseCardAction : GameAction
    {
        public OnLoseCardAction(GameActionContext context) : base(context) { }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is not int id)
            {
                Debug.LogError("[OnLoseCardAction]Context.Target不是卡牌!");
                IsValid = false;
                return UniTask.CompletedTask;
            }
            if (!CardManager.Instance.LoseCardById(id, out Card card, out string errorMsg))
            {
                Debug.LogError(errorMsg);
                IsValid = false;
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }
    }
}
