using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 存放纪念品原型数据,注意原型数据只能通过Inspector修改
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "Souvenir")]
public class SouvenirData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; } // id为主键
    [field: SerializeField] public string Name { get; private set; }
    [field: TextArea]
    [field: SerializeField] public string Info { get; private set; }
    [field: SerializeField] public int ArtworkID { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [field: SerializeField] public SouvenirCategory Category { get; private set; }
}

public enum SouvenirCategory{
    battle, explorer
}