using System.Collections.Generic;
using UnityEngine;
using BetCity.Core.Tools;
using BetCity.Core.DialogueSystem; 

namespace BetCity.Data.ConfigModels
{
    public class DialogueData : ScriptableObject, IDialogue
    {
        [SerializeField]
        private int id;
        [SerializeField]
        private int priority = 100;//优先级，当多个对话满足触发条件时，系统会根据 priority 决定先执行哪个
        [SerializeField]
        private SerializableDictionary<string, List<string>> conditions;
        [SerializeField]
        private string triggerTiming;
        [SerializeField]
        private bool isOneTime = false;
        [SerializeField]
        private List<DialogueLine> lines = new List<DialogueLine>();

        public int Id => id;
        public int Priority => priority;
        public SerializableDictionary<string, List<string>> Conditions => conditions;
        public string TriggerTiming => triggerTiming;
        public bool IsOneTime => isOneTime;
        public List<DialogueLine> Lines => lines;
    }
}
