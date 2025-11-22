// DiceManager.cs
using UnityEngine;

public class DiceManager : MonoBehaviour
{
      [SerializeField] private DiceCounter diceCounter; // 引用 DiceCounter
       public DiceCounter DiceCounter => diceCounter; // 添加这个属性





    private void Awake()
    {
        if (diceCounter == null)
        {
            Debug.LogError("DiceCounter 未赋值给 DiceManager！请在 Inspector 中拖拽赋值。");
        }
    }

    public int RollDice()
    {
     //   if (IsRolling) return 0;  // 如果骰子正在滚动，直接返回，防止重复点击
        if (diceCounter == null)
        {
            Debug.LogError("DiceCounter 未赋值！无法投掷可视化骰子。");
            return 0;
        }
   //     IsRolling = true;  // 设置为正在滚动
        int result = Random.Range(1, 7); // 标准6面骰
        Debug.Log($"骰子结果: {result}");

        // 调用 DiceCounter 来显示可视化骰子动画
        diceCounter.SetResultIndexAndAnimate(result);

        // 预留商人事件扩展点
        // if(result >= 6) EventManager.OnFullMoon?.Invoke();


        return result;
    }
    // 动画结束后调用此方法，设置 IsRolling 为 false，表示可以再次投掷骰子
  /*  public void EndRoll()
    {
        IsRolling = false;  // 动画结束，设置为不再滚动
    }*/

    // 更新骰子的显示
    public void UpdateDiceDisplay(int points)
    {
        diceCounter.DiceDisplay(points);
    }
}
