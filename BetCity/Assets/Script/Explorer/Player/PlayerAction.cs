
//using UnityEngine;
//using UnityEngine.UI; // 引用 UI 命名空间以访问 Image 组件


//public class PlayerAction : MonoBehaviour
//{
//    [Header("组件引用")]
//    [SerializeField] private DiceManager diceManager; // 投骰子管理器
//    [SerializeField] private SanityManager sanityManager; // 理智管理器
//    [SerializeField] private PlayerMovement playerMovement;



//    [Header("理智消耗设置")]
//    [SerializeField] private int dailySanityCost = 5; // 每次行动消耗的理智值


//    private int _actionPoints;  // 私有字段用于保存行动点数

//    public int ActionPoints
//    {
//        get { return _actionPoints; }  // 获取私有字段的值
//        set
//        {
//            if (_actionPoints != value)  // 只有在值改变时才更新
//            {
//                _actionPoints = value;
//                diceManager.UpdateDiceDisplay(_actionPoints);  // 每次更新行动点数时，更新骰子的显示
//            }
//        }
//    }




//    // 投掷骰子
//    public void RollDice()
//    {
//      //  Debug.Log($"RollDice: 检测 IsMoving = {playerMovement.IsMoving}");
//        // 🚫 玩家正在移动，禁止投掷
//        if (playerMovement != null && playerMovement.IsMoving)
//        {
//       //     Debug.Log("玩家正在移动中，不能投掷骰子！");
//            return;
//        }
//        // 🚫 骰子正在滚动中，禁止投掷
//        if (diceManager.DiceCounter.IsRolling())
//        {
//         //   Debug.Log("骰子正在滚动，无法被点击。");
//            return;
//        }

//        // 🚫 当前行动点数大于 0，不能再投
//        if (ActionPoints > 0)
//        {
//      //      Debug.Log("还有未使用的行动点，不能再次投掷。");
//            return;
//        }

//        // 🚫 理智不足，不能投掷
//        if (sanityManager.CurrentSanity < dailySanityCost)
//        {
//            Debug.LogWarning("理智值不足，无法投掷骰子。");
//            return;
//        }


//        sanityManager.DecreaseSanity(dailySanityCost);

//        ActionPoints = diceManager.RollDice(); 

//        Debug.Log($"获得行动点数: {ActionPoints}");

//        // 在骰子滚动结束后，解除锁定（你可以用事件做这事）
//        diceManager.DiceCounter.OnDiceRollFinished += OnRollFinished;
//    }

//    private void OnRollFinished(int result)
//    {
//        sanityManager.IsLocked = false;
//        diceManager.DiceCounter.OnDiceRollFinished -= OnRollFinished; // 移除监听
//    }




//    // 消耗行动点数
//    public void DecreaseActionPoints(int amount = 1)
//    {
//        ActionPoints -= amount;
//    }

//    // 设置行动点数
//    public void SetActionPoints(int amount)
//    {
//        ActionPoints = amount;
//    }



//}
