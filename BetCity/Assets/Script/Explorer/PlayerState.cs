using UnityEngine;
using System.Collections.Generic;

public class PlayerState
{
    public int sanity;             // ����ֵ
    public Vector3 position;      // ���λ��
    public int actionPoints;      // �ж�����
    public List<Item> inventory;  // ����չ�ĵ���ϵͳ

    // ���캯��
    public PlayerState(int sanity, Vector3 position, int actionPoints, List<Item> inventory = null)
    {
        this.sanity = sanity;
        this.position = position;
        this.actionPoints = actionPoints;
        this.inventory = inventory ?? new List<Item>(); // ���û�д���inventory�����ʼ��Ϊ���б�
    }

    // �������״̬
    public void UpdateState(int newSanity, Vector3 newPosition, int newActionPoints, List<Item> newInventory = null)
    {
        this.sanity = newSanity;
        this.position = newPosition;
        this.actionPoints = newActionPoints;
        this.inventory = newInventory ?? this.inventory;
    }
}
