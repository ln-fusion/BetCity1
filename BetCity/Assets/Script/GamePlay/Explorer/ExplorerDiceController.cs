using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using BetCity.Core.Tools;
using BetCity.Core.ActionSystem;
namespace BetCity.Explorer {

    public class ExplorerDiceController : MonoSingleton<ExplorerDiceController> 
    {
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        private data.PlayerData PlayerData;
        private ExplorerScreenController ScreenController;
        private ExplorerPlayerController playerController;
        /// <summary>
        /// 骰子图片
        /// </summary>
        public static Sprite[] DiceImage = new Sprite[7];
        /// <summary>
        /// 骰子当前的图片
        /// </summary>
        public Image DiceCurrentImage;
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
                DiceImage[0] = Resources.Load<Sprite>("Image/Dice/DN0");
                DiceImage[1] = Resources.Load<Sprite>("Image/Dice/DN1");
                DiceImage[2] = Resources.Load<Sprite>("Image/Dice/DN2");
                DiceImage[3] = Resources.Load<Sprite>("Image/Dice/DN3");
                DiceImage[4] = Resources.Load<Sprite>("Image/Dice/DN4");
                DiceImage[5] = Resources.Load<Sprite>("Image/Dice/DN5");
                DiceImage[6] = Resources.Load<Sprite>("Image/Dice/DN6");
            }
        }
        /// <summary>
        /// 获取场景中的实例
        /// </summary>
        void Start()
        {
            playerController=ExplorerPlayerController.Instance;
            ScreenController=ExplorerScreenController.Instance;
            PlayerData = data.PlayerData.Instance;
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
        public IEnumerator DiceThrow()
        {
            if (ExplorerPlayerController.PLAYER_STATUS != 0)
            {
                ExplorerScreenController.CreateMessage("当前无法操作");
                yield break;
            }
            if (PlayerData.CurrentSanity < SanityCost)
            {
                ExplorerScreenController.CreateMessage("理智值不足");
                yield break;
            }
            if (PlayerData.CurrentActionPoints != 0)
            {
                ExplorerScreenController.CreateMessage("AP点不为0");
                yield break;
            }
            ExplorerPlayerController.PLAYER_STATUS = 2;
            playerController.UseSanityChange(-1 * SanityCost);
            float currentdicetime = DiceTime;
            int imagenum = 0;
            while (currentdicetime > 0)
            {
                imagenum = (imagenum + 1) % 7;
                DiceCurrentImage.sprite = DiceImage[imagenum];
                currentdicetime -= 0.3f;
                yield return new WaitForSeconds(0.3f);
            }
            int randomInt = Random.Range(0, 100);
            randomInt = randomInt % 7;
            if (randomInt == 0)
            {
                randomInt = 1;
            }
            DiceCurrentImage.sprite = DiceImage[randomInt];
            playerController.UseActionPointChange(randomInt);
            ExplorerScreenController.CreateMessage("AP点+" + randomInt);
            ExplorerPlayerController.PLAYER_STATUS = 0;
            yield return null;
        }
        /// <summary>
        /// 在消耗AP点后更新骰子的UI显示
        /// </summary>
        public void APRefresh()
        {
            DiceCurrentImage.sprite = DiceImage[PlayerData.CurrentActionPoints];
        }
    }
}
