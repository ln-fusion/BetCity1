using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance; // ����ģʽ�������

    public Tilemap combatGrid; // ����Tilemap
    public List<Card> publicDeck; // �����ƿ�
    public List<Card> DiscardPile; // A�����ƶ�

    private List<Card> playerAHand; // A������
    private List<Card> playerBHand; // B������

    private CardOwner currentPlayer; // ��ǰ�غ����

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        // ��ʼ���ƿ⡢���Ƶ�
        // ���ݲ߻��ĵ�����˫��25�ſ��ƻ��ϴ�ƺ��γɹ����ƿ�
        // ˫������5������
        // ȷ���������
    }

    public void StartTurn()
    {
        // �غϿ�ʼ�߼�
        // �ж��Ƿ�Ϊ���ֵ�һ�غ���Ȩ
        // ����鿨�׶�
    }

    private void DrawPhase()
    {
        // �鿨�߼���Ͷ��d4����ȡX�ſ�
        // �ƿ������߼�
        // ����Ӳ�ҴӶ������Ƴ鿨�߼�
        // �������Ʋ���ͷ��߼�
    }

    private void PlayPhase()
    {
        // �����߼�������Ӷ������Ƴ�ȡ�Ŀ���
        // ����Ч�������߼�
    }

    private void CombinationPhase()
    {
        // ������Ͻ����߼�
    }

    private void EndPhase()
    {
        // �����׶��߼����������ޡ�ʤ���ж�
    }

    // �����������������磺
    // public bool IsGridOccupied(Vector3Int gridPosition)
    // public void PlaceCardOnGrid(Card card, Vector3Int gridPosition)
    // public void RemoveCardFromGrid(Vector3Int gridPosition)
}
