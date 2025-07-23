// DiceManager.cs
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private DiceCounter diceCounter; // ���� DiceCounter

    private void Awake()
    {
        if (diceCounter == null)
        {
            Debug.LogError("DiceCounter δ��ֵ�� DiceManager������ Inspector ����ק��ֵ��");
        }
    }

    public int RollDice()
    {
        if (diceCounter == null)
        {
            Debug.LogError("DiceCounter δ��ֵ���޷�Ͷ�����ӻ����ӡ�");
            return 0;
        }

        int result = Random.Range(1, 7); // ��׼6����
        Debug.Log($"���ӽ��: {result}");

        // ���� DiceCounter ����ʾ���ӻ����Ӷ���
        diceCounter.SetResultIndexAndAnimate(result);

        // Ԥ�������¼���չ��
        // if(result >= 6) EventManager.OnFullMoon?.Invoke();

        return result;
    }
}
