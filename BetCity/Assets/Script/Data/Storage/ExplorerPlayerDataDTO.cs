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
    public class Explorer_PlayerDTO
    {

        /// <summary>
        /// 关联原型ID
        /// </summary>
        public static int maxSanity { get; private set; }
        public static int currentSanity { get; private set; }
        public static int maxActionPoints { get; private set; }
        public static int currentActionPoints { get; private set; }
        public static int currentNodeNum { get; private set; }

        public Explorer_PlayerDTO() { }

        [JsonConstructor]
        public Explorer_PlayerDTO(int maxsanity, int currentsanity, int maxactionpoints, int currentactionpoints, int currentnodenum)
        {
            maxSanity = maxsanity;
            currentSanity = currentsanity;
            maxActionPoints = maxactionpoints;
            currentActionPoints = currentactionpoints;
            currentNodeNum = currentnodenum;
        }
    }
}
