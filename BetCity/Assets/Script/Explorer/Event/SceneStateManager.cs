using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    // ʹ�õ���ģʽ��ȷ��ȫ��ֻ��һ��ʵ��
    public static SceneStateManager Instance { get; private set; }

    // ���ڼ�¼��һ������������
    private int lastSceneIndex = -1;

    private void Awake()
    {
        // ����ģʽ�ı�׼ʵ��
        if (Instance == null)
        {
            Instance = this;
            // ������������л�����ʱ��������
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ����Ѿ�����һ��ʵ��������������µģ���֤Ψһ��
            Destroy(gameObject);
        }
    }

    // ������Ҫȥ��һ���³���ʱ�����ô˷�������¼��ǰ����
    public void RecordCurrentScene()
    {
        lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"�Ѽ�¼��ǰ����������Ϊ: {lastSceneIndex}");
    }

    // ����Ҫ����ʱ�����ô˷���
    public void ReturnToLastScene()
    {
        if (lastSceneIndex != -1)
        {
            Debug.Log($"׼�����ص�����: {lastSceneIndex}");
            SceneManager.LoadScene(lastSceneIndex);
        }
        else
        {
            Debug.LogError("û�пɷ��صĳ�����¼��");
        }
    }
}

