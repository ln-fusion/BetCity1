using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Store;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.Tools.Test
{
    public class TestAct : GameAction
    {
        public TestAct(GameActionContext context) : base(context)
        {
        }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }

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
            ActionManager.SubscribeReaction<TestAct>(Test, ReactionTiming.POST);
        }

        public void Test(TestAct action)
        {
            Debug.Log("Test");
        }
        public void Perform()
        {
           
            ActionManager.Instance.Perform(new TestAct(null));
            //StoreManager.Instance.OnEnterStoreNode(1);
            //Debug.Log(StoreManager.Instance.CurrentListedProducts.Count);
            //Debug.Log(StoreManager.Instance.CurrentListedProducts[0].ProductId);
        }
    }
}

