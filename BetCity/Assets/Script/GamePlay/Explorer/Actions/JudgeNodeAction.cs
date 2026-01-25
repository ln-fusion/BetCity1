using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Explorer;

public class JudgeNodeAction : GameAction
{
    public JudgeNodeAction(GameActionContext context) : base(context)
    {

    }
    public override async UniTask Perform(CancellationToken cancellationToken)
    {
        if (Context.Source is Node a&& Context.Target is Node b)
        {
            await ExplorerPlayerController.Instance.MoveJudge(a,b);

            //await ExplorerPlayerController.Instance.Move(a);
        }
        return;
    }


}
