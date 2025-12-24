using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.Tools.Test
{
    public class TestAction : GameAction
    {
        public override async UniTask Perform()
        {
            Debug.Log("这是TestAction的perform流程");
            await UniTask.CompletedTask;
        }
        public TestAction(GameActionContext context) : base(context) { }
    }

    public class EndTurnAction : GameAction
    {
        public override async UniTask Perform()
        {
            Debug.Log("这是EndTurnAction的perform流程");
            await UniTask.CompletedTask;
        }
        public EndTurnAction(GameActionContext context) : base(context) { }
    }

    public class TestEffect : MonoBehaviour
    {
        private void Awake()
        {
            if (button != null)
            {
                // 绑定无参点击事件
                button.onClick.AddListener(Test);
            }
        }
        [SerializeField] private Button button;
        [SerializeField] private PassiveEffectConfig config;
        public void Test()
        {
            config.Activate();
            TestAction action1 = new(null);
            EndTurnAction action2 = new(null);
            ActionManager.Instance.Perform(action1);
            ActionManager.Instance.Perform(action2);
            ActionManager.Instance.Perform(action1);
            ActionManager.Instance.Perform(action2);
            ActionManager.Instance.Perform(action1);
            ActionManager.Instance.Perform(action2);
            ActionManager.Instance.Perform(action1);
            ActionManager.Instance.Perform(action2);
        }
    }

}