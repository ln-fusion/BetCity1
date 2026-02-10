using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 城市事件
    /// </summary>
    [CreateAssetMenu(fileName = "Event", menuName = "Event/CityEvent")]
    public class CityEvent : BaseEvent
    {
        /// <summary>
        /// 城市事件对应的商店事件
        /// </summary>
        [field: SerializeField] public StoreEvent StoreEvent {  get; private set; }
    }
}
