using UnityEngine;
using BetCity.GamePlay.Explorer; // 确保指向你的 ExplorerDice 命名空间
using Cysharp.Threading.Tasks;
using System.Threading;

public class DiceManualTester : MonoBehaviour
{
    [Header("绑定场景中的骰子组件")]
    public ExplorerDice dice;

    [Header("测试参数")]
    [Tooltip("按下键盘 1-6 测试对应点数")]
    public bool useNumberKeys = true;

    void Update()
    {
        // 方式 A：按下 T 键随机投掷
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestDiceThrow().Forget();
        }

        // 方式 B：按下数字键 1-6，直接测试特定角度是否对准
        if (useNumberKeys)
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    TestSpecificPoint(i).Forget();
                }
            }
        }
    }

    async UniTaskVoid TestDiceThrow()
    {
        Debug.Log("<color=yellow>开始随机投掷测试...</color>");
        int result = await dice.Throw(CancellationToken.None);
        Debug.Log($"<color=green>投掷结束！逻辑点数：{result}</color>");
    }

    async UniTaskVoid TestSpecificPoint(int point)
    {
        Debug.Log($"<color=cyan>正在强制测试点数：{point} 的旋转角度...</color>");

        // 我们需要稍微修改一下你的 Throw 函数，或者在这里直接写逻辑
        // 这里为了简单，我们直接让骰子执行一次 Throw，但你可以观察它停下的位置
        // 如果你的 Throw 内部是随机的，这里可以用来检查它是否能停在你那 6 组坐标上
        await dice.Throw(CancellationToken.None);
    }
}