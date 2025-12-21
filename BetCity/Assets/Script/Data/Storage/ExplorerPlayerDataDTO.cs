using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.Storage
{
    /// <summary>
    /// 地图系统中玩家的数值
    /// </summary>
    [Serializable]
    public class PlayerDTO
    {

        /// <summary>
        /// 关联原型ID
        /// </summary>
        //public int MaxSanity { get; private set; }
        //public int CurrentSanity { get; private set; }
        //public int MaxActionPoints { get; private set; }
        //public int CurrentActionPoints { get; private set; }
        //public int CurrentNodeNum { get; private set; }
        public int MaxSanity { get; set; }
        public int CurrentSanity { get; set; }
        public int MaxActionPoints { get; set; }
        public int CurrentActionPoints { get;set; }
        public int CurrentNodeNum { get; set; }
        public int Coin { get; set; }


        public PlayerDTO() { }

        [JsonConstructor]
        public PlayerDTO(int maxsanity, int currentsanity, int maxactionpoints, int currentactionpoints, int currentnodenum, int coin)
        {
            MaxSanity = maxsanity;
            CurrentSanity = currentsanity;
            MaxActionPoints = maxactionpoints;
            CurrentActionPoints = currentactionpoints;
            CurrentNodeNum = currentnodenum;
            Coin = coin;

        }
    }
}
