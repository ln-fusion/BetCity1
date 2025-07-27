using UnityEngine;
using UnityEngine.SceneManagement;

public class EventSceneController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 检查玩家是否按下了 Escape 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("检测到 Escape 键按下，准备返回地图。");

            // 调用场景状态管理器的返回方法
            // 确保场景中有 SceneStateManager 实例
            /* if (SceneStateManager.Instance != null)
             {
                 SceneStateManager.Instance.ReturnToLastScene();
             }
             else
             {
                 Debug.LogError("找不到 SceneStateManager 实例，无法返回！");
             }*/
            SceneManager.LoadScene(0);
        }
    }
}
