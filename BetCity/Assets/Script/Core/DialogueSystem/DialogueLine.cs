using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.DialogueSystem
{
    [Serializable]
    public class DialogueChoice
    {
        public string Text;
        public int NextLineId = -1; // -1为结束
        public DialogueResult Result;
    }

    [Serializable]
    public class DialogueLine
    {
        public int Id;
        public string Speaker;
        // 角色id（用于从 PortraitDatabase 获取立绘）
        public string CharacterId;
        // 表情/差分（使用枚举）
        public Expression Expression = Expression.Neutral;
        //加差分图片
        [TextArea]
        public string Text;
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
        public DialogueResult Result;
    }

    [Serializable]
    public class DialogueResult
    {
        public List<int> TriggerEventIds = new List<int>();
        public List<int> UnlockDialogues = new List<int>();
        public List<string> KVKeys = new List<string>();
        public List<string> KVValues = new List<string>();

        public void Apply()
        { 
            try
            {
                var pm = BetCity.Core.ProgressSystem.ProgressManager.Instance;
                if (pm != null)
                {
                    foreach (var id in TriggerEventIds)
                    {
                        pm.EnterEvent(id);
                    }
                    for (int i = 0; i < Math.Min(KVKeys.Count, KVValues.Count); i++)
                    {
                        pm.SetKVData(KVKeys[i], KVValues[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueResult] Apply error: {e}");
            }
        }
    }
}
