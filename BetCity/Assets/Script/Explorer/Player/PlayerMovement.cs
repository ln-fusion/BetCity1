using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 0.5f; // 移动速度
    [SerializeField] private float playerHeight = 0.5f; // 玩家高度

    public Node CurrentNode { get; private set; } // 当前所在节点
    public bool IsMoving { get; private set; } // 是否正在移动


    // 初始化玩家位置
    public void InitAtNode(Node node)
    {
        CurrentNode = node;
        transform.position = node.transform.position + Vector3.up * playerHeight;
    }

    // 尝试移动到目标节点
    public IEnumerator MoveToNode(Node targetNode)
    {
        IsMoving = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetNode.transform.position + Vector3.up * playerHeight;
        float elapsed = 0f;
        float duration = moveSpeed; // 将 moveSpeed 视为移动持续时间

        // 这里的循环条件应该基于持续时间
        while (elapsed < duration)
        {
            // 使用 Lerp 的第三个参数 t，其值应该在 0 到 1 之间
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终位置精确
        transform.position = endPos;

        CurrentNode = targetNode;
        IsMoving = false;
        Debug.Log($"移动完成，当前节点: {CurrentNode.name}");
    }

    // 检查是否可以移动到目标节点
    public bool CanMoveTo(Node targetNode, int actionPoints)
    {
        if (IsMoving)
        {
            Debug.LogWarning("正在移动中");
            return false;
        }

        if (actionPoints <= 0)
        {
            Debug.LogWarning("行动点数不足");
            return false;
        }

        if (CurrentNode == null || !CurrentNode.connectedNodes.Contains(targetNode))
        {
            Debug.LogWarning("目标节点不相邻或当前节点未设置");
            return false;
        }

        return true;
    }
}

