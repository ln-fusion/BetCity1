using BetCity.Core.ActionSystem;
using BetCity.Core.EffectSystem;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 通用效果配置基类，可被藏品、Buff、卡牌等复用
    /// </summary>
    public abstract class EffectConfig : ScriptableObject
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
        /// 效果类型
        /// </summary>
        [field: SerializeField] public EffectType Type { get; protected set; } 
        /// <summary>
        /// 载体类型（藏品/卡牌/Buff）
        /// </summary>
        [field: SerializeField] public EffectCarrier CarrierType { get; protected set; }

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
            var allPassiveEffectGuids = AssetDatabase.FindAssets($"t:{nameof(PassiveEffectConfig)}");
            var conflictAssets = allPassiveEffectGuids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid)) // 转资产路径
                .Select(path => AssetDatabase.LoadAssetAtPath<PassiveEffectConfig>(path)) // 加载SO
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

        /// <summary>
        /// 激活效果（由载体调用，如藏品被拾取、Buff被添加、卡牌被使用时）
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// 取消激活效果
        /// </summary>
        public abstract void Deactivate();       
    }

    public enum EffectType
    {
        /// <summary>
        /// 被动（订阅回调）
        /// </summary>
        Passive,
        /// <summary>
        /// 主动（发出动作）
        /// </summary>
        Active,
    }

    /// <summary>
    /// 效果载体类型
    /// </summary>
    public enum EffectCarrier
    {
        Souvenir // 藏品
    }
}
