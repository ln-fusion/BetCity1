using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 城市事件
    /// </summary>
    [CreateAssetMenu(fileName = "City", menuName = "City/CharacterData")]
    /// <summary>
    /// 存放城市中的NPC数据，注意原型数据只能通过Inspector修改
    /// </summary>
    public class CityNPCData : ScriptableObject
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        [field: SerializeField] public int Id { get; private set; }
        /// <summary>
        /// 所在主城的编号，后期可以删除，因为同一角色可以出现在多个城市中
        /// </summary>
        [field: SerializeField] public int CityId { get; private set; }
        /// <summary>
        /// 人物图片，后期会改成待机动画
        /// </summary>
        [field: SerializeField] public Sprite CharacterImage { get; private set; }

    }
}

