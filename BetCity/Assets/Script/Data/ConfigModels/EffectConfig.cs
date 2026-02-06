using BetCity.Core.EffectSystem;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 被动效果配置（自动订阅事件并响应）
    /// </summary>
    [CreateAssetMenu(fileName = "Effect", menuName = "Effect/EffectConfig")]
    public class EffectConfig : ScriptableObject
    {
        /// <summary>
        /// 效果来源
        /// </summary>
        public object Source { get; set; }
        /// <summary>
        /// 唯一标识
        /// </summary>
        [field: SerializeField] public int Id { get; protected set; }
        /// <summary>
        /// 效果名称
        /// </summary>
        [field: SerializeField] public string Name { get; protected set; }
        /// <summary>
        /// 描述文本
        /// </summary>
        [field: TextArea][field: SerializeField] public string Description { get; protected set; }
        /// <summary>
        /// 载体类型（藏品/卡牌/Buff）
        /// </summary>
        [field: SerializeField] public EffectCarrier CarrierType { get; protected set; }
        /// <summary>
        /// 触发条件（订阅的事件）
        /// </summary>
        [field: SerializeField] public TriggerCondition TriggerCondition { get; private set; }
        /// <summary>
        /// 生命周期类型
        /// </summary>
        [field: SerializeField] public EffectLifetime Lifetime { get; protected set; }
        /// <summary>
        /// 持续时间（回合，根据Lifetime生效）
        /// </summary>
        [field: SerializeField] public int Duration { get; protected set; }
        /// <summary>
        /// 持续回合结束动作类型
        /// </summary>
        [field: SerializeField] public EndTurnReaction EndTurnReaction { get; protected set; }

        /// <summary>
        /// 激活效果
        /// </summary>
        public bool Activate()
        {
            switch (Lifetime)
            {
                case EffectLifetime.Permanent:
                    return EffectManager.Instance.ActivatePermanentPassiveEffect(Id, CarrierType, TriggerCondition);
                case EffectLifetime.Timed:
                    return EffectManager.Instance.ActivateTimedPassiveEffect(Id, CarrierType, TriggerCondition, Duration, EndTurnReaction);
                case EffectLifetime.OneShot:
                    return EffectManager.Instance.ActivateOneShotPassiveEffect(Id, CarrierType);
            }
            return true;
        }

        /// <summary>
        /// 取消激活效果（一次性效果不需要取消激活）
        /// </summary>
        public bool Deactivate()
        {
            switch (Lifetime)
            {
                case EffectLifetime.Permanent:
                    return EffectManager.Instance.DeActivatePassiveEffect(Id);
                case EffectLifetime.Timed:
                    return EffectManager.Instance.DeActivateTimedEffectEndTurnSubscribe(Id) && EffectManager.Instance.DeActivatePassiveEffect(Id);
                case EffectLifetime.OneShot:
                    return true;
            }
            return true;
        }

        [Button("检查ID是否重复")]
        private void CheckIdDuplication()
        {
#if UNITY_EDITOR
            // 1. 校验ID有效性（避免ID为0/负数时的无效检查）
            if (Id < 0)
            {
                EditorUtility.DisplayDialog("提示", "ID无效（≤0），请先设置合法的ID！", "确定");
                return;
            }

            // 2. 查找所有同类型的PassiveEffectConfig资产
            var allPassiveEffectGuids = AssetDatabase.FindAssets($"t:{nameof(EffectConfig)}");
            var conflictAssets = allPassiveEffectGuids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid)) // 转资产路径
                .Select(path => AssetDatabase.LoadAssetAtPath<EffectConfig>(path)) // 加载SO
                .Where(so => so != null && so != this && so.Id == Id) // 排除自己，匹配相同ID
                .ToList();

            // 3. 弹窗反馈检查结果
            if (conflictAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("检查结果", $"ID {Id} 未被其他效果配置占用", "确定");
            }
            else
            {
                // 拼接冲突资产路径
                string conflictList = string.Join("\n", conflictAssets.Select(so =>
                {
                    string assetPath = AssetDatabase.GetAssetPath(so);
                    return $"• {assetPath}";
                }));

                // 显示冲突提示，自动选中第一个冲突资产（方便定位）
                EditorUtility.DisplayDialog(
                    "⚠️ ID重复警告",
                    $"ID {Id} 已被以下配置占用：\n\n{conflictList}",
                    "确定"
                );
                Selection.activeObject = conflictAssets[0];
            }
#endif
        }
    }

    /// <summary>
    /// 效果载体类型
    /// </summary>
    public enum EffectCarrier
    {
        Souvenir, // 藏品
        Card

    }

    /// <summary>
    /// 生命周期类型
    /// </summary>
    public enum EffectLifetime
    {
        Permanent, // 永久生效被动
        Timed, // 时效型被动
        OneShot // 一次性（触发后立即失效）可以是被动/也可以是主动
    }
}