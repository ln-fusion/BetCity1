using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using BetCity.Data.ConfigModels;
using System.Threading;
using UnityEngine;

public class OnEnterNodeAction :GameAction
{

    public OnEnterNodeAction(GameActionContext context):base(context)
    {

    }
    public override UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Target is Node a)
        {
            ExplorerPlayerController.Instance.EnterNode(a);
        }
        return UniTask.CompletedTask;
    }
}
