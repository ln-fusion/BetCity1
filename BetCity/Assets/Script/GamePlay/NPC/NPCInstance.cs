using System;
using UnityEngine;

namespace BetCity.GamePlay.NPC
{
    [Serializable]
    public class NPCInstance
    {
        // 实例唯一ID
        public string InstanceId;
        // 对应的模板ID
        public string TemplateId;
        // 存储的位置信息
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        // 是否为固定NPC
        public bool IsPersistent = true;
        // 用于立绘查找的CharacterId
        public string PortraitCharacterId;
        // 当前表情
        public BetCity.Core.DialogueSystem.Expression CurrentExpression = BetCity.Core.DialogueSystem.Expression.Neutral;
        // 运行时生成的GameObject引用
        [NonSerialized]
        public GameObject RuntimeObject;
    }
}
