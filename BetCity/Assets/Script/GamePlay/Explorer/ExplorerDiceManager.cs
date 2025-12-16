using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using BetCity.Core.Tools;
namespace BetCity.Explorer {

    public class ExplorerDiceManager : MonoSingleton<ExplorerDiceManager> 
    {
        public data.PlayerData PlayerData;

        public ExplorerScreenController ScreenController;
        public static Sprite[] DiceImage = new Sprite[7];
        public Image DiceCurrentImage;
        [Header("资源初始化标志")]
        private static bool _initial = false;
        [Header("骰子参数")]
        public float DiceTime;
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
        public void ThrowDice()
        {
            if (ExplorerPlayerController.PLAYER_STATUS == 0)
            {
                if (PlayerData.CurrentSanity>=SanityCost)
                {
                    if (PlayerData.CurrentActionPoints== 0)
                    {
                        StartCoroutine(Dice());
                    }
                    else
                    {
                        ExplorerScreenController.CreateMessage("AP点不为0");
                    }
                }
                else
                {
                    ExplorerScreenController.CreateMessage("理智值不足");
                }
            }
            else
            {
                ExplorerScreenController.CreateMessage("当前无法操作");
            }
        }
        public IEnumerator Dice()
        {
            ExplorerPlayerController.PLAYER_STATUS = 2;
            PlayerData.CurrentSanity-=SanityCost;
            ScreenController.printPlayerNature();
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
            PlayerData.CurrentActionPoints+=randomInt;
            ExplorerScreenController.CreateMessage("AP点+" + randomInt);
            if (PlayerData.CurrentActionPoints >= PlayerData.MaxActionPoints)
            {
                PlayerData.CurrentActionPoints+=PlayerData.MaxActionPoints - PlayerData.CurrentActionPoints;
            }
            ScreenController.printPlayerNature();
            yield return null;
            ExplorerPlayerController.PLAYER_STATUS = 0;
        }
        public void APMinus()
        {
            DiceCurrentImage.sprite = DiceImage[PlayerData.CurrentActionPoints];
        }
    }
}
