using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    public class ExplorerResourceController : MonoSingleton<ExplorerResourceController>
    {
        /// <summary>
        /// 可序列化字典，暂时用不到
        /// </summary>
        [field: SerializeField]
        public SerializableDictionary<int, MapData> Map { get; private set; }
        /// <summary>
        /// 存入地图的预制体
        /// </summary>
        [field: SerializeField]
        public List<GameObject> MapPrefab { get; private set; }
        /// <summary>
        /// 存入玩家gameobject的预制体
        /// </summary>
        [field: SerializeField]
        public GameObject Player { get; private set; }
        protected override void Awake()
        {
            base.Awake();
        }
        /// <summary>
        /// 获取对应地图的接口
        /// </summary>
        public GameObject GetMap(int i)
        {
            return (MapPrefab[i]);
        }
    }
}
