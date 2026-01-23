using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Store;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.Tools.Test
{
    public class TestEvent : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        private void Awake()
        {
            if (button != null)
            {
                // 绑定无参点击事件
                button.onClick.AddListener(Perform);
            }
        }

        public void Perform()
        {
            //StoreManager.Instance.OnEnterStoreNode(1);
            //Debug.Log(StoreManager.Instance.CurrentListedProducts.Count);
            //Debug.Log(StoreManager.Instance.CurrentListedProducts[0].ProductId);
        }
    }
}

