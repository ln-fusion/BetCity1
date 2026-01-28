using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using BetCity.Data.ConfigModels;
using System.Threading;
using UnityEngine;

public class OnEnterNodeAction :GameAction
{

    public OnEnterNodeAction(GameActionContext context) : base(context) { }

    public override UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Target is not Node a)
        {
            Debug.LogError("OnEnterNodeAction:Context.Target不是一个节点！");
            IsValid = false;
            return UniTask.CompletedTask;
        }

        // 保证不是在同一个节点
        if (Context.Source != Context.Target)
        {
            ExplorerPlayerController.Instance.EnterNode(a);
        }
        return UniTask.CompletedTask;
    }
}
