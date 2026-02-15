using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store
{
    /// <summary>
    /// 商店刷新动作
    /// </summary>
    public class OnStoreRefreshAction : GameAction
    {
        /// <summary>
        /// 为true用金币否则用理智
        /// </summary>
        public bool UseCoin { get; }
        /// <summary>
        /// 消耗的金币/理智（为正数）
        /// </summary>
        public int Price {  get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="useCoin">是否用金币</param>
        /// <param name="price">消耗量（为正）</param>
        public OnStoreRefreshAction(GameActionContext context, bool useCoin, int price) : base(context) 
        { 
            UseCoin = useCoin;
            Price = price;
        }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if(UseCoin)
            {
                CoinChangeAction coinChangeAction = new(new GameActionContext(this, Explorer.ExplorerPlayerController.Instance.PlayerData, this), -Price);
                await ActionManager.Instance.PerformChildActionAsync(coinChangeAction, this.Depth, cancellationToken);
                if (!coinChangeAction.IsValid)
                {
                    IsValid = false;
                    return;
                }
            }
            else
            {
                CurrentSanityChangeAction sanityChangeAction = new(new GameActionContext(this, this, this), -Price);
                await ActionManager.Instance.PerformChildActionAsync(sanityChangeAction, this.Depth, cancellationToken);
                if (sanityChangeAction.IsValid)
                {
                    IsValid = false;
                    return;
                }
            }
            StoreEventManager.Instance.RefreshProducts();
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
            if(discount >= 0)
            {
                Price = (int) (Price * discount);
            }
        }
    }

}