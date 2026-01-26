using BetCity.GamePlay.Explorer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerDice : MonoBehaviour
{
    //骰子信息
    public int[] Num => ExplorerPlayerController.Instance.PlayerData.Dice;
}
