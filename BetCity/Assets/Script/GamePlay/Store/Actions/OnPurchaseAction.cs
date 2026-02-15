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
    /// 买商品的动作Target为购买商品Product
    /// </summary>
    public class OnPurchaseAction : GameAction
    {
        /// <summary>
        /// 为true用金币否则用理智
        /// </summary>
        public bool UseCoin { get;}
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; private set; }
        /// <summary>
        /// 通知商店完成事件
        /// </summary>
        public string State { get; private set; } = null;
        /// <summary>
        /// 当前商品
        /// </summary>
        public Product Product { get; }

        public OnPurchaseAction(GameActionContext context, bool useCoin, int price) : base(context)
        {
            UseCoin = useCoin;
            Price = price;
            if (Context.Target is not Product product)
            {
                IsValid = false;
                Debug.LogError("[OnPurchaseAction]传入Context.Target不是product类！");
                return;
            }
            Product = product;
        }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if (UseCoin)
            {
                GameActionContext context = new(this, ExplorerPlayerController.Instance.PlayerData, this);
                CoinChangeAction coinChangeAction = new(context, -Price);
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
                CurrentSanityChangeAction sanityChangeAction = new(context, -Price);
                await ActionManager.Instance.PerformChildActionAsync(sanityChangeAction, Depth, cancellationToken);
                if (sanityChangeAction.IsValid == false)
                {
                    IsValid = false;
                    return;
                }
            }

            switch (Product.ItemType)
            {
                case ItemType.Souvenir:
                    GameActionContext context = new(this, Product.ProductId, this);
                    OnOwnSouvenirAction onOwnSouvenirAction = new(context);
                    //默认成功
                    await ActionManager.Instance.PerformChildActionAsync(onOwnSouvenirAction, Depth, cancellationToken);
                    State = "PurchaseSouvenir" + Product.ProductId;
                    break;
                case ItemType.Card:
                    ///待改动
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// 修改消耗量，正数为增加
        /// </summary>
        public void ChangeAmount(int amount)
        {
            Price += amount;
        }

        /// <summary>
        /// 按比打折 <0的折扣视为无效
        /// </summary>
        public void DiscountAmount(float discount)
        {
            if (discount >= 0)
            {
                Price = (int)(Price * discount);
            }
        }
    }
}
