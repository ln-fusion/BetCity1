using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using BetCity.Core.Tools;
using BetCity.Core.ActionSystem;
using Cysharp.Threading.Tasks;
using System;
namespace BetCity.GamePlay.Explorer {

    public class ExplorerDiceController : MonoSingleton<ExplorerDiceController> 
    {
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        private data.PlayerData PlayerData;
        private ExplorerScreenController ScreenController;
        private ExplorerPlayerController playerController;

        /// <summary>
        /// 骰子
        /// </summary>
        public ExplorerDice Dice;
        /// <summary>
        /// 骰子位置
        /// </summary>
        public Transform DiceTransform;

        [Header("资源初始化标志")]
        private static bool _initial = false;
        [Header("骰子参数")]
        /// <summary>
        /// 骰子动画播放时间
        /// </summary>
        public float DiceTime;
        /// <summary>
        /// 使用骰子的理智值消耗
        /// </summary>
        public int SanityCost;

        protected override void Awake()
        {
            base.Awake();
            if (!_initial)
            {
                _initial = true;
                GameObject dice = Instantiate(Resources.Load<GameObject>("Prefab/Dice"), Vector3.zero, Quaternion.Euler(Vector3.zero));
                dice.GetComponent<RectTransform>().SetParent(DiceTransform, false);
                Dice = dice.GetComponent<ExplorerDice>();
                Dice.Initial();
            }
            
        }
        /// <summary>
        /// 获取场景中的实例
        /// </summary>
        void Start()
        {
            playerController=ExplorerPlayerController.Instance;
            ScreenController=ExplorerScreenController.Instance;
            PlayerData = playerController.playerData;
        }
        /// <summary>
        /// 创建投掷骰子动作函数
        /// </summary>
        public void UseDiceThrow()
        {
            GameActionContext context = new(this, this, null);
            var DiceAction = new DiceThrowAction(context);
            ActionManager.Instance.Perform(DiceAction);
        }
        /// <summary>
        /// 投掷骰子的实际逻辑
        /// </summary>
        public async UniTask DiceThrow()
        {
            if (ExplorerPlayerController.PLAYER_STATUS != 0)
            {
                //当前无法操作
                return;
            }
            if (PlayerData.CurrentSanity < SanityCost)
            {
                //理智值不足
                return;
            }
            if (PlayerData.CurrentActionPoints != 0)
            {
                //AP点不为0
                return;
            }
            ExplorerPlayerController.PLAYER_STATUS = 2;
            playerController.UseSanityChange(-1 * SanityCost);
            //找到当前最大的骰子面数，动画使用
            int MaxDiceNum = 0;
            for(int i = 0; i < 6; i++)
            {
                if (MaxDiceNum < Dice.Num[i])
                {
                    MaxDiceNum=Dice.Num[i];
                }
            }

            float currentdicetime = DiceTime;
            int imagenum = 0;
            while (currentdicetime > 0)
            {
                imagenum = (imagenum + 1) % MaxDiceNum;
                Dice. DiceCurrentImage.sprite = Dice.DiceImage[imagenum];
                currentdicetime -= 0.3f;
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            }
            int randomInt = UnityEngine. Random.Range(0, 100);
            randomInt = randomInt % 6;
            Debug.Log("?:"+randomInt);
            randomInt=Dice.Num[randomInt];
            Dice.ChangeSprite(randomInt);
            playerController.UseActionPointChange(randomInt);
            ExplorerPlayerController.PLAYER_STATUS = 0;
            await UniTask.Yield();
        }
        /// <summary>
        /// 在消耗AP点后更新骰子的UI显示
        /// </summary>
        public void APRefresh()
        {
            if(playerController == null)
            {
                playerController = ExplorerPlayerController.Instance;
                PlayerData = playerController.playerData;
            }
            Dice. DiceCurrentImage.sprite =Dice. DiceImage[PlayerData.CurrentActionPoints];
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
