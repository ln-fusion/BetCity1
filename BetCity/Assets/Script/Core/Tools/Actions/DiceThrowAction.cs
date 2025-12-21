using BetCity.Core.ActionSystem;
using BetCity.Explorer;
using System.Collections;
namespace BetCity.Core.Tools
{
    /// <summary>
    /// 玩家投掷骰子动作
    /// </summary>
    public class DiceThrowAction : GameAction
    {
        /// <summary>
        /// 是否执行完修改，如果修改不合法一直为false
        /// </summary>
        public bool HasChanged { get; protected set; } = false;
        public DiceThrowAction(GameActionContext context)
            : base(context)
        {
        }
        public override IEnumerator Perform()
        {
            if (Context.Target is ExplorerDiceController diceController)
            {
                yield return diceController.DiceThrow();
            }
            yield break;
        }
    }
}
