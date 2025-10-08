using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public Image d4DiceImage;                // 改为UI Image组件
    public Sprite[] d4DiceFaces;             // 四面骰子四个面的图片
    public float d4RollDuration = 1.5f;      // 四面骰子滚动持续时间
    public float d4FaceChangeInterval = 0.1f; // 四面骰子面切换间隔

    [Header("UI大小控制")]
    [Range(50, 300)]
    public float diceSize = 100f; // UI 大小，单位像素

    private D4DiceState d4DiceState = D4DiceState.Idle;
    private int d4DiceResult;                // 四面骰子最终结果
    private Coroutine d4RollCoroutine;       // 四面骰子滚动协程

    // 四面骰子投掷完成事件
    public System.Action<int> OnD4DiceRollFinished;

    private void Awake()
    {
        // 初始化骰子状态
        if (d4DiceImage != null && d4DiceFaces != null && d4DiceFaces.Length > 0)
        {
            d4DiceImage.sprite = d4DiceFaces[0];
        }

        // 设置初始大小
        SetDiceSize(diceSize);
    }

    // 设置骰子UI大小
    public void SetDiceSize(float size)
    {
        diceSize = size;
        if (d4DiceImage != null)
        {
            RectTransform rectTransform = d4DiceImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(size, size);
            }
        }
    }

    // 投掷四面骰子
    public void RollD4Dice()
    {
        if (d4DiceState != D4DiceState.Idle) return;

        Debug.Log("开始投掷四面骰子");
        d4DiceState = D4DiceState.Rolling;

        if (d4DiceObject != null)
            d4DiceObject.SetActive(true);

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
                int randomFace = Random.Range(0, 4);
                if (d4DiceImage != null && d4DiceFaces.Length > randomFace && d4DiceFaces[randomFace] != null)
                {
                    d4DiceImage.sprite = d4DiceFaces[randomFace];
                }

                nextFaceChangeTime = Time.time + d4FaceChangeInterval;
            }

            yield return null;
        }

        // 确定最终结果 (1-4)
        d4DiceResult = Random.Range(1, 5);

        // 显示最终结果
        if (d4DiceImage != null && d4DiceFaces.Length >= d4DiceResult && d4DiceFaces[d4DiceResult - 1] != null)
        {
            d4DiceImage.sprite = d4DiceFaces[d4DiceResult - 1];
        }

        d4DiceState = D4DiceState.ShowResult;
        Debug.Log($"四面骰子投掷完成，点数: {d4DiceResult}");

        yield return new WaitForSeconds(1f);

        OnD4DiceRollFinished?.Invoke(d4DiceResult);

        d4DiceState = D4DiceState.Idle;
        d4RollCoroutine = null;
    }

    // 检查四面骰子是否正在滚动
    public bool IsD4Rolling()
    {
        return d4DiceState == D4DiceState.Rolling;
    }

    // 设置骰子可见性
    public void SetDiceActive(bool active)
    {
        if (d4DiceObject != null)
        {
            d4DiceObject.SetActive(active);
        }
    }
}