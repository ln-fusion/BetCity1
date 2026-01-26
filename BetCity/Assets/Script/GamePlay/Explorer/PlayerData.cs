using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.Data.Storage;
using BetCity.GamePlay.Explorer;
using BetCity.Data.ConfigModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BetCity.GamePlay.Souvenir;
namespace BetCity.GamePlay.Explorer
{
    public class PlayerData:IHasCoin
    {
        public int MaxSanity { get; private set; }
        public int CurrentSanity { get; private set; }
        public int MaxActionPoints { get; private set; }
        public int CurrentActionPoints { get; private set; }
        public int CurrentNodeNum { get; private set; }
        public int CurrentCoin { get; private set; }
        public int MapID { get; private set; }
        //接口，忽略
        public int Coin =>CurrentCoin;
        public int SouvenirMaxSlot { get; private set; }
        public int SouvenirCurrentSlot => SouvenirManager.Instance.CurrentSlots;
        public int[] Dice { get; private set; }
        public PlayerData()
        {
            MaxSanity = 20;
            CurrentSanity = 10;
            MaxActionPoints = 6;
            CurrentActionPoints = 0;
            CurrentNodeNum = 0;
            CurrentCoin = 10;
            MapID = 1;
            SouvenirMaxSlot = 0;
            Dice =new int[] { 1, 1, 1, 1, 2, 3 };
        }
        public void Load(PlayerDataDTO playerDataDTO)
        {
            MaxSanity = playerDataDTO.MaxSanity;
            CurrentSanity = playerDataDTO.CurrentSanity;
            MaxActionPoints = playerDataDTO.MaxActionPoints;
            CurrentActionPoints = playerDataDTO.CurrentActionPoints;
            CurrentNodeNum = playerDataDTO.CurrentNodeNum;
            CurrentCoin = playerDataDTO.Coin;
            MapID = playerDataDTO.MapID;
            SouvenirMaxSlot = playerDataDTO.SouvenirMaxSlot;
            Dice=playerDataDTO.Dice;
        }
        #region 接口
        /// <summary>
        /// 更改当前理智值接口
        /// </summary>
        public bool ChangeCurrentSanity(int Sanity, CurrentSanityChangeAction currentSanityChangeAction)
        {
            if (CurrentSanity >= MaxSanity)
            {
                //理智值已满
                CurrentSanity = MaxSanity;
            }
            else if (CurrentSanity > MaxSanity - Sanity)
            {
                CurrentSanity = MaxSanity;
            }
            else
            {
                CurrentSanity += Sanity;

            }
            return true;
        }
        /// <summary>
        /// 更改当前金币值接口
        /// </summary>
        public bool ChangeCoin(int coin, CoinChangeAction coinChangeAction)
        {
            CurrentCoin += coin;
            return true;
        }
        /// <summary>
        /// 更改当前AP点接口
        /// </summary>
        public bool ChangeCurrentActionPoint(int actionPoint, CurrentActionPointChangeAction currentActionPointChangeAction)
        {
            CurrentActionPoints += actionPoint;

            if (CurrentActionPoints >= MaxActionPoints)
            {
                CurrentActionPoints = MaxActionPoints;
            }
            else if (CurrentActionPoints < 0)
            {
                CurrentActionPoints = 0;
            }

            return true;
        }
        /// <summary>
        /// 更改当前金币值接口
        /// </summary>
        public bool ChangeCurrentNodeNum(int targetNum, CurrentNodeNumChangeAction currentNodeNumChangeAction)
        {
            CurrentNodeNum = targetNum;
            return true;
        }
        /// <summary>
        /// 更改最大纪念品槽接口
        /// </summary>
        public bool ChangeSouvenirMaxSlot(int maxNum, SouvenirMaxSlotChangeAction souvenirMaxSlotChangeAction)
        {
            SouvenirMaxSlot = maxNum;
            return true;
        }
        /// <summary>
        /// 更改骰子的面值
        /// </summary>
        public bool ChangeDiceNum(int num,int changeNum)
        {
            if (num < 0 || num > 6)
            {
                //报错越界
                Debug.LogWarning("[" + this + "]骰子编号越界，无法进行骰子升级");

                return false;
            }
            if (Dice[num] + changeNum > 9|| Dice[num] + changeNum<0)
            {
                //报错越界
                Debug.LogWarning("[" + this + "]骰子升级量报错越界，无法进行骰子升级");

                return false;
            }
            Dice[num] += changeNum;
            return true;
        }

        #endregion

    }
}
