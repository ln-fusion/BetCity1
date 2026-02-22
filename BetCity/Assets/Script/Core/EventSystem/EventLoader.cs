using BetCity.Core.CheckSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetCity.Core.EventSystem
{
    /// <summary>
    /// 事件加载器
    /// </summary>
    public class EventLoader : MonoSingleton<EventLoader>
    {
        private Dictionary<int, BaseEvent> events = new Dictionary<int, BaseEvent>();
        private Dictionary<int, StoreEvent> storeEvents = new Dictionary<int, StoreEvent>();
        private Dictionary<int, TaskEvent> taskEvents = new Dictionary<int, TaskEvent>();
        private Dictionary<int,CityEvent> cityEvents=new Dictionary<int, CityEvent>();
        private Dictionary<int,ChestEvent> chestEvents=new Dictionary<int, ChestEvent>();
        ///<summary>
        /// 事件
        /// </summary>
        public IReadOnlyDictionary<int, BaseEvent> Events => events;
        /// <summary>
        /// 商店事件
        /// </summary>
        public IReadOnlyDictionary<int, StoreEvent> StoreEvents => storeEvents;
        /// <summary>
        /// 任务板事件
        /// </summary>
        public IReadOnlyDictionary<int, TaskEvent> TaskEvents => taskEvents;
        /// <summary>
        /// 城市事件
        /// </summary>
        public IReadOnlyDictionary<int, CityEvent> CityEvents => cityEvents;
        /// <summary>
        /// 宝箱事件
        /// </summary>
        public IReadOnlyDictionary<int, ChestEvent> ChestEvents => chestEvents;
        /// <summary>
        /// 纪念品资源路径
        /// </summary>
        public const string EVENT_RESOURCES_PATH = "Event";

        protected override void Awake()
        {
            base.Awake();
            LoadAllEvents();
        }

        private void LoadAllEvents()
        {
            try
            {
                List<BaseEvent> baseEvents = Resources.LoadAll<BaseEvent>(EVENT_RESOURCES_PATH).ToList();
                if (baseEvents == null || baseEvents.Count == 0)
                {
                    Debug.LogWarning($"[EventManager] 未在Resources/{EVENT_RESOURCES_PATH}路径下找到任何Event资源");
                    return;
                }
                foreach(BaseEvent baseEvent in baseEvents)
                {
                    events[baseEvent.Id] = baseEvent;
                    switch (baseEvent.Type)
                    {
                        case TypeOfEvent.Store:
                            storeEvents[baseEvent.Id] = (StoreEvent)baseEvent;
                            break;
                        case TypeOfEvent.Task:
                            taskEvents[baseEvent.Id] = (TaskEvent)baseEvent;
                            break;
                        case TypeOfEvent.City:
                            cityEvents[baseEvent.Id] = (CityEvent)baseEvent;
                            break;
                        case TypeOfEvent.Chest:
                            chestEvents[baseEvent.Id] = (ChestEvent)baseEvent;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SouvenirDataManager] 加载数据失败：{e.Message}\n{e.StackTrace}");
            }
        }

        #region 接口
        /// <summary>
        /// 判定指定id事件条件是否满足
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>成功与否</returns>
        public bool CheckIfLegal(int id)
        {
            if (events.ContainsKey(id))
            {
                return ConditionChecker.Instance.Check(events[id].Conditions.Init());
            }
            else
            {
                Debug.LogWarning($"[EventManager]Id为{id}的事件不存在！");
                return false;
            }
        }
        #endregion
    }
}