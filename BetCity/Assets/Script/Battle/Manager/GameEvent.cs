using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CoinState
{
    Idle,
    Flipping,
    ShowResult
}

public enum CoinResult
{
    Heads, 
    Tails  
}

public class BattleCoinManager : MonoBehaviour
{
    [Header("硬币设置")]
    public GameObject coinObject;          // 硬币UI对象
    public Image coinImage;                // 硬币Image组件
    public Sprite headsSprite;             // 正面图片
    public Sprite tailsSprite;             // 反面图片
    public Sprite flippingSprite;          
    public float flipDuration = 1.5f;      // 硬币翻转持续时间
    public float flipChangeInterval = 0.1f; // 硬币面切换间隔

    private CoinState coinState = CoinState.Idle;
    private CoinResult coinResult;         // 硬币最终结果
    private Coroutine flipCoroutine;       // 硬币翻转协程

    // 硬币投掷完成事件
    public System.Action<CoinResult> OnCoinFlipFinished;

    private void Awake()
    {
        // 确保硬币初始状态正确
        if (coinImage != null && headsSprite != null)
        {
            coinImage.sprite = headsSprite;
        }
    }

    // 投掷硬币
    public void FlipCoin()
    {
        if (coinState != CoinState.Idle) return;

        Debug.Log("开始投掷硬币");
        coinState = CoinState.Flipping;

        // 开始投掷协程
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        flipCoroutine = StartCoroutine(FlipCoinCoroutine());
    }

    // 硬币投掷动画协程
    private IEnumerator FlipCoinCoroutine()
    {
        float elapsedTime = 0f;
        float nextFaceChangeTime = 0f;

        if (flippingSprite != null && coinImage != null)
        {
            coinImage.sprite = flippingSprite;
        }

        while (elapsedTime < flipDuration)
        {
            elapsedTime += Time.deltaTime;

            // 定期切换硬币面
            if (Time.time >= nextFaceChangeTime)
            {
                // 随机显示正面或反面
                bool showHeads = Random.Range(0, 2) == 0;
                if (coinImage != null)
                {
                    coinImage.sprite = showHeads ? headsSprite : tailsSprite;
                }

                nextFaceChangeTime = Time.time + flipChangeInterval;
            }

            yield return null;
        }

        // 确定最终结果
        coinResult = Random.Range(0, 2) == 0 ? CoinResult.Heads : CoinResult.Tails;

        // 显示最终结果
        if (coinImage != null)
        {
            coinImage.sprite = (coinResult == CoinResult.Heads) ? headsSprite : tailsSprite;
        }

        coinState = CoinState.ShowResult;
        Debug.Log($"硬币投掷完成，结果: {coinResult}");

        // 等待1秒后通知结果
        yield return new WaitForSeconds(1f);

        // 通知外部硬币投掷结束
        OnCoinFlipFinished?.Invoke(coinResult);

        coinState = CoinState.Idle;
        flipCoroutine = null;
    }

    // 检查硬币是否正在翻转
    public bool IsFlipping()
    {
        return coinState == CoinState.Flipping;
    }

    // 设置硬币交互性
    public void SetInteractable(bool interactable)
    {
        // 获取按钮组件
        Button coinButton = GetComponent<Button>();
        if (coinButton != null)
        {
            coinButton.interactable = interactable;
        }

    }
}