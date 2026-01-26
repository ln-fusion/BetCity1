using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace BetCity.GamePlay.Explorer {

    public class ExplorerDiceController : MonoSingleton<ExplorerDiceController> 
    {
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        private PlayerData PlayerData => PlayerController.PlayerData;
        private ExplorerScreenController ScreenController=>ExplorerScreenController.Instance;
        private ExplorerPlayerController PlayerController=>ExplorerPlayerController.Instance;

        /// <summary>
        /// 骰子
        /// </summary>
        public ExplorerDice Dice {  get; private set; }

        [Header("资源初始化标志")]
        private bool _initial = false;
        [Header("骰子参数")]
        /// <summary>
        /// 使用骰子的理智值消耗
        /// </summary>
        private const int SANITY_COST = 2;

        protected override void Awake()
        {
            base.Awake();
            if (!_initial)
            {
                _initial = true;
                GameObject dice = Instantiate(Resources.Load<GameObject>("Prefab/Dice"), Vector3.zero, Quaternion.Euler(Vector3.zero));
                dice.SetActive(false);
                Dice = dice.GetComponent<ExplorerDice>();
            }
            
        }
        
        /// <summary>
        /// 创建投掷骰子动作函数
        /// </summary>
        public void UseDiceThrow()
        {
            if (PlayerController.PlayerStatus != 0)
            {
                //当前无法操作
                Debug.LogWarning("["+this.name + "]当前无法操作，无法投掷骰子");
                return;
            }
            if (PlayerData.CurrentSanity < SANITY_COST)
            {
                //理智值不足
                Debug.LogWarning("[" + this.name + "]理智值不足，无法投掷骰子");

                return;
            }
            if (PlayerData.CurrentActionPoints != 0)
            {
                //AP点不为0
                Debug.LogWarning("[" + this.name + "]AP点不为0，无法投掷骰子");

                return;
            }
            GameActionContext context = new(this, this, null);
            var DiceAction = new DiceThrowAction(context);
            ActionManager.Instance.Perform(DiceAction);
        }
        /// <summary>
        /// 投掷骰子的实际逻辑
        /// </summary>
        public async UniTask DiceThrow(CancellationToken cancellationToken)
        {
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
        {
            GameActionContext context = new(this, num, null);
            var changeDice = new ChangeDiceAction(context,changeNum);
            ActionManager.Instance.Perform(changeDice);
        }
        /// <summary>
        /// 创建骰子升级/降级的事件
        /// </summary>
        public void Tmp(int i)
        {
            ChangeDice(i, 1);
        }
        /// <summary>
        /// 展示骰子的信息
        /// </summary>
        public void DisplayDice()
        {
            Debug.Log($"骰子信息");

            for (int j = 0; j < 6; j++)
            {
                Debug.Log($"第" + j + "面:" + Dice.Num[j]);
            }
        }

    }
}
