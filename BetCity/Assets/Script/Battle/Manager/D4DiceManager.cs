using System.Collections;
using UnityEngine;

public enum D4DiceState
{
    Idle,
    Rolling,
    ShowResult
}

public class D4DiceManager : MonoBehaviour
{
    [Header("四面骰子设置")]
    public GameObject d4DiceObject;          // 四面骰子UI对象
    public SpriteRenderer d4DiceRenderer;    // 四面骰子渲染器
    public Sprite[] d4DiceFaces;             // 四面骰子四个面的图片
    public float d4RollDuration = 1.5f;      // 四面骰子滚动持续时间
    public float d4FaceChangeInterval = 0.1f; // 四面骰子面切换间隔

    private D4DiceState d4DiceState = D4DiceState.Idle;
    private int d4DiceResult;                // 四面骰子最终结果
    private Coroutine d4RollCoroutine;       // 四面骰子滚动协程

    // 四面骰子投掷完成事件
    public System.Action<int> OnD4DiceRollFinished;

    private void Awake()
    {
        // 初始隐藏四面骰子
        if (d4DiceObject != null)
            d4DiceObject.SetActive(false);
    }

    // 投掷四面骰子
    public void RollD4Dice()
    {
        if (d4DiceState != D4DiceState.Idle) return;

        Debug.Log("开始投掷四面骰子");
        d4DiceState = D4DiceState.Rolling;

        if (d4DiceObject != null)
            d4DiceObject.SetActive(true);

        // 开始投掷协程
        if (d4RollCoroutine != null)
            StopCoroutine(d4RollCoroutine);

        d4RollCoroutine = StartCoroutine(RollD4DiceCoroutine());
    }

    // 四面骰子投掷动画协程
    private IEnumerator RollD4DiceCoroutine()
    {
        float elapsedTime = 0f;
        float nextFaceChangeTime = 0f;

        while (elapsedTime < d4RollDuration)
        {
            elapsedTime += Time.deltaTime;

            // 定期切换骰子面
            if (Time.time >= nextFaceChangeTime)
            {
                // 显示随机面（不包括最终结果）
                int randomFace = Random.Range(0, 4);
                if (d4DiceRenderer != null && d4DiceFaces.Length > randomFace && d4DiceFaces[randomFace] != null)
                {
                    d4DiceRenderer.sprite = d4DiceFaces[randomFace];
                }

                nextFaceChangeTime = Time.time + d4FaceChangeInterval;
            }

            yield return null;
        }

        // 确定最终结果 (1-4)
        d4DiceResult = Random.Range(1, 5);

        // 显示最终结果
        if (d4DiceRenderer != null && d4DiceFaces.Length >= d4DiceResult && d4DiceFaces[d4DiceResult - 1] != null)
        {
            d4DiceRenderer.sprite = d4DiceFaces[d4DiceResult - 1];
        }

        d4DiceState = D4DiceState.ShowResult;
        Debug.Log($"四面骰子投掷完成，点数: {d4DiceResult}");

        // 等待1秒后通知结果
        yield return new WaitForSeconds(1f);

        // 通知外部骰子滚动结束
        OnD4DiceRollFinished?.Invoke(d4DiceResult);

        d4DiceState = D4DiceState.Idle;
        d4RollCoroutine = null;
    }

    // 检查四面骰子是否正在滚动
    public bool IsD4Rolling()
    {
        return d4DiceState == D4DiceState.Rolling;
    }

    // 隐藏四面骰子
    public void HideD4Dice()
    {
        if (d4DiceObject != null)
            d4DiceObject.SetActive(false);
    }
}