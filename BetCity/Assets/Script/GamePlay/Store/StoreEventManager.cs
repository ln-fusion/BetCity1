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
    /// </summary>
    public class StoreEventManager : BaseEventManager<StoreEvent>
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
        public int UpgradeDiceCount { get; set; }
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
            List<int> selectedIndexs = RandomTool.GetWeightRandomIndexNoRepeat(legalProducts.Select(l => l.Weight).ToList(), CurrentEvent.Amount);
            CurrentListedProducts = selectedIndexs.Select(i => legalProducts[i]).ToArray();
            SanityPurchaseIndexs = RandomTool.GetWeightRandomIndexNoRepeat(Enumerable.Repeat(1, CurrentEvent.Amount).ToList(), CurrentEvent.SanityPurchaseAmount).ToArray();
        }

        #region 接口
        /// <summary>
        /// 进入商店节点动作触发该函数
        /// </summary>
        /// <param name="id">id</param>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            base.EnterEvent(cancellationToken, id);
            if (storeEvents.TryGetValue(id, out StoreEvent storeEvent))
            {
                CurrentEvent = storeEvent;
            }
            else
            {
                Debug.LogWarning($"[StoreManager]对应Id为{id}的商店事件不存在！");
                CurrentEventState = "None";
                return UniTask.CompletedTask;
            }
            LoadProducts();
            return UniTask.CompletedTask;
        }

        
        #endregion
    }
}
