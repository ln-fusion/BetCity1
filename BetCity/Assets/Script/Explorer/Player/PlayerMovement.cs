//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class PlayerMovement : MonoBehaviour
//{
//    [Header("移动设置")]
//    [SerializeField] private float moveSpeed = 0.2f; // 移动速度
//    [SerializeField] private float playerHeight = 0.5f; // 玩家高度

//    public Node CurrentNode { get; private set; } // 当前所在节点
//    public bool IsMoving { get; private set; } // 是否正在移动
//    //public RectTransform Player;



//    // --- 新增代码 ---
//    private Animator animator; // 1. 添加 Animator 引用


//    // Awake 在 Start 之前执行，适合获取组件
//    private void Awake()
//    {
//        // 2. 获取挂载在同一个游戏对象上的 Animator 组件
//        animator = GetComponent<Animator>();
//    }

//    // 初始化玩家位置
//    public void InitAtNode(Node node)
//    {
//        CurrentNode = node;
//        transform.position = node.transform.position + Vector3.up * playerHeight;//删除
//        //Player.anchoredPosition=new Vector2();
//        /*
//        // --- 核心修复代码 ---
//        // 检查 Animator 是否存在
//        if (animator != null)
//        {
//            // 1. 立即将 isMoving 参数设为 false
//            animator.SetBool("isMoving", false);

//            // 2. (可选但强烈推荐) 强制 Animator 立即播放站立动画
//            // "Player_Idle" 是你的站立动画剪辑的名字
//            // 0 表示在第一层 Layer
//            // 0f 表示从动画的开头播放
//            animator.Play("Player_Idle", 0, 0f);
//        }
//        // --- 修复代码结束 ---
//        */
//    }

//    public IEnumerator MoveToNode(Node targetNode)
//    {
//        IsMoving = true;
//        Debug.Log("MoveToNode: 开始移动 — 设置 IsMoving = true");

//        if (animator != null)
//            animator.SetBool("isMoving", true);

//        Vector3 startPos = transform.position;
//        Vector3 endPos = targetNode.transform.position + Vector3.up * playerHeight;

//        float elapsed = 0f;
//        float duration = moveSpeed;  // ❗ 再次将 moveSpeed 当作“移动持续时间”

//        while (elapsed < duration)
//        {
//            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        transform.position = endPos;
//        CurrentNode = targetNode;

//        IsMoving = false;
//        Debug.Log("移动结束");

//        if (animator != null)
//            animator.SetBool("isMoving", false);
//    }



//    // 检查是否可以移动到目标节点
//    public bool CanMoveTo(Node targetNode, int actionPoints)
//    {
//        if (IsMoving)
//        {
//            Debug.LogWarning("正在移动中");
//            return false;
//        }

//        if (actionPoints <= 0)
//        {
//            Debug.LogWarning("行动点数不足");
//            return false;
//        }

//        if (CurrentNode == null || !CurrentNode.connectedNodes.Contains(targetNode))
//        {
//            Debug.LogWarning("目标节点不相邻或当前节点未设置");
//            return false;
//        }

//        return true;
//    }
//}

