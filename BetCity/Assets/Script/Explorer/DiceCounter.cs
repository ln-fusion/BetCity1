// DiceCounter.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCounter : MonoBehaviour
{
    [Header("����ͼƬ (����0��Ϊ�ջ�Ϊ������ͼƬ������1-6��Ӧ1-6��)")]
    public Sprite[] AllNumbers; // AllNumbers[0] = null/rolling, AllNumbers[1]=1��, ..., AllNumbers[6]=6��
    public SpriteRenderer Render;

    [Header("��������")]
    public float PerTime = 0.05f; // ÿ֡ͼƬ�л�ʱ��
    public float ProgressTime = 2f; // ����������ʱ��

    private Coroutine currentRollCoroutine; // ���ڹ���Э�̣������ظ�����

    // �������������һ���¼���֪ͨ�ⲿ���ӹ��������������ݽ��
    public System.Action<int> OnDiceRollFinished;

    private void Awake() // ʹ�� Awake ȷ�� Render ������ Start ֮ǰ����
    {
        if (Render == null)
        {
            Render = GetComponent<SpriteRenderer>();
            if (Render == null)
            {
                Debug.LogError("DiceCounter: SpriteRenderer δ��ֵ��δ�ҵ�����ȷ�������ڴ��� SpriteRenderer �� GameObject �ϡ�");
            }
        }
        // ȷ�� AllNumbers ���������� 7 ��Ԫ�� (���� 0-6)
        if (AllNumbers == null || AllNumbers.Length < 7)
        {
            Debug.LogError("DiceCounter: AllNumbers ����δ��ֵ�򳤶Ȳ���7������ Inspector �����á�");
        }
    }

    // �ⲿ���ô˷������������ӽ������ʼ����
    public void SetResultIndexAndAnimate(int finalResult)
    {
        if (finalResult < 1 || finalResult > 6)
        {
            Debug.LogError($"DiceCounter: ��Ч�����ӽ�� {finalResult}����������� 1 �� 6 ֮�䡣");
            return;
        }

        if (currentRollCoroutine != null)
        {
            StopCoroutine(currentRollCoroutine);
        }
        currentRollCoroutine = StartCoroutine(ShowProgress(finalResult));
    }

    IEnumerator ShowProgress(int finalResult)
    {
        // ��ѡ���ڶ�����ʼʱ��ʾһ���������С���ͼƬ
        if (AllNumbers.Length > 0 && AllNumbers[0] != null)
        {
            Render.sprite = AllNumbers[0]; // ���� AllNumbers[0] �ǹ����е�ͼƬ
        }

        float timer = 0;
        while (timer < ProgressTime)
        {
            // ���ѡ�� 1 �� 6 ��ͼƬ������ʾ
            int index = Random.Range(1, 7);
            if (Render != null && AllNumbers.Length > index && AllNumbers[index] != null)
            {
                Render.sprite = AllNumbers[index];
            }
            timer += PerTime;
            yield return new WaitForSeconds(PerTime);
        }

        // ������������ʾ���ս��
        if (Render != null && AllNumbers.Length > finalResult && AllNumbers[finalResult] != null)
        {
            Render.sprite = AllNumbers[finalResult];
        }
        else
        {
            Debug.LogError($"DiceCounter: �޷���ʾ���ս�� {finalResult}��AllNumbers ������û�ж�Ӧ�� Sprite��");
        }

        // ֪ͨ�ⲿ���ӹ�������
        OnDiceRollFinished?.Invoke(finalResult);

        currentRollCoroutine = null; // Э�̽������������
    }

    // �������������һ�����������ⲿ��ѯ�����Ƿ����ڹ���
    public bool IsRolling()
    {
        return currentRollCoroutine != null;
    }
}
