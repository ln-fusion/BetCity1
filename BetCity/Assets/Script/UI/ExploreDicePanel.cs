using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using BetCity.UI.Core; // 引用你的核心命名空间

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 骰子面板：继承自 UIBase，适配 UIManager 框架
    /// </summary>
    public class ExplorerDicePanel : UIBase
    {
        [Header("UI 引用")]
        public Text resultText;
        public Button confirmBtn;

        [Header("3D 实体引用")]
        // 注意：这里改为 public 且首字母大写，解决 Controller 的报错
        public ExplorerDice DiceEntity;

        /// <summary>
        /// 重写 Init：注册按钮点击事件（符合 UIBase 规范）
        /// </summary>
        public override void Init()
        {
            base.Init(); // 必须调用基类初始化

            if (confirmBtn != null)
            {
                // 点击确定按钮，通过 UIManager 隐藏自己
                confirmBtn.onClick.AddListener(() =>
                {
                    UIManager.Instance.HideUI(gameObject.name);
                });
            }
        }

        /// <summary>
        /// 重写显示前的钩子：重置状态
        /// </summary>
        protected override void OnShowBeforeAnimation()
        {
            base.OnShowBeforeAnimation();
            if (resultText != null) resultText.gameObject.SetActive(false);
            if (confirmBtn != null) confirmBtn.gameObject.SetActive(false);
        }

        /// <summary>
        /// 核心投掷业务逻辑
        /// </summary>
        public async UniTask<int> StartDiceThrow(CancellationToken token)
        {
            // 确保显示了“投掷中”文字
            if (resultText != null)
            {
                resultText.text = "正在投掷...";
                resultText.gameObject.SetActive(true);
            }

            // 1. 调用 3D 骰子的旋转动画
            int finalPoint = await DiceEntity.Throw(token);

            // 2. 动画结束，展示最终点数
            if (resultText != null)
            {
                resultText.text = $"获得行动力: {finalPoint}";
            }

            // 3. 显示确认按钮
            if (confirmBtn != null)
            {
                confirmBtn.gameObject.SetActive(true);
            }

            return finalPoint;
        }
    }
}