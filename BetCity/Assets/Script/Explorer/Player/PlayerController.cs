using UnityEngine;
using UnityEngine.SceneManagement;

// [RequireComponent] 特性确保了所有必需的组件都存在于这个游戏对象上。
// 如果您尝试添加 PlayerController 到一个没有这些组件的对象上，Unity会自动为您添加它们。
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAction))]
[RequireComponent(typeof(PlayerEventSystem))]
[RequireComponent(typeof(PlayerStateSaver))]
public class PlayerController : MonoBehaviour
{
    [Header("外部场景引用")]
    [Tooltip("对场景中理智管理器的引用")]
    [SerializeField] private SanityManager sanityManager;
    [Tooltip("玩家在游戏开始时所在的初始节点")]
    [SerializeField] private Node startNode;
    [SerializeField] private DiceManager diceManager;

    // --- 内部组件引用 ---
    // 这些变量将由 Awake() 自动填充，无需在 Inspector 中手动拖拽。
    private PlayerMovement playerMovement;
    private PlayerAction playerAction;
    private PlayerEventSystem playerEventSystem;
    private PlayerStateSaver playerStateSaver;

    private Node targetNodeForEvent;

    // Awake() 在所有 Start() 方法之前执行，是获取组件引用的最佳位置。
    private void Awake()
    {
        // 自动获取挂载在同一个游戏对象上的其他组件。
        playerMovement = GetComponent<PlayerMovement>();
        playerAction = GetComponent<PlayerAction>();
        playerEventSystem = GetComponent<PlayerEventSystem>();
        playerStateSaver = GetComponent<PlayerStateSaver>();
    }

    // 初始化
    private void Start()
    {
        // 检查外部引用是否已在 Inspector 中设置
        if (sanityManager == null)
        {
            Debug.LogError("错误：SanityManager 引用未在 Inspector 中设置！", this.gameObject);
            return;
        }

        if (startNode != null)
        {
            // 调用 RestoreState，它会智能判断是恢复数据还是进行初始设置
            playerStateSaver.RestoreState(startNode);
        }
        else
        {
            Debug.LogError("错误：起始节点 (Start Node) 未在 Inspector 中设置！", this.gameObject);
        }

        // 添加理智归零事件监听
        sanityManager.onSanityZero.AddListener(HandleSanityZero);
    }

    // 在对象被销毁时，移除事件监听，防止内存泄漏。
    private void OnDestroy()
    {
        if (sanityManager != null)
        {
            sanityManager.onSanityZero.RemoveListener(HandleSanityZero);
        }
    }

    // 处理理智归零的事件
    private void HandleSanityZero()
    {
        Debug.Log("玩家理智归零，游戏结束。");
        SceneManager.LoadScene("GameOverScene"); // 加载游戏结束场景
    }

    // 公共方法：投骰子按钮
    public void RollDiceButton()
    {
        if (playerEventSystem.IsInEvent)
        {
            Debug.LogWarning("当前正在事件中，无法投掷骰子。");
            return;
        }

        playerAction.RollDice();
    }

    // 公共方法：尝试移动到目标节点

    public void TryMoveToNode(Node targetNode)
    {

        // 🚫 禁止在骰子滚动时移动
        if (diceManager.DiceCounter.IsRolling())
        {
            Debug.Log("骰子正在滚动，不能移动！");
            return;
        }
        // 检查是否可以移动到目标节点
        if (playerMovement.CanMoveTo(targetNode, playerAction.ActionPoints))
        {
            // 如果可以移动，则启动移动协程
            Debug.Log("可以移动到该节点");
            StartCoroutine(playerMovement.MoveToNode(targetNode));

            // 移动后消耗行动点
            playerAction.DecreaseActionPoints();

            // --- 【在这里添加延迟】 ---

            // 1. 把要检查事件的节点存起来
            targetNodeForEvent = targetNode;

            // 2. 使用 Invoke 来延迟调用一个新的方法，比如延迟 0.7 秒
            Invoke("CheckEventAfterDelay", 2.5f);
        }
        else
        {
            Debug.LogWarning("移动失败：CanMoveTo 方法返回 false。");
        }
    }

    // --- 【新增一个专门用于延迟调用的方法】 ---
    private void CheckEventAfterDelay()
    {
        Debug.Log("延迟结束，现在检查节点事件...");
        // 3. 从我们存好的变量中取出节点，并检查事件
        playerEventSystem.CheckForNodeEvent(targetNodeForEvent);
    }


    // 【新增】一个清晰的公共方法，供 PlayerEventSystem 调用
    public void SaveStateBeforeLoadingScene()
    {
        // 1. 保存玩家状态 (行动点，位置)
        playerStateSaver.SaveState();

        // 2. 记录当前场景，以便之后可以返回
        SceneStateManager.Instance.RecordCurrentScene();

        Debug.Log("[PlayerController] 状态已保存，场景已记录。准备切换...");
    }



    // 离开事件，返回探索场景 (由外部调用，例如事件结束按钮)
    public void EndEvent()
    {
        playerEventSystem.EndEvent();

    }
}
