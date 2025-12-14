using UnityEngine;

public class Card
{
    public int id;
    public string cardName;
    public string description;
    public int cardArtworkid;
    public CardSeries series;
    public Sprite cardArtwork;

    private CardOwner _owner;
    public CardOwner owner
    {
        get => _owner;
        set
        {
            if (_owner != value)
            {
                CardOwner oldOwner = _owner;
                _owner = value;
                OnOwnerChanged?.Invoke(this, oldOwner, value);
                //GameEvent.TriggerCardOwnershipChanged(this, oldOwner, value);
            }
        }
    }

    public System.Action<Card, CardOwner, CardOwner> OnOwnerChanged;

    public Card(int _id, string _cardName, string _description, int _cardArtworkid, CardSeries _series, CardOwner _owner)
    {
        this.id = _id;
        this.cardName = _cardName;
        this.description = _description;
        this.cardArtworkid = _cardArtworkid;
        this.series = _series;
        this.owner = _owner;
        LoadCardArtwork();
    }

    // 加载卡图的方法
    public void LoadCardArtwork()
    {
        string path = $"Image/CardImage/{cardArtworkid}";
        cardArtwork = Resources.Load<Sprite>(path);
    }
}

// 怪兽卡类
public class MonsterCard : Card
{
    public int score;
    public bool isActive = false; 

    public MonsterCard(int _id, string _cardName, string _description,
        int _cardArtworkid, int _score, CardOwner _owner, CardSeries _series)
        : base(_id, _cardName, _description, _cardArtworkid, _series, _owner)
    {
        this.score = _score;
    }
}

// 魔法卡类
public class SpellCard : Card
{
    public SpellCard(int _id, string _cardName, string _description,
        int _cardArtworkid, CardOwner _owner, CardSeries _series)
        : base(_id, _cardName, _description, _cardArtworkid, _series, _owner)
    {
    }
}

// 卡牌所有者枚举
public enum CardOwner
{
    PlayerA,
    PlayerB,
    None
}

// 卡牌系列枚举
public enum CardSeries
{
    None,
    Memory, // 记忆系列
    Burn,   // 灼烧系列
    Root,
}