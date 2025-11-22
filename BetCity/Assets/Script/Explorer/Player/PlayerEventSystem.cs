
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEventSystem : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private MySceneLoader mySceneLoader; // 场景加载器
    [SerializeField] private SanityManager sanitymanager; // 场景加载器

    [Header("事件场景索引")]
    [SerializeField] private int[] eventSceneIndices = { 2, 3, 4, 5 }; // 事件场景的索引

    

    public bool IsInEvent { get; private set; } = false; // 标记是否在事件场景中

    private Vector3 savedCameraPosition; // 用于保存摄像机的初始位置



    // 【新增】引用 PlayerController
    private PlayerController playerController;



    private void Awake()
    {


        // 【新增】获取 PlayerController 的引用
        // 因为它们都在同一个 GameObject 上，所以这样获取最可靠
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerEventSystem 无法找到 PlayerController 组件！", this.gameObject);
        }

        // 确保MySceneLoader实例已赋值
        if (mySceneLoader == null)
        {
            // 尝试在场景中查找MySceneLoader实例
            mySceneLoader = FindObjectOfType<MySceneLoader>();
            if (mySceneLoader == null)
            {
                Debug.LogError("未找到MySceneLoader组件！请确保场景中有一个挂载了MySceneLoader脚本的GameObject。");
            }
        }

        // 保存摄像机的位置
        if (Camera.main != null)
        {
            savedCameraPosition = Camera.main.transform.position;
        }
        else
        {
            Debug.LogError("未找到MainCamera，请确保场景中有一个标记为MainCamera的摄像机！");
        }
    }

    // 切换到事件场景
    public void LoadEventScene(int sceneIndex)
    {
        // 【关键步骤】在加载场景前，调用 PlayerController 的保存方法
        if (playerController != null)
        {
            playerController.SaveStateBeforeLoadingScene(); // 我们将在 PlayerController 中创建这个新方法
        }
        else
        {
            Debug.LogError("PlayerController 引用丢失，无法保存状态！");
            // 即使无法保存，也继续加载场景，避免游戏卡死
        }

        string sceneName = GetSceneNameByIndex(sceneIndex);
        if (mySceneLoader != null)
        {
            mySceneLoader.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("MySceneLoader未引用，无法加载场景！");
        }
        IsInEvent = true;
    }


    // EndEvent 方法现在应该只负责返回主地图的逻辑
    public void EndEvent()
    {
        // 【修改】直接调用场景管理器返回上一场景，而不是写死 "ExplorerMap"
        // 这样更灵活，也符合我们之前的设计
        if (SceneStateManager.Instance != null)
        {
            SceneStateManager.Instance.ReturnToLastScene();
        }
        else
        {
            Debug.LogError("SceneStateManager 实例未找到，无法返回上一场景！将尝试加载默认地图。");
            mySceneLoader.LoadScene("ExplorerMap"); // 作为后备方案
        }
        IsInEvent = false;
    }


    private string GetSceneNameByIndex(int index)
    {

        switch (index)
        {
            case 0:
                return "ExplorerMap"; // 探索地图
            case 1:
                return "MainCityEvent1"; // 事件场景1
            case 2:
                return "BattleScene"; // 战斗场景
            case 3:
                return "Event1"; // 事件场景1
            case 4:
                return "Event2"; // 事件场景2
            case 5:
                return "Event3"; // 事件场景3
            default:
                Debug.LogWarning($"未知的场景索引: {index}，将返回默认的探索地图。");
                return "ExplorerMap"; // 默认返回探索场景
        }
    }

    // 触发随机事件
    public void TriggerRandomEvent()
    {
        int randomIndex = Random.Range(3, eventSceneIndices.Length);
        int sceneToLoad = eventSceneIndices[randomIndex];
        LoadEventScene(sceneToLoad);
    }

    // 触发战斗
    public void TriggerBattle()
    {
        int sceneToLoad = eventSceneIndices[0]; // 假设战斗场景在事件场景索引中
        LoadEventScene(sceneToLoad);
    }

    // CheckForNodeEvent 方法现在会正确地触发带有保存功能的 LoadEventScene
    public void CheckForNodeEvent(Node node)
    {
        switch (node.nodeType)
        {
            case NodeType.RandomEvent:
                TriggerRandomEvent();
                break;
            case NodeType.FixedEvent:
                if (node.fixedEventSceneIndex != -1)
                {
                    Debug.Log($"触发固定事件，加载场景索引: {node.fixedEventSceneIndex}");
                    // 这个调用现在会先保存状态，再加载场景
                    LoadEventScene(node.fixedEventSceneIndex);
                }
                else
                {
                    Debug.LogWarning($"节点 {node.name} 的NodeType是FixedEvent，但未指定fixedEventSceneIndex。");
                }
                break;
            // ... 其他 case 保持不变 ...
            case NodeType.Battle:
                TriggerBattle();
                break;
            case NodeType.Normal:

                Debug.Log("当前节点是普通节点，无事件触发。");
                //  sanitymanager.IncreaseSanity(10);
                //   Debug.Log("理智+10");
                break;

            case NodeType.Rest:
                Debug.Log("当前节点是休息节点，休息。");
                //  sanitymanager.IncreaseSanity(10);
                //   Debug.Log("理智+10");
                break;
            default:
                Debug.LogWarning($"未处理的节点类型: {node.nodeType}");
                break;
        }
    }
}

