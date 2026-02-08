using BetCity.Core.Tools;
using System.Collections.Generic;

namespace BetCity.Core.DialogueSystem
{
    /// <summary>
    /// 对话接口定义
    /// </summary>
    public interface IDialogue
    {
        int Id { get; }
        int Priority { get; }
        SerializableDictionary<string, List<string>> Conditions { get; }
        string TriggerTiming { get; }
        bool IsOneTime { get; }
        List<DialogueLine> Lines { get; }
    }
}
