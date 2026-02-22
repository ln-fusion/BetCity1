using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BetCity.Core.Tools;
using BetCity.Core.DialogueSystem;
using BetCity.Data.ConfigModels;

namespace BetCity.GamePlay.NPC
{
    /// <summary>
    /// NPC管理器。
    /// 与PortraitDatabase集成用于立绘，与DialogueManager集成用于通过模板的DialogueTrigger触发对话。
    /// </summary>
    public class NPCManager : MonoSingleton<NPCManager>
    {
        [SerializeField]
        private List<NPCData> templates = new List<NPCData>();
        private Dictionary<string, NPCData> templateMap = new Dictionary<string, NPCData>();
        private Dictionary<string, NPCInstance> instances = new Dictionary<string, NPCInstance>();

        [SerializeField]
        private Transform npcParent;

        protected override void Awake()
        {
            base.Awake();
            if (npcParent == null)
            {
                var go = new GameObject("_NPCs");
                DontDestroyOnLoad(go);
                npcParent = go.transform;
            }
            BuildTemplateMap();
        }

        private void BuildTemplateMap()
        {
            templateMap.Clear();
            foreach (var t in templates)
            {
                if (t == null || string.IsNullOrEmpty(t.Id)) continue;
                templateMap[t.Id] = t;
            }
        }

        public void RegisterTemplate(NPCData template)
        {
            if (template == null || string.IsNullOrEmpty(template.Id)) return;
            templates.Add(template);
            templateMap[template.Id] = template;
        }

        public NPCInstance SpawnFixed(string templateId, Vector3 position, Quaternion rotation)
        {
            if (!templateMap.TryGetValue(templateId, out var tpl)) return null;
            var instance = new NPCInstance()
            {
                InstanceId = System.Guid.NewGuid().ToString(),
                TemplateId = tpl.Id,
                Position = position,
                Rotation = rotation,
                IsPersistent = tpl.IsPersistent,
                PortraitCharacterId = tpl.PortraitCharacterId,
                CurrentExpression = tpl.DefaultExpression
            };

            // 实例化预制体
            if (tpl.Prefab != null)
            {
                var go = GameObject.Instantiate(tpl.Prefab, position, rotation, npcParent);
                instance.RuntimeObject = go;
                var helper = go.GetComponent<NPCViewHelper>();
                if (helper == null)
                {
                    helper = go.AddComponent<NPCViewHelper>();
                }
                helper.InstanceId = instance.InstanceId;
                helper.Manager = this;
                helper.ApplyPortrait();
            }

            instances[instance.InstanceId] = instance;
            return instance;
        }

        public void Despawn(string instanceId)
        {
            if (!instances.TryGetValue(instanceId, out var inst)) return;
            if (inst.RuntimeObject != null)
            {
                GameObject.Destroy(inst.RuntimeObject);
                inst.RuntimeObject = null;
            }
            instances.Remove(instanceId);
        }

        public NPCInstance GetById(string instanceId)
        {
            instances.TryGetValue(instanceId, out var inst);
            return inst;
        }

        public IEnumerable<NPCInstance> GetAll()
        {
            return instances.Values.ToList();
        }

        // 由 NPCViewHelper 在玩家交互时调用
        public void OnPlayerInteract(string instanceId)
        {
            var inst = GetById(instanceId);
            if (inst == null) return;
            var portrait = PortraitDatabase.Instance?.GetPortrait(inst.PortraitCharacterId, inst.CurrentExpression);
            if (templateMap.TryGetValue(inst.TemplateId, out var tpl) && !string.IsNullOrEmpty(tpl.DialogueTrigger))
            {
                // 通过 DialogueManager 触发对话
                BetCity.GamePlay.Plot.DialogueManager.Instance.TryTriggerDialogue(tpl.DialogueTrigger, this, inst);
            }
        }

        // 更新指定实例的表情并应用到视图
        public void SetExpression(string instanceId, Expression expr)
        {
            var inst = GetById(instanceId);
            if (inst == null) return;
            inst.CurrentExpression = expr;
            if (inst.RuntimeObject != null)
            {
                var helper = inst.RuntimeObject.GetComponent<NPCViewHelper>();
                helper?.ApplyPortrait();
            }
        }
    }
}
