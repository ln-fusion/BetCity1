using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.UI.Core;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 探索模式骰子控制器（单例）
    /// 职责：
    /// 1. 逻辑决策：判断当前理智值、AP点、玩家状态是否允许投掷。
    /// 2. 行为分发：通过 GameAction 系统触发投掷动作，确保动作序列化。
    /// 3. 界面交互：驱动 UIManager 唤起 3D 骰子面板并获取异步结果。
    /// </summary>
    public class ExplorerDiceController : MonoSingleton<ExplorerDiceController>
    {
        #region 属性与字段
        [Header("配置参数")]
        [Tooltip("投掷骰子消耗的理智值")]
        private const int SANITY_COST = 2;

        [Tooltip("UI预制体名称（需位于Resources/UI目录下）")]
        private const string DICE_PANEL_NAME = "ExplorerDicePanel";

        // 快捷访问引用
        private PlayerData PlayerData => PlayerController.PlayerData;
        private ExplorerPlayerController PlayerController => ExplorerPlayerController.Instance;
        #endregion

        #region 生命周期
        protected override void Awake()
        {
<<<<<<< Updated upstream
            GameActionContext context = new(this, this, null);
            var DiceAction = new DiceThrowAction(context);
            ActionManager.Instance.Perform(DiceAction);
        }
        public bool JudgeDiceThrow()
        {
            Debug.LogWarning("JudgeDiceThrow");
            if (PlayerController.PlayerStatus != 0)
            {
                //当前无法操作
                Debug.LogWarning("[" + this.name + "]当前无法操作，无法投掷骰子");
                return false ;
=======
            base.Awake();
            // 注意：此处不再在Awake中实例化Dice，改为随UI面板按需加载，节省初始内存
        }
        #endregion

        #region 外部调用接口 (Input/Button 调用)
        /// <summary>
        /// 尝试触发投掷逻辑（由 UI 按钮或快捷键直接调用）
        /// 执行前置条件检查，通过后进入 Action 系统
        /// </summary>
        public void UseDiceThrow()
        {
            // 1. 状态检查：必须处于 Idle 状态
            if (PlayerController.PlayerStatus != 0)
            {
                Debug.LogWarning($"【{name}】当前状态繁忙({PlayerController.PlayerStatus})，无法投掷");
                return;
>>>>>>> Stashed changes
            }

            // 2. 消耗检查：理智值是否足够
            if (PlayerData.CurrentSanity < SANITY_COST)
            {
<<<<<<< Updated upstream
                //理智值不足
                Debug.LogWarning("[" + this.name + "]理智值不足，无法投掷骰子");

                return false;
=======
                Debug.LogWarning($"【{name}】理智值不足({PlayerData.CurrentSanity}/{SANITY_COST})");
                // 此处可以扩展：弹出提示浮窗“理智值不足”
                return;
>>>>>>> Stashed changes
            }

            // 3. 规则检查：AP 点为 0 时才允许重新投掷（根据项目规则调整）
            if (PlayerData.CurrentActionPoints != 0)
            {
<<<<<<< Updated upstream
                //AP点不为0
                Debug.LogWarning("[" + this.name + "]AP点不为0，无法投掷骰子");

                return false;
            }
            return true ;
        }
        /// <summary>
        /// 投掷骰子的实际逻辑
        /// </summary>
        public async UniTask DiceThrow(CancellationToken cancellationToken)
        {
            Debug.LogWarning("DiceThrow");
            PlayerController.PlayerStatus = 2;
            PlayerController.UseSanityChange(-1 * SANITY_COST);

            int randomInt = await Dice.Throw(cancellationToken);

            PlayerController.UseActionPointChange(randomInt);
            PlayerController.PlayerStatus = 0;
            return;
        }
        /// <summary>
        /// 创建骰子升级/降级的事件
        /// </summary>
        public void ChangeDice(int num,int changeNum)
=======
                Debug.LogWarning($"【{name}】当前尚有剩余AP，无需投掷");
                return;
            }

            // 4. 进入行为系统：封装成 Action 派发，保证逻辑链条完整
            GameActionContext context = new(this, this, null);
            var diceAction = new DiceThrowAction(context);
            ActionManager.Instance.Perform(diceAction);
        }
        #endregion

        #region 核心业务逻辑 (由 DiceThrowAction 驱动)
        /// <summary>
        /// 投掷骰子的异步实际执行逻辑
        /// </summary>
        /// <param name="cancellationToken">取消令牌，用于处理切换场景等导致的异步终止</param>
        public async UniTask DiceThrow(CancellationToken cancellationToken)
>>>>>>> Stashed changes
        {
            // 1. 状态锁定：防止执行期间再次触发
            PlayerController.PlayerStatus = 2; // 设置为 Busy/Anim 状态

            // 2. 预扣除消耗
            PlayerController.UseSanityChange(-1 * SANITY_COST);

            // 3. UI 展示：通过管理器打开 3D 骰子面板
            // 注意：使用 UIType.Popup 会自动将其压入栈并设置正确的 SortingOrder
            UIManager.Instance.ShowUI(DICE_PANEL_NAME, UIType.Popup);

            // 4. 获取面板组件引用
            var dicePanel = UIManager.Instance.GetUIInstance(DICE_PANEL_NAME) as ExplorerDicePanel;

            if (dicePanel != null)
            {
                try
                {
                    // 核心逻辑：等待 UI 面板内部完成 3D 物理模拟并返回最终点数
                    // 该方法会持续挂起直到玩家点击“确定”或动画结束
                    int finalResult = await dicePanel.StartDiceThrow(cancellationToken);

                    // 5. 应用结果：更新玩家 AP
                    PlayerController.UseActionPointChange(finalResult);
                    Debug.Log($"【{name}】投掷完成。点数：{finalResult}，消耗理智：{SANITY_COST}");
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log($"【{name}】投掷动作已被取消");
                }
            }
            else
            {
                Debug.LogError($"【{name}】未能找到 UI 实例：{DICE_PANEL_NAME}，请检查预制体配置");
            }

            // 6. 状态恢复：允许玩家进行下一次操作
            PlayerController.PlayerStatus = 0; // 回到 Idle
        }
        #endregion

        #region 调试与扩展
        /// <summary>
        /// 升级/降级骰子（保留原功能，供其他系统调用）
        /// </summary>
        public void ChangeDice(int faceIndex, int changeValue)
        {
            GameActionContext context = new(this, faceIndex, null);
            var changeDice = new ChangeDiceAction(context, changeValue);
            ActionManager.Instance.Perform(changeDice);
        }

        /// <summary>
        /// 调试用：控制台打印当前骰子各面数值
        /// </summary>
        public void DisplayDice()
        {
            // 由于骰子现在存在于面板内，需要从面板获取引用
            var dicePanel = UIManager.Instance.GetUIInstance(DICE_PANEL_NAME) as ExplorerDicePanel;
            if (dicePanel != null && dicePanel.DiceEntity != null)
            {
                Debug.Log("--- 当前骰子配置 ---");
                for (int i = 0; i < 6; i++)
                {
                    Debug.Log($"面 [{i}]: {dicePanel.DiceEntity.Num[i]}");
                }
            }
        }
        #endregion
    }
}