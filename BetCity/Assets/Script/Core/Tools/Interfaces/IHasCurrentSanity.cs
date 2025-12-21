using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.Core.Tools
{
    public interface IHasCurrentSanity
    {
        /// <summary>
        /// 理智
        /// </summary>
        int CurrentSanity { get; }
        /// <summary>
        /// 变换理智
        /// </summary>
        bool ChangeCurrentSanity(int amount, CurrentSanityChangeAction caller);
    }
}