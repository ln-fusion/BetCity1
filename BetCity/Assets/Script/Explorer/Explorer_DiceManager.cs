using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
namespace BetCity.Explorer {

    public class Explorer_DiceManager : MonoBehaviour
    {
        public Explorer_ScreenController screencontroller;
        public static Sprite[] diceimage = new Sprite[7];
        public Image image;
        [Header("资源初始化标志")]
        private static bool Initial = false;
        [Header("骰子参数")]
        public float dicetime;
        public int sanitycost;

        public void Awake()
        {
            if (!Initial)
            {
                Initial = true;
                diceimage[0] = Resources.Load<Sprite>("Image/Dice/DN0");
                diceimage[1] = Resources.Load<Sprite>("Image/Dice/DN1");
                diceimage[2] = Resources.Load<Sprite>("Image/Dice/DN2");
                diceimage[3] = Resources.Load<Sprite>("Image/Dice/DN3");
                diceimage[4] = Resources.Load<Sprite>("Image/Dice/DN4");
                diceimage[5] = Resources.Load<Sprite>("Image/Dice/DN5");
                diceimage[6] = Resources.Load<Sprite>("Image/Dice/DN6");
            }
        }
        public void ThrowDice()
        {
            if (Explorer_PlayerController.playerstatus == 0)
            {
                if (PlayerNature.currentSanity >= sanitycost)
                {
                    StartCoroutine(Dice());
                }
                else
                {
                    Explorer_ScreenController.CreateMessage("理智值不足");
                }
            }
            else
            {
                Explorer_ScreenController.CreateMessage("当前无法操作");
            }
        }
        public IEnumerator Dice()
        {
            Explorer_PlayerController.playerstatus = 2;
            PlayerNature.modifyCurrentSanity(-sanitycost);
            screencontroller.printPlayerNature();
            float currentdicetime = dicetime;
            int imagenum = 0;
            while (currentdicetime > 0)
            {
                imagenum = (imagenum + 1) % 7;
                image.sprite = diceimage[imagenum];
                currentdicetime -= 0.3f;
                yield return new WaitForSeconds(0.3f);
            }
            int randomInt = Random.Range(0, 100);
            randomInt = randomInt % 6;
            image.sprite = diceimage[randomInt];
            PlayerNature.modifyCurrentActionPoints(randomInt);
            Explorer_ScreenController.CreateMessage("AP点+" + randomInt);
            if (PlayerNature.currentActionPoints >= PlayerNature.maxActionPoints)
            {
                PlayerNature.modifyCurrentActionPoints(PlayerNature.maxActionPoints - PlayerNature.currentActionPoints);
            }
            screencontroller.printPlayerNature();
            yield return null;
            Explorer_PlayerController.playerstatus = 0;
        }
    }
}
