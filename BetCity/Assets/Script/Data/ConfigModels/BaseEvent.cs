using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Core.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 所有事件的基类-SO
    /// </summary>
    public abstract class BaseEvent : ScriptableObject
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        [field: SerializeField] public int Id { get; private set; }
        /// <summary>
        /// 名称
        /// </summary>
        [field: SerializeField] public string Name { get; private set; }
        /// <summary>
        /// 事件描述
        /// </summary>
        [field: SerializeField, TextArea] public string Description;
        /// <summary>
        /// 事件类型
        /// </summary>
        [field: SerializeField] public TypeOfEvent Type;
        /// <summary>
        /// 事件出现条件
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, List<string>> Conditions { get; private set; }
        /// <summary>
        /// 事件状态-对话
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, List<int>> Dialogues { get; private set; }
    }

    /// <summary>
    /// 事件类型
    /// </summary>
    public enum TypeOfEvent
    {
        Store
    }
}