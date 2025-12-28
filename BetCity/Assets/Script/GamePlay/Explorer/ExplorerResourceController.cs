using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.Explorer
{
    public class ExplorerResourceController : MonoSingleton<ExplorerResourceController>
    {
        [Header("可序列化的字典,但是好像用不到")]
        public SerializableDictionary<int, MapData> Map;
        [SerializeField]
        public List<GameObject> MapPrefab;
        public GameObject Player;
        protected override void Awake()
        {
            base.Awake();
        }
        public GameObject GetMap(int i)
        {
            return (MapPrefab[i]);
        }
    }
}
