using UnityEngine;

public class Item
{
    public string itemName;  // ��Ʒ����
    public int itemID;       // ��ƷID�������������ֲ�ͬ����Ʒ��
    public string description; // ��Ʒ����

    // ���캯��
    public Item(string itemName, int itemID, string description = "")
    {
        this.itemName = itemName;
        this.itemID = itemID;
        this.description = description;
    }

    // ����Ը�����Ҫ��չ�������Ʒ���ԣ�����۸��;öȡ�ͼ���
}
