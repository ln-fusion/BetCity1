using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 存放卡牌原型数据，注意原型数据只能通过Inspector修改
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Card/CardData")]
    public class CardData : ScriptableObject
    {
        /// <summary>
        /// Id为主键索引
        /// </summary>
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public string CardName { get; private set; }
        [field: TextArea]
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int ArtworkID { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; }
        [field: SerializeField] public CardSeries Series { get; private set; }
        [field: SerializeField] public CardType Type { get; private set; } // 区分怪兽/魔法卡

        // 怪兽卡特有属性
        [field: SerializeField] public int MonsterScore { get; private set; }
    }

    public enum CardType
    {
        Monster,
        Spell
    }
}

