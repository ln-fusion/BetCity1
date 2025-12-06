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

public class CoinManager : MonoBehaviour
{
    [Header("硬币设置")]
    public GameObject coinObject;
    public Image coinImage;
    public Sprite headsSprite;
    public Sprite tailsSprite;
    public Sprite flippingSprite;
    public float flipDuration = 1.5f;
    public float flipChangeInterval = 0.1f;

    [Header("交互设置")]
    public Button coinButton;

    [Header("UI大小控制")]
    [Range(50, 300)]
    public float coinSize = 100f;

    private CoinState coinState = CoinState.Idle;
    private CoinResult coinResult;
    private Coroutine flipCoroutine;

    public System.Action<CoinResult> OnCoinFlipFinished;

    private void Awake()
    {
        if (coinImage != null && headsSprite != null)
        {
            coinImage.sprite = headsSprite;
        }

        // 设置初始大小
        SetCoinSize(coinSize);

        if (coinButton != null)
        {
            coinButton.onClick.AddListener(OnCoinClicked);
        }
    }

    // 设置硬币大小
    public void SetCoinSize(float scale)
    {
        coinSize = scale;
        if (coinImage != null)
        {
            RectTransform rectTransform = coinImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(scale, scale);
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

        // 禁用按钮交互
        SetInteractable(false);

        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        flipCoroutine = StartCoroutine(FlipCoinCoroutine());
    }

    public void OnCoinClicked()
    {
        FlipCoin();
    }

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

        coinResult = Random.Range(0, 2) == 0 ? CoinResult.Heads : CoinResult.Tails;

        if (coinImage != null)
        {
            coinImage.sprite = (coinResult == CoinResult.Heads) ? headsSprite : tailsSprite;
        }

        coinState = CoinState.ShowResult;
        Debug.Log($"硬币投掷完成，结果: {coinResult}");

        yield return new WaitForSeconds(1f);

        OnCoinFlipFinished?.Invoke(coinResult);

        coinState = CoinState.Idle;
        flipCoroutine = null;

        // 重新启用按钮交互（由外部阶段控制决定是否真正启用）
        SetInteractable(true);
    }

    // 设置硬币交互性
    public void SetInteractable(bool interactable)
    {
        if (coinButton != null)
        {
            // 只有在空闲状态或外部强制启用时才真正启用
            bool shouldBeInteractable = interactable && coinState == CoinState.Idle;
            coinButton.interactable = shouldBeInteractable;
        }
    }

    public void SetCoinActive(bool active)
    {
        if (coinObject != null)
        {
            coinObject.SetActive(active);
        }
    }

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

        // 确保按钮状态正确
        SetInteractable(true);
    }

    public bool IsFlipping()
    {
        return coinState == CoinState.Flipping;
    }

    public CoinResult GetCurrentResult()
    {
        return coinResult;
    }
}