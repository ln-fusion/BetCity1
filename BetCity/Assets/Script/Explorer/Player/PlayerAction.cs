
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private DiceManager diceManager; // 投骰子管理器
    [SerializeField] private SanityManager sanityManager; // 理智管理器

    [Header("理智消耗设置")]
    [SerializeField] private int dailySanityCost = 5; // 每次行动消耗的理智值


    private int _actionPoints;  // 私有字段用于保存行动点数

    public int ActionPoints
    {
        get { return _actionPoints; }  // 获取私有字段的值
        set
        {
            if (_actionPoints != value)  // 只有在值改变时才更新
            {
                _actionPoints = value;
                diceManager.UpdateDiceDisplay(_actionPoints);  // 每次更新行动点数时，更新骰子的显示
            }
        }
    }


    // 投掷骰子
    public void RollDice()
    {
        if (sanityManager.CurrentSanity <= 0)
        {
            Debug.LogWarning("理智值不足，无法投掷骰子。");
            return;
        }

        sanityManager.DecreaseSanity(dailySanityCost);

        if (sanityManager.CurrentSanity <= 0)
        {
            Debug.Log("理智值已归零，无法投掷骰子。");
            return;
        }

        ActionPoints = diceManager.RollDice();
        Debug.Log($"获得行动点数: {ActionPoints}");
    }

    // 消耗行动点数
    public void DecreaseActionPoints(int amount = 1)
    {
        ActionPoints -= amount;
    }

    // 设置行动点数
    public void SetActionPoints(int amount)
    {
        ActionPoints = amount;
    }
}
