using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using BetCity.Data.ConfigModels;
using System.Threading;
using UnityEngine;

public class OnEnterNodeAction :GameAction
{

    public OnEnterNodeAction(GameActionContext context) : base(context) { }

    public override async UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Target is not Node targetnode)
        {
            Debug.LogError("OnEnterNodeAction:Context.Target不是一个节点！");
            IsValid = false;
            return;
        }


        await ActionManager.Instance.PerformChildActionAsync(new CurrentActionPointChangeAction(new(this, this, null), -1), Depth, cancellationToken);


        await ActionManager.Instance.PerformChildActionAsync(new CurrentNodeNumChangeAction(new(this, this, null), targetnode.Id.Id), Depth, cancellationToken);

        await ExplorerPlayerController.Instance.EnterNode(targetnode, cancellationToken);

        return;
    }
}
