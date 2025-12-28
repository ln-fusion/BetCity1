using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.Tools.Test
{
    public class TestAction : GameAction
    {
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            ActionManager actionManager = ActionManager.Instance;
            for(int i = 0; i < 100; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                await actionManager.WaitIfPaused(cancellationToken);
                await UniTask.Delay(1000);
                Debug.Log(i);
            }
            Debug.Log("这是TestAction的perform流程");
            return;
        }
        public TestAction(GameActionContext context) : base(context) { }
    }

    public class EndTurnAction : GameAction
    {
        public override UniTask Perform(CancellationToken cancellationToken)
        {
            Debug.Log("这是EndTurnAction的perform流程");
            return UniTask.CompletedTask;
        }
        public EndTurnAction(GameActionContext context) : base(context) { }
    }

    public class TestEffect : MonoBehaviour
    {
        private void Awake()
        {
            if (button1 != null)
            {
                // 绑定无参点击事件
                button1.onClick.AddListener(Pause);
            }
            if (button2 != null)
            {
                // 绑定无参点击事件
                button2.onClick.AddListener(Resume);
            }
            TestAction action = new TestAction(null);
            ActionManager.Instance.Perform(action);
        }
        [SerializeField] private Button button1;
        [SerializeField] private Button button2;
        [SerializeField] private EffectConfig config;
        public void Pause()
        {
            ActionManager.Instance.PauseAllActions();
        }

        public void Resume()
        {
            ActionManager.Instance.ResumeAllActions();
        }
    }

}