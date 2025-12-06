using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 允许修改Souvenir的接口（目前设计除SouvenirManger应无人继承）
/// </summary>
internal interface IModifySouvenir
{
    bool LoseSouvenirById(int id, out Souvenir souvenir, out string errorMsg);
    bool OwnSouvenirById(int id, out Souvenir souvenir, out string errorMsg);
}

/// <summary>
/// 对应的纪念品包装
/// </summary>
public class Souvenir
{
    private readonly SouvenirData souvenirData;

    //暂时除了金钱属性其他默认不能修改，如有需要可以更改（我搞错了这个不是映射，=>代表只读属性）
    public int Id => souvenirData.Id;
    public string Name => souvenirData.Name;
    public string Info => souvenirData.Info;
    public int ArtworkID => souvenirData.ArtworkID;
    public Sprite Image => souvenirData.Image;
    public SouvenirCategory Category => souvenirData.Category;
    public int Price {  get; set; }
    public bool IsOwned { get; private set; } 
    public Souvenir(SouvenirData souvenirData, bool isOwned = false)
    {
        this.souvenirData = souvenirData;
        this.Price = souvenirData.Price;
        this.IsOwned = isOwned;
    }

    /// <summary>
    /// 修改IsOwned属性，只限实现IModifySouvenir的SouvenirManger访问
    /// <param name="caller">允许修改Souvenir的接口</param>
    /// </summary>
    internal void SetIsOwned(bool isOwned, IModifySouvenir caller)
    {
        // 关键：校验调用者必须是B的实例（防止其他类伪造接口）
        if (caller is not SouvenirManager)
        {
            throw new InvalidOperationException("仅SouvenirManager类可修改Souvenir的Price属性");
        }
        this.IsOwned = isOwned;
    }
}

