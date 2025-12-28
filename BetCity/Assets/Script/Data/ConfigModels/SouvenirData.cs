using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 存放纪念品原型数据,注意原型数据只能通过Inspector修改
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "Souvenir")]
    public class SouvenirData : ScriptableObject
    {
        /// <summary>
        /// Id为主键索引
        /// </summary>
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: TextArea]
        [field: SerializeField] public string Info { get; private set; }
        [field: SerializeField] public int ArtworkID { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        /// <summary>
        /// 纪念品分类，战斗类/探索类
        /// </summary>
        [field: SerializeField] public SouvenirCategory Category { get; private set; }
        /// <summary>
        /// 纪念品的效果（都是被动）
        /// </summary>
        [field: SerializeField] public List<EffectConfig> Effects { get; private set; }
    }

    public enum SouvenirCategory
    {
        battle, explorer
    }
}