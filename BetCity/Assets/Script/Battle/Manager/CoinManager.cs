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
    Heads, // 正面
    Tails  // 反面
}

public class CoinManager : MonoBehaviour
{
    [Header("硬币设置")]
    public GameObject coinObject;          // 硬币UI对象
    public Image coinImage;                // 改为UI Image组件
    public Sprite headsSprite;
    public Sprite tailsSprite;
    public Sprite flippingSprite;
    public float flipDuration = 1.5f;
    public float flipChangeInterval = 0.1f;

    [Header("交互设置")]
    public Button coinButton;

    [Header("UI大小控制")]
    [Range(50, 300)]
    public float coinSize = 100f; // UI 大小，单位像素

    private CoinState coinState = CoinState.Idle;
    private CoinResult coinResult;
    private Coroutine flipCoroutine;

    // 硬币投掷完成事件
    public System.Action<CoinResult> OnCoinFlipFinished;

    private void Awake()
    {
        // 初始化硬币状态
        if (coinImage != null && headsSprite != null)
        {
            coinImage.sprite = headsSprite;
        }

        // 设置初始大小
        SetCoinSize(coinSize);
    }

    // 设置硬币UI大小
    public void SetCoinSize(float size)
    {
        coinSize = size;
        if (coinImage != null)
        {
            RectTransform rectTransform = coinImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(size, size);
            }
        }
    }

    // 投掷硬币
    public void FlipCoin()
    {
        if (coinState != CoinState.Idle) return;

        Debug.Log("开始投掷硬币");
        coinState = CoinState.Flipping;

        if (coinObject != null)
        {
            coinObject.SetActive(true);
        }

        // 禁用按钮交互（防止重复点击）
        SetInteractable(false);

        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        flipCoroutine = StartCoroutine(FlipCoinCoroutine());
    }

    // 点击硬币（玩家操作）
    public void OnCoinClicked()
    {
        FlipCoin();
    }

    // 硬币投掷动画协程
    private IEnumerator FlipCoinCoroutine()
    {
        float elapsedTime = 0f;
        float nextFaceChangeTime = 0f;

        // 使用翻转中的图片
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

    // 设置硬币交互性
    public void SetInteractable(bool interactable)
    {
        if (coinButton != null)
        {
            coinButton.interactable = interactable;
        }
    }

    // 设置硬币可见性
    public void SetCoinActive(bool active)
    {
        if (coinObject != null)
        {
            coinObject.SetActive(active);
        }
    }

    // 重置硬币状态
    public void ResetCoin()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        coinState = CoinState.Idle;
        if (coinImage != null && headsSprite != null)
        {
            coinImage.sprite = headsSprite;
        }
    }

    // 检查硬币是否正在翻转
    public bool IsFlipping()
    {
        return coinState == CoinState.Flipping;
    }

    public CoinResult GetCurrentResult()
    {
        return coinResult;
    }
}