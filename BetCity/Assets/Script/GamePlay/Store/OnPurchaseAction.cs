using BetCity.Card;
using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Explorer;
using BetCity.GamePlay.Souvenir;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 买商品的动作，Target为Product
    /// </summary>
    public class OnPurchaseAction : GameAction
    {
        /// <summary>
        /// 为true用金币否则用理智
        /// </summary>
        public bool UseCoin {  get;}

        public OnPurchaseAction(GameActionContext context, bool useCoin) : base(context)
        {
            UseCoin = useCoin;
        }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if(!(Context.Target is Product product))
            {
                Debug.LogError("[OnPurchaseAction]传入Target不是Product类的实例");
                IsValid = false;
                return;
            }

            SouvenirData souvenirData = null;
            CardData cardData = null;
            switch (product.ItemType)
            {
                case ItemType.Souvenir:
                    souvenirData = SouvenirDataManager.Instance.GetDataById(product.ProductId);
                    break;
                case ItemType.Card:
                    cardData = CardDataManager.Instance.GetDataById(product.ProductId);
                    break;
            }
            if(souvenirData == null && cardData == null)
            {
                Debug.LogError("[OnPurchaseAction]传入ProductId存在问题!");
                IsValid = false;
                return;
            }

            if (UseCoin)
            {
                int price = souvenirData == null ? cardData.Price : souvenirData.Price;
                GameActionContext context = new(null, ExplorerPlayerController.Instance.PlayerData, null);
                CoinChangeAction coinChangeAction = new(context, price);
                await ActionManager.Instance.PerformChildActionAsync(coinChangeAction, Depth, cancellationToken);
                if (coinChangeAction.IsValid == false)
                {
                    //扣金币动作报错了，此处无需报错
                    IsValid = false;
                    return;
                }
            }
            else
            {
                GameActionContext context = new(null, ExplorerPlayerController.Instance, null);
                CurrentSanityChangeAction sanityChangeAction = new(context, product.SanityPrice);
                await ActionManager.Instance.PerformChildActionAsync(sanityChangeAction, Depth, cancellationToken);
                if (sanityChangeAction.IsValid == false)
                {
                    IsValid = false;
                    return;
                }
            }
        }
    }
}
