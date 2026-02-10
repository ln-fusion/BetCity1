using BetCity.Core.ActionSystem;
using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Store;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.City
{
    /// <summary>
    /// 城市事件管理器
    /// </summary>
    public class CityEventManager : BaseEventManager<CityEvent, CityEventManager>
    {
        private ActionManager actionManager => ActionManager.Instance;
        /// <summary>
        /// 城市事件
        /// </summary>
        public IReadOnlyDictionary<int, CityEvent> cityEvents => EventLoader.Instance.CityEvents;




        protected override void Awake()
        {
            base.Awake();

        }

        #region 接口
        /// <summary>
        /// OnEnterStoreNode触发该函数，将CurrentEventState设置为Start
        /// </summary>
        /// <param name="id">id</param>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            if(cityEvents.TryGetValue(id,out CityEvent cityevent))
            {
                base.EnterEvent(cancellationToken, id);
                CurrentEvent = cityevent;
            }
            else
            {
                Debug.LogError($"[CityEventManager]对应ID的地图不存在");
                CurrentEventState = "None";
                return UniTask.CompletedTask;
            }

            return UniTask.CompletedTask;
        }
        /// <summary>
        /// 创建交谈动作，将状态设置为"Purchase"+ "Card/Souvenir" + id
        /// </summary>
        /// <param name="index"></param>
        public void CreateDialogueAction(int index)
        {
            //OnPurchaseAction onPurchaseAction = new(new GameActionContext(this, index, null), SanityPurchaseIndexs.Contains(index));
            //ActionManager.Instance.Perform(onPurchaseAction, () => { if (onPurchaseAction.IsValid) CurrentEventState = onPurchaseAction.State; });
        }
        #endregion
    }
}
