using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.City
{
    /// <summary>
    /// 创建城市NPC交流动作
    /// </summary>
    public class CityChatAction : GameAction
    {

        public CityChatAction(GameActionContext context, bool useCoin) : base(context)
        {

        }
        public override async UniTask Perform(CancellationToken cancellationToken)
        {
        }
    }
}
