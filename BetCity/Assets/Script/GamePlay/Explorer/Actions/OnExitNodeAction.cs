using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Explorer;


public class OnExitNodeAction : GameAction
{
    public OnExitNodeAction(GameActionContext context) : base(context)
    {

    }
    public override async UniTask Perform(CancellationToken cancellationToken)
    {
        if(Context.Source is Node a)
        {
            await ExplorerPlayerController.Instance .ExitNode(a);
        }
        

        await UniTask.Yield();
        return;
    }
}
