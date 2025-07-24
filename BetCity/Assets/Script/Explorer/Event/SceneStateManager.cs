using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    // 使用单例模式，确保全局只有一个实例
    public static SceneStateManager Instance { get; private set; }

    // 用于记录上一个场景的索引
    private int lastSceneIndex = -1;

    private void Awake()
    {
        // 单例模式的标准实现
        if (Instance == null)
        {
            Instance = this;
            // 让这个对象在切换场景时不被销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已经存在一个实例，就销毁这个新的，保证唯一性
            Destroy(gameObject);
        }
    }

    // 当我们要去往一个新场景时，调用此方法来记录当前场景
    public void RecordCurrentScene()
    {
        lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"已记录当前场景，索引为: {lastSceneIndex}");
    }

    // 当需要返回时，调用此方法
    public void ReturnToLastScene()
    {
        if (lastSceneIndex != -1)
        {
            Debug.Log($"准备返回到场景: {lastSceneIndex}");
            SceneManager.LoadScene(lastSceneIndex);
        }
        else
        {
            Debug.LogError("没有可返回的场景记录！");
        }
    }
}

