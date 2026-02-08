using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using BetCity.Data.ConfigModels;

namespace BetCity.GamePlay.Plot
{
    /// <summary>
    /// 将对话作为一个游戏行为执行，执行时会触发结果
    /// </summary>
    public class  DialogueAction : GameAction
    {
        public DialogueData Dialogue { get; }

        public DialogueAction(GameActionContext context, DialogueData dialogue) : base(context)
        {
            this.Dialogue = dialogue;
        }

        public override UniTask Perform(CancellationToken cancellationToken)
        {
            if (Dialogue == null || Dialogue.Lines == null || Dialogue.Lines.Count == 0)
                return UniTask.CompletedTask;

            var first = Dialogue.Lines[0];
            try
            {
                first.Result?.Apply();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[DialogueAction] Perform error: {e}");
            }
            return UniTask.CompletedTask;
        }
    }
}
