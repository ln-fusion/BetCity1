using BetCity.GamePlay.Explorer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerDice : MonoBehaviour
{
    /// <summary>
    /// 骰子当前的图片
    /// </summary>
    public Image DiceCurrentImage;
    //骰子信息
    public int[] Num => ExplorerPlayerController.Instance.PlayerData.Dice;
    /// <summary>
    /// 骰子图片
    /// </summary>
    public Sprite[] DiceImage;
    private void Start()
    {

    }
    /// <summary>
    /// 初始化
    /// </summary>
    public void Initial()
    {
        DiceCurrentImage.sprite = DiceImage[0];
    }
    /// <summary>
    /// 更改骰子图片
    /// </summary>
    public void ChangeSprite(int i)
    {
        DiceCurrentImage.sprite = DiceImage[i];
    }
}
