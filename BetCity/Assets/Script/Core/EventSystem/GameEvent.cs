using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEvent
{
    public static Action<Card, CardOwner, CardOwner> OnCardOwnershipChanged;
    public static void TriggerCardOwnershipChanged(Card card, CardOwner oldOwner, CardOwner newOwner)
    {
        OnCardOwnershipChanged?.Invoke(card, oldOwner, newOwner);
    }
}