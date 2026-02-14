using System.Collections.Generic;
using UnityEngine;
using BetCity.Core.Tools;
using BetCity.Core.DialogueSystem;

// 使用 JSON 存储对话数据：将 DialogueData 作为可序列化的普通类（非 ScriptableObject）
namespace BetCity.Data.ConfigModels
{
    [System.Serializable]
    public class DialogueData : BetCity.Core.DialogueSystem.IDialogue
    {
        public int Id;
        public int Priority = 100; // 优先级
        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> Conditions;
        public string TriggerTiming;
        public int Times = 1;
        public System.Collections.Generic.List<BetCity.Core.DialogueSystem.DialogueLine> Lines = new System.Collections.Generic.List<BetCity.Core.DialogueSystem.DialogueLine>();

        // 显式接口实现适配
        int BetCity.Core.DialogueSystem.IDialogue.Id => Id;
        int BetCity.Core.DialogueSystem.IDialogue.Priority => Priority;
        System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> BetCity.Core.DialogueSystem.IDialogue.Conditions => Conditions;
        string BetCity.Core.DialogueSystem.IDialogue.TriggerTiming => TriggerTiming;
        int BetCity.Core.DialogueSystem.IDialogue.Times => Times;
        System.Collections.Generic.List<BetCity.Core.DialogueSystem.DialogueLine> BetCity.Core.DialogueSystem.IDialogue.Lines => Lines;
    }
}
