using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存储玩家拥有的纪念品
/// </summary>
[Serializable]
public class OwnedSouvenirDTO
{
    public int Id { get; set; }          // 关联原型ID
    public int CustomPrice { get; set; } // 玩家修改后的价格（无修改则等于原型）

    public OwnedSouvenirDTO() { }

    public OwnedSouvenirDTO(int id, int customPrice)
    {
        Id = id;
        CustomPrice = customPrice;
    }
}

/// <summary>
/// 存档容器
/// </summary>
[Serializable]
public class OwnedSouvenirContainer
{
    public List<OwnedSouvenirDTO> OwnedSouvenirs = new List<OwnedSouvenirDTO>();
    //public int SaveVersion = 1;  版本兼容
}