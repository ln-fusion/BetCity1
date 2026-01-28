using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerDice : MonoBehaviour
{
    //骰子信息
    public int[] Num => ExplorerPlayerController.Instance.PlayerData.Dice;

    /// <summary>
    /// 扔骰子
    /// </summary>
    /// <returns>投到的值</returns>
    public async UniTask<int> Throw(CancellationToken cancellationToken)
    {
        gameObject.SetActive(true);
        return 0;
    }
}
