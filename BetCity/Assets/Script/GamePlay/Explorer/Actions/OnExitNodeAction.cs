using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Explorer;
using static UnityEditor.Timeline.TimelinePlaybackControls;


public class OnExitNodeAction : GameAction
{
    public OnExitNodeAction(GameActionContext context) : base(context)
    {

    }
    public override UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Target is not Node || Context.Source is not Node)
        {
            Debug.LogError("OnExitNodeAction:Context.Target/Source不是一个节点！");
            IsValid = false;
            return UniTask.CompletedTask;
        }
        ExplorerPlayerController.Instance.ExitNode((Node)Context.Target);
        //if (!await ExplorerPlayerController.Instance.ExitNode((Node)Context.Target))
        //{
        //    IsValid = false;
        //}
        if (Context.Source != Context.Target)
        {
            ExplorerPlayerController.Instance.ExitNode((Node)Context.Target);
        }
        return UniTask.CompletedTask;
    }
}
