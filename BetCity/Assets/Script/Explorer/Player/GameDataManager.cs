using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    // 需要储存的核心数据
    public int StoredActionPoints { get; private set; }
    public string StoredNodeName { get; private set; } // 储存节点的名字

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 公共方法，用于保存玩家数据
    public void SavePlayerData(int actionPoints, Node currentNode)
    {
        StoredActionPoints = actionPoints;
        if (currentNode != null)
        {
            StoredNodeName = currentNode.name;
            Debug.Log($"[GameDataManager] 数据已保存: 行动点数 = {StoredActionPoints}, 所在节点 = {StoredNodeName}");
        }
        else
        {
            StoredNodeName = null;
            Debug.LogWarning("[GameDataManager] 尝试保存数据，但当前节点为空！");
        }
    }

    // 公共方法，用于清除已储存的数据
    public void ClearStoredData()
    {
        StoredActionPoints = 0;
        StoredNodeName = null;
        Debug.Log("[GameDataManager] 已清除储存的玩家数据。");
    }
}
