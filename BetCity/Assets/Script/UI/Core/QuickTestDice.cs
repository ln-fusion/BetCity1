using UnityEngine;
using BetCity.GamePlay.Explorer;
using Cysharp.Threading.Tasks;
using System.Threading;

public class QuickTestDice : MonoBehaviour
{
    // 直接在 Inspector 里把刚才拖进场景的面板拖给这个变量
    public ExplorerDicePanel testPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunTest().Forget();
        }
    }

    async UniTaskVoid RunTest()
    {
        Debug.Log("测试开始：骰子滚动中...");
        // 直接绕过 UIManager，调用面板的投掷逻辑
        int result = await testPanel.StartDiceThrow(CancellationToken.None);
        Debug.Log($"测试结束！结果点数为: {result}");
    }
}