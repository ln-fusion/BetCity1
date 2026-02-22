using UnityEngine;
using UnityEngine.UI;
using BetCity.Core.DialogueSystem;

namespace BetCity.GamePlay.NPC
{
    /// <summary>
    /// 附加到NPC预制体的辅助组件。
    /// 暴露public的InstanceId，便于NPCManager将运行时对象映射到实例数据。
    /// 使用PortraitDatabase将立绘应用到SpriteRenderer或 UI Image。
    /// 提供一个简单的触发器回调，在玩家进入时调用NPCManager.OnPlayerInteract。
    /// </summary>
    public class NPCViewHelper : MonoBehaviour
    {
        public string InstanceId;
        public NPCManager Manager;

        [Tooltip("可选的 SpriteRenderer，用于接收立绘精灵")]
        public SpriteRenderer TargetSpriteRenderer;

        //[Tooltip("可选的 UI Image，用于接收立绘精灵")]
       // public Image TargetImage;

        private void Reset()
        {
            TargetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
           // TargetImage = GetComponentInChildren<Image>();
        }

        public void ApplyPortrait()
        {
            if (Manager == null || string.IsNullOrEmpty(InstanceId)) return;
            var inst = Manager.GetById(InstanceId);
            if (inst == null) return;

            var sprite = PortraitDatabase.Instance?.GetPortrait(inst.PortraitCharacterId, inst.CurrentExpression);
            if (sprite != null)
            {
                if (TargetSpriteRenderer != null)
                    TargetSpriteRenderer.sprite = sprite;
               // if (TargetImage != null)
                  //  TargetImage.sprite = sprite;
            }
        }

        // 使用Unity触发器检测玩家并调用管理器
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Manager?.OnPlayerInteract(InstanceId);
            }
        }
    }
}
