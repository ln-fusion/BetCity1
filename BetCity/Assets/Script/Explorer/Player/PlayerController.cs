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
            // 由于 Awake 中已经获取了正确的引用，这里的调用会作用于正确的游戏对象。
            playerMovement.InitAtNode(startNode);
            Debug.Log($"玩家已初始化到节点: {startNode.name}");
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
        // ... 前置的 if 判断保持不变 ...

        // 检查是否可以移动到目标节点
        if (playerMovement.CanMoveTo(targetNode, playerAction.ActionPoints))
        {
            // 如果可以移动，则启动移动协程
            Debug.Log("条件满足，开始移动...");
            StartCoroutine(playerMovement.MoveToNode(targetNode));

            // 移动后消耗行动点
            playerAction.DecreaseActionPoints();

            // --- 【在这里添加延迟】 ---

            // 1. 把要检查事件的节点存起来
            targetNodeForEvent = targetNode;

            // 2. 使用 Invoke 来延迟调用一个新的方法，比如延迟 0.7 秒
            Invoke("CheckEventAfterDelay", 0.7f);
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

    //切换到事件场景 (由外部调用，例如节点事件触发)
     public void LoadEventScene(int sceneIndex)
    {
         playerStateSaver.SaveState(); // 保存当前状态
       playerEventSystem.LoadEventScene(sceneIndex);
    }

    // 离开事件，返回探索场景 (由外部调用，例如事件结束按钮)
    public void EndEvent()
    {
        playerEventSystem.EndEvent();
        playerStateSaver.RestoreState(); // 恢复玩家状态
    }
}
