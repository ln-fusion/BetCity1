using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneLoader : MonoBehaviour // �������� SceneManager �޸�Ϊ MySceneLoader
{
    // �����³�����ж�ص�ǰ����
    public void LoadScene(string sceneName)
    {
        // �Զ�ж�ص�ǰ�����������³���
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
