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
    /// 买商品的动作Target为购买商品的Index
    /// </summary>
    public class OnPurchaseAction : GameAction
    {
        /// <summary>
        /// 为true用金币否则用理智
        /// </summary>
        public bool UseCoin {  get;}
        /// <summary>
        /// 通知商店完成事件
        /// </summary>
        public string State = null;

        public OnPurchaseAction(GameActionContext context, bool useCoin) : base(context)
        {
            UseCoin = useCoin;
        }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if(!(Context.Target is int index))
            {
                Debug.LogError("[OnPurchaseAction]传入Target不是int类的实例");
                IsValid = false;
                return;
            }

            Product product;
            if (index >= StoreEventManager.Instance.CurrentListedProducts.Length || index < 0)
            {
                Debug.LogError("[OnPurchaseAction]传入Target的值index为不合法值");
                IsValid = false;
                return;
            }
            else
            {
                product = StoreEventManager.Instance.CurrentListedProducts[index];
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
                GameActionContext context = new(this, ExplorerPlayerController.Instance.PlayerData, this);
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
                GameActionContext context = new(this, ExplorerPlayerController.Instance, this);
                CurrentSanityChangeAction sanityChangeAction = new(context, product.SanityPrice);
                await ActionManager.Instance.PerformChildActionAsync(sanityChangeAction, Depth, cancellationToken);
                if (sanityChangeAction.IsValid == false)
                {
                    IsValid = false;
                    return;
                }
            }

            switch (product.ItemType)
            {
                case ItemType.Souvenir:
                    GameActionContext context = new(this, product.ProductId, this);
                    OnOwnSouvenirAction onOwnSouvenirAction = new(context);
                    //默认成功
                    await ActionManager.Instance.PerformChildActionAsync(onOwnSouvenirAction, Depth, cancellationToken);
                    State = "PurchaseSouvenir" + product.ProductId;
                    break;
                case ItemType.Card:
                    ///待改动
                    throw new System.NotImplementedException();
            }
        }
    }
}
