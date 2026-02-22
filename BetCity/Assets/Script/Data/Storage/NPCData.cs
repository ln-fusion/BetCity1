using UnityEngine;
using BetCity.Core.DialogueSystem;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 存放 NPC 原型数据，通过 Inspector 修改（与 CardData 写法一致）
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "NPC/NPCData")]
    public class NPCData : ScriptableObject
    {
        // 模板唯一 ID（用于映射运行时实例）
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public string PortraitCharacterId { get; private set; }
        [field: SerializeField] public Expression DefaultExpression { get; private set; } = Expression.Neutral;
        [field: SerializeField] public string DialogueTrigger { get; private set; }
        [field: SerializeField] public bool IsPersistent { get; private set; } = true;

    }
}
