using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对应的纪念品包装
/// </summary>
public class Souvenir
{
    private readonly SouvenirData souvenirData;

    //暂时除了金钱属性其他默认不能修改，如有需要可以更改（我搞错了这个不是映射，=>代表只读属性）
    public int id => souvenirData.Id;
    public string name => souvenirData.Name;
    public string info => souvenirData.Info;
    public int artworkID => souvenirData.ArtworkID;
    public Sprite image => souvenirData.Image;
    public SouvenirCategory category => souvenirData.Category;
    public int Price {  get; set; }
    public bool IsOwned {  get; set; } //玩家是否拥有 
    public Souvenir(SouvenirData souvenirData, bool isOwned = false)
    {
        this.souvenirData = souvenirData;
        this.Price = souvenirData.Price;
        this.IsOwned = isOwned;
    }
}

