//using UnityEngine;


//public class PlayerStateSaver : MonoBehaviour
//{
//    // 内部组件引用
//    private PlayerAction playerAction;
//    private PlayerMovement playerMovement;

//    private void Awake()
//    {
//        // 自动获取挂载在同一个游戏对象上的其他组件
//        playerAction = GetComponent<PlayerAction>();
//        playerMovement = GetComponent<PlayerMovement>();
//    }

//    // 保存当前状态到 GameDataManager
//    public void SaveState()
//    {
//        // 检查 GameDataManager 是否存在
//        if (GameDataManager.Instance == null)
//        {
//            Debug.LogError("错误: GameDataManager 实例未找到！无法保存玩家状态。");
//            return;
//        }

//        Debug.Log("[PlayerStateSaver] 正在执行 SaveState...");
//        // 从 PlayerAction 和 PlayerMovement 获取当前数据
//        int currentActionPoints = playerAction.ActionPoints;
//        Node currentNode = playerMovement.CurrentNode;

//        // 调用 GameDataManager 的保存方法
//        GameDataManager.Instance.SavePlayerData(currentActionPoints, currentNode);
//    }

//    // 从 GameDataManager 恢复状态
//    public void RestoreState(Node defaultStartNode)
//    {
//        // 检查 GameDataManager 是否存在，可以删除
//        if (GameDataManager.Instance == null)
//        {
//            Debug.LogError("错误: GameDataManager 实例未找到！无法恢复玩家状态。");
//            // 即使出错，也初始化到默认位置，避免游戏卡死
//            playerMovement.InitAtNode(defaultStartNode);
//            return;
//        }

//        // 检查是否有可恢复的数据，储存方式需要修改
//        if (!string.IsNullOrEmpty(GameDataManager.Instance.StoredNodeName))
//        {
//            Debug.Log("[PlayerStateSaver] 发现已储存的数据，正在执行 RestoreState...");

//            // 1. 恢复行动点数
//            int restoredActionPoints = GameDataManager.Instance.StoredActionPoints;
//            playerAction.SetActionPoints(restoredActionPoints);
//            Debug.Log($"行动点数已恢复为: {restoredActionPoints}");

//            // 2. 恢复位置
//            string restoredNodeName = GameDataManager.Instance.StoredNodeName;
//            // 在当前场景中通过名字查找节点
//            GameObject nodeObject = GameObject.Find(restoredNodeName);
//            if (nodeObject != null)
//            {
//                Node targetNode = nodeObject.GetComponent<Node>();
//                playerMovement.InitAtNode(targetNode);
//                Debug.Log($"玩家位置已恢复到节点: {targetNode.name}");
//            }
//            else
//            {
//                Debug.LogError($"无法在当前场景中找到名为 {restoredNodeName} 的节点！将使用默认初始节点。");
//                playerMovement.InitAtNode(defaultStartNode);

//            }

//            // 3. 【重要】清除 GameDataManager 中的数据，避免下次返回时错误地重用
//            GameDataManager.Instance.ClearStoredData();
//        }
//        else
//        {
//            // 如果没有数据，说明是游戏初次启动，使用默认节点初始化
//            Debug.Log("[PlayerStateSaver] 未发现需恢复的数据，执行标准初始化。");
//            playerMovement.InitAtNode(defaultStartNode);
//        }
//    }
//}
