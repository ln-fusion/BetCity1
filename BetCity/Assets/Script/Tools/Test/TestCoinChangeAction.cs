using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Tools.Test 
{
    public class TestCoinChangeAction : MonoBehaviour
    {
        public void Awake()
        {
            GameActionContext context = new(this, EconomyManager.Instance, null);
            CoinChangeAction action1 = new CoinChangeAction(context, 10);
            ActionManager.Instance.Perform(action1);
            CoinChangeAction action2 = new CoinChangeAction(context, -100);
            ActionManager.Instance.Perform(action2);
            CoinChangeAction action3 = new CoinChangeAction(context, -5);
            ActionManager.Instance.Perform(action3, () => Debug.Log(EconomyManager.Instance.Coin));
            Debug.Log(EconomyManager.Instance.Coin);
        }
    }
}
