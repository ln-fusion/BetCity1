using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEvent
{
    // ȫ�ֿ�������Ȩ����¼�
    public static Action<Card, CardOwner, CardOwner> OnCardOwnershipChanged;

    // ����ȫ���¼��ķ���
    public static void TriggerCardOwnershipChanged(Card card, CardOwner oldOwner, CardOwner newOwner)
    {
        OnCardOwnershipChanged?.Invoke(card, oldOwner, newOwner);
    }
}