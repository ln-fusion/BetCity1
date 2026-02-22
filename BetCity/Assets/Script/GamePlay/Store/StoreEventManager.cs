using BetCity.Card;
using BetCity.Core.ActionSystem;
using BetCity.Core.CheckSystem;
using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.Data.Storage;
using BetCity.GamePlay.Souvenir;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Store 
{
    /// <summary>
    /// 商店事件管理器
    /// CurrentEventState有None, Start, Purchase
    /// </summary>
    public class StoreEventManager : BaseEventManager<StoreEvent, StoreEventManager>
    {
        //商店系统中不会刷新已拥有纪念品，此条不用写在condition里
        private IReadOnlyDictionary<int, Souvenir.Souvenir> ownedSouvenirs => SouvenirManager.Instance.OwnedSouvenirs;
        private ConditionChecker conditionChecker => ConditionChecker.Instance;
        private ProgressManager progressManager => ProgressManager.Instance;

        [Header("骰子升级次数-价格字典")] 
        public SerializableDictionary<int, int> UpgradeDicePriceDict;
        /// <summary>
        /// 商店升级骰子次数
        /// </summary>
        public int UpgradeDiceCount { get; private set; }
        /// <summary>
        /// 商店事件
        /// </summary>
        public IReadOnlyDictionary<int, StoreEvent> storeEvents => EventLoader.Instance.StoreEvents;
        /// <summary>
        /// 当前上架商品
        /// </summary>
        public Product[] CurrentListedProducts { get; private set; }
        /// <summary>
        /// 用理智购买商品的索引
        /// </summary>
        public int[] SanityPurchaseIndexs { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if(progressManager.GetKVData<int>("UpgradeDiceCount", out int value))
            {
                UpgradeDiceCount = value;
            }
            else
            {
                UpgradeDiceCount = 0;
                progressManager.SetKVData("UpgradeDiceCount", 0);
            }
        }

        private List<Product> CheckCondition()
        {
            List<Product> legalProducts = new List<Product>();
            foreach(Product product in CurrentEvent.Products)
            {
                switch (product.ItemType)
                {
                    case ItemType.Souvenir:
                        if (ownedSouvenirs.ContainsKey(product.ProductId))
                        {
                            continue;
                        }
                        else if (conditionChecker.Check(product.Conditions.Init()))
                        {
                            legalProducts.Add(product);
                            continue;
                        }
                        else continue;
                    //商店暂时不存卡牌
                    case ItemType.Card:
                        continue;
                }
            }
            return legalProducts;
        }

        //加载商品
        private void LoadProducts()
        {
            List<Product> legalProducts = CheckCondition();
            List<int> selectedCardIndexs = RandomTool.GetWeightRandomIndexNoRepeat(legalProducts.Where(p => p.ItemType == ItemType.Card)
                .Select(l => l.Weight).ToList(), CurrentEvent.CardAmount);
            List<int> selectedSouvenirIndexs = RandomTool.GetWeightRandomIndexNoRepeat(legalProducts.Where(p => p.ItemType == ItemType.Souvenir)
                .Select(l => l.Weight).ToList(), CurrentEvent.SouvenirAmount);
            List<Product> selectedCardProduct = selectedCardIndexs.Select(i => legalProducts[i]).ToList();
            List<Product> selectedSouvenirProduct = selectedSouvenirIndexs.Select(i => legalProducts[i]).ToList();
            CurrentListedProducts = selectedCardProduct.Concat(selectedSouvenirProduct).ToArray();
            SanityPurchaseIndexs = RandomTool.GetWeightRandomIndexNoRepeat(Enumerable.Repeat(1, CurrentListedProducts.Length).ToList(), CurrentEvent.SanityPurchaseAmount).ToArray();
        }

        // 加载商品原始价格，-1即为出现错误
        private int LoadOriginalPrice(int index)
        {
            if (index >= StoreEventManager.Instance.CurrentListedProducts.Length || index < 0)
            {
                Debug.LogError("[StoreEventManager]传入Target的值index为不合法值");
                return -1;
            }
            Product product = StoreEventManager.Instance.CurrentListedProducts[index];
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
            if (souvenirData == null && cardData == null)
            {
                Debug.LogError("[StoreEventManager]传入ProductId存在问题!");
                return -1;
            }

            if (SanityPurchaseIndexs.Contains(index))
            {
                return product.SanityPrice;
            }
            else
            {
                return souvenirData == null ? cardData.Price : souvenirData.Price;
            }
        }

        #region 接口
        /// <summary>
        /// OnEnterStoreNode触发该函数，将CurrentEventState设置为Start
        /// </summary>
        /// <param name="id">id</param>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            if (storeEvents.TryGetValue(id, out StoreEvent storeEvent))
            {
                base.EnterEvent(cancellationToken, id);
                CurrentEvent = storeEvent;
            }
            else
            {
                Debug.LogError($"[StoreEventManager]对应Id为{id}的商店事件不存在！");
                CurrentEventState = "None";
                return UniTask.CompletedTask;
            }
            LoadProducts();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 创建购买商品动作，将状态设置为"Purchase"+ "Card/Souvenir" + id
        /// </summary>
        /// <param name="index">第几个商品</param>
        public void CreateOnPurchaseAction(int index)
        {
            int price = LoadOriginalPrice(index);
            if (price == -1)
            {
                return;
            }
            Product product = CurrentListedProducts[index];

            OnPurchaseAction onPurchaseAction = new(new GameActionContext(this, product, null), SanityPurchaseIndexs.Contains(index), price);
            ActionManager.Instance.Perform(onPurchaseAction, () => { if (onPurchaseAction.IsValid) CurrentEventState = onPurchaseAction.State; });
        }

        /// <summary>
        /// 加载商品价格，-1即为出现错误
        /// </summary>
        /// <param name="index">第几个商品</param>
        public int LoadPrice(int index)
        {
            int price = LoadOriginalPrice(index);
            if (price == -1) return price;
            OnPurchaseAction onPurchaseAction = new(new GameActionContext(this, index, null), SanityPurchaseIndexs.Contains(index), price);
            ActionManager.Instance.ExecuteOnlyPreSub(onPurchaseAction);
            return onPurchaseAction.Price;
        }

        /// <summary>
        /// 【仅供OnStoreRefreshAction使用】刷新商品
        /// </summary>
        public void RefreshProducts()
        {
            LoadProducts();
        }
        #endregion
    }
}
