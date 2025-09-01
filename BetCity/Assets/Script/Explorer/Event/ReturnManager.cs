using UnityEngine;

public class ReturnManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 1. 检查玩家是否按下了 Escape 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("检测到 Escape 键按下，正在通过 SceneStateManager 返回...");

            // 2. 检查 SceneStateManager 是否存在
            if (SceneStateManager.Instance != null)
            {
                // 3. 调用 SceneStateManager 的返回方法
                // 这个方法会加载我们之前用 RecordCurrentScene() 保存的场景索引。
                SceneStateManager.Instance.ReturnToLastScene();
            }
            else
            {
                // 这是一个后备方案，以防 SceneStateManager 意外丢失。
                // 在正常情况下，这行代码不应该被执行。
                Debug.LogError("错误：SceneStateManager 实例未找到！无法智能返回。将尝试加载默认地图场景。");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0); // 假设 0 是主地图
            }
        }
    }
}
