using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.data
{
    public class PlayerData : MonoSingleton<PlayerData>
    {
        public int MaxSanity;
        public int CurrentSanity;
        public int MaxActionPoints;
        public int CurrentActionPoints;
        public int CurrentNodeNum;
        public int Coin;
        public int MapID;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
