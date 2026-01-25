using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.Storage
{
    /// <summary>
    /// 玩家基础信息
    /// </summary>
    [Serializable]
    public class PlayerDataDTO
    {
        public int MaxSanity { get;}
        public int CurrentSanity { get; }
        public int MaxActionPoints { get;}
        public int CurrentActionPoints { get; }
        public int CurrentNodeNum { get;}
        public int Coin { get; }
        public int MapID { get;}
        public int SouvenirMaxSlot { get; }
        public int[] Dice { get;}


        public PlayerDataDTO() { }

        [JsonConstructor]
        public PlayerDataDTO(int maxsanity, int currentsanity, int maxactionpoints, int currentactionpoints, int currentnodenum, int coin,int mapid, int souvenirMaxSlot, int[] dice)
        {
            MaxSanity = maxsanity;
            CurrentSanity = currentsanity;
            MaxActionPoints = maxactionpoints;
            CurrentActionPoints = currentactionpoints;
            CurrentNodeNum = currentnodenum;
            Coin = coin;
            MapID = mapid;
            SouvenirMaxSlot = souvenirMaxSlot;
            Dice = dice;
        }
    }
}
