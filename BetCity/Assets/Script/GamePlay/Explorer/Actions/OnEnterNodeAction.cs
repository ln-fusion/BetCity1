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
    public override async UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Target is Node a)
        {
            await ExplorerPlayerController.Instance.EnterNode(a);
            await UniTask.Yield();
        }
        return;
    }
}
