using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Explorer;
using static UnityEditor.Timeline.TimelinePlaybackControls;


    /// <summary>
    /// 进入结点动作,source随意，target是目标结点
    /// </summary>
    /// <param name="context"></param>
    public class OnExitNodeAction : GameAction
    {
        public OnExitNodeAction(GameActionContext context) : base(context) { }

        public override async UniTask Perform(CancellationToken cancellationToken)
        {
            if (Context.Target is not Node || Context.Source is not Node)
            {
                Debug.LogError("OnExitNodeAction:Context.Target/Source不是一个节点！");
                IsValid = false;
                return;
            }
            await ExplorerPlayerController.Instance.ExitNode((Node)Context.Target, cancellationToken);
            return;
        }
    }
