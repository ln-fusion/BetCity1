using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class Playernature
{
    public static int maxSanity ;
    public static int currentSanity;
    public static int maxActionPoints;
    public static int currentActionPoints;
    public static int currentNodeNum;
}
public class Explorer_PlayerController : MonoBehaviour
{
    public GameObject player;
    private RectTransform playertransform;
    private static bool Initial = false;
    private Animator animator;
    public static int playerstatus = 0;
    //0空闲 1行走
    public Explorer_ScreenController screencontroller;
    //move相关
    public float movespeed;
    private void Awake()
    {
        if (!Initial)
        {
            Initial = true;
            playertransform = player.GetComponent<RectTransform>();
            animator = player.GetComponent<Animator>();
        }
    }
    void Start()
    {
        
    }
    public void ToNode(Node currentnode, Node targetnode)
    {
        if(playerstatus==0)
        {
            if (Playernature.currentActionPoints>0)
            {
                Playernature.currentActionPoints--;
                StartCoroutine(Move(currentnode, targetnode));
            }
            else
            {
                Explorer_ScreenController.CreateMessage("AP点不足");
                return;
            }

        }
    }
    public IEnumerator Move(Node currentnode,Node targetnode)
    {
        Playernature.currentNodeNum = targetnode.id;
        playerstatus = 1;
        animator.SetBool("move",true);

        Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) - new Vector2(currentnode.Xposition, currentnode.Yposition);
        Vector2 target = new Vector2(targetnode.Xposition, targetnode.Yposition) + new Vector2(-50, 50);
        Vector2 moveframe = movetarget.normalized;
        float distance = movetarget.magnitude;
        while (distance>10)
        {
            playertransform.anchoredPosition +=moveframe*movespeed*Time.deltaTime;
            distance = Vector2.Distance(playertransform.anchoredPosition, target);
            yield return null;
        }
        playertransform.anchoredPosition = target;
        animator.SetBool("move", false);
        screencontroller.printPlayerNature();
        yield return null;
        playerstatus = 0;
    }
    public void ToNodeInstant(Node targetnode)
    {
        Playernature.currentNodeNum = targetnode.id;
        playertransform.anchoredPosition = new Vector2(targetnode.Xposition - 50, targetnode.Yposition + 50);
    }
    public void addap()
    {
        if (Playernature.currentActionPoints<Playernature.maxActionPoints)
        {
            Playernature.currentActionPoints++;
            screencontroller.printPlayerNature();
        }
        else
        {
            Explorer_ScreenController.CreateMessage("AP点已满");
        }
    }
    //调试

    // 初始化
    //private void Start()
    //{

    //    // 添加理智归零事件监听
    //    //sanityManager.onSanityZero.AddListener(HandleSanityZero);
    //}

    //// 在对象被销毁时，移除事件监听，防止内存泄漏。
    //private void OnDestroy()
    //{
    //    if (sanityManager != null)
    //    {
    //        sanityManager.onSanityZero.RemoveListener(HandleSanityZero);
    //    }
    //}

    //// 处理理智归零的事件
    //private void HandleSanityZero()
    //{
    //    //Debug.Log("玩家理智归零，游戏结束。");
    //    //SceneManager.LoadScene("GameOverScene"); // 加载游戏结束场景
    //}

    //// 公共方法：投骰子按钮
    //public void RollDiceButton()
    //{


    //    if (playerEventSystem.IsTransitioningToEvent)
    //    {
    //        Debug.LogWarning("正在切换到事件中，禁止操作！");
    //        return;
    //    }

    //    if (playerEventSystem.IsInEvent)
    //    {
    //        Debug.LogWarning("当前正在事件中，无法投掷骰子。");
    //        return;
    //    }

    //    if (playerMovement != null && playerMovement.IsMoving)
    //    {
    //        Debug.Log("玩家正在移动，不能投掷骰子。");
    //        return;
    //    }

    //    if (diceManager != null && diceManager.IsRolling) // 使用中转属性更简洁
    //    {
    //        Debug.Log("骰子正在滚动，不能投掷。");
    //        return;
    //    }

    //    if (playerAction.ActionPoints > 0)
    //    {
    //        Debug.Log("当前还有剩余行动次数，不能再次投掷骰子！");
    //        return;
    //    }

    //    playerAction.RollDice();
    //}


    //// 公共方法：尝试移动到目标节点

    //public void TryMoveToNode(Node targetNode)
    //{
    //    Debug.Log("IsTransitioningToEvent 状态：" + playerEventSystem.IsTransitioningToEvent);

    //    if (playerEventSystem.IsTransitioningToEvent)
    //    {
    //        Debug.Log("正在切换到事件中，禁止操作！");
    //        return;
    //    }

    //    // 🚫 禁止在骰子滚动时移动
    //    if (diceManager.DiceCounter.IsRolling())
    //    {
    //    //    Debug.Log("骰子正在滚动，不能移动！");
    //        return;
    //    }
    //    // 检查是否可以移动到目标节点
    //    if (playerMovement.CanMoveTo(targetNode, playerAction.ActionPoints))
    //    {
    //        // 如果可以移动，则启动移动协程
    //        Debug.Log("可以移动到该节点");
    //        StartCoroutine(playerMovement.MoveToNode(targetNode));

    //        // 移动后消耗行动点
    //        playerAction.DecreaseActionPoints();

    //        // --- 【在这里添加延迟】 ---

    //        // 1. 把要检查事件的节点存起来
    //        targetNodeForEvent = targetNode;
    //        // ✅ 如果是事件节点，立刻锁定
    //        if (targetNode.nodeType == NodeType.RandomEvent || targetNode.nodeType == NodeType.FixedEvent || targetNode.nodeType == NodeType.Battle)
    //        {
    //            playerEventSystem.SetTransitioningToEvent(true);
    //        }

    //        // 2. 使用 Invoke 来延迟调用一个新的方法，比如延迟 0.7 秒
    //        Invoke("CheckEventAfterDelay", 5.5f);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("移动失败：CanMoveTo 方法返回 false。");
    //    }
    //}

    //// --- 【新增一个专门用于延迟调用的方法】 ---
    //private void CheckEventAfterDelay()
    //{
    //    Debug.Log("延迟结束，现在检查节点事件...");

    //    // ✅ 延迟结束后立即锁定状态
    //    playerEventSystem.SetTransitioningToEvent(true);


    //    // 3. 从我们存好的变量中取出节点，并检查事件
    //    playerEventSystem.CheckForNodeEvent(targetNodeForEvent);
    //}


    //// 【新增】一个清晰的公共方法，供 PlayerEventSystem 调用
    //public void SaveStateBeforeLoadingScene()
    //{
    //    // 1. 保存玩家状态 (行动点，位置)
    //    playerStateSaver.SaveState();

    //    // 2. 记录当前场景，以便之后可以返回
    //    SceneStateManager.Instance.RecordCurrentScene();

    //    Debug.Log("[PlayerController] 状态已保存，场景已记录。准备切换...");
    //}



    //// 离开事件，返回探索场景 (由外部调用，例如事件结束按钮)
    //public void EndEvent()
    //{
    //    playerEventSystem.EndEvent();

    //}
}
