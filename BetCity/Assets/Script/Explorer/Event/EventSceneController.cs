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

            SceneManager.LoadScene(0);
        }
    }
}
