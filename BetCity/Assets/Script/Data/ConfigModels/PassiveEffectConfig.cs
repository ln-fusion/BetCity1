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
    [CreateAssetMenu(fileName = "PassiveEffect", menuName = "Effect/PassiveEffectConfig")]
    public class PassiveEffectConfig : EffectConfig
    {
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
        public override void Activate()
        {
            switch (Lifetime)
            {
                case EffectLifetime.Permanent:
                    EffectManager.Instance.ActivatePermanentPassiveEffect(Id, CarrierType, TriggerCondition);
                    break;
                case EffectLifetime.Timed:
                    EffectManager.Instance.ActivateTimedPassiveEffect(Id, CarrierType, TriggerCondition, Duration, EndTurnReaction);
                    break;
                case EffectLifetime.OneShot:
                    EffectManager.Instance.ActivateOneShotPassiveEffect(Id, CarrierType);
                    break;
            }
        }

        /// <summary>
        /// 取消激活效果（一次性效果不需要取消激活）
        /// </summary>
        public override void Deactivate()
        {
            switch (Lifetime)
            {
                case EffectLifetime.Permanent:
                    EffectManager.Instance.DeActivatePassiveEffect(Id);
                    break;
                case EffectLifetime.Timed:
                    EffectManager.Instance.DeActivateTimedEffectEndTurnSubscribe(Id);
                    EffectManager.Instance.DeActivatePassiveEffect(Id);
                    break;
            }
        }
    }

    /// <summary>
    /// 生命周期类型
    /// </summary>
    public enum EffectLifetime
    {
        Permanent, // 永久生效（如藏品被动）
        Timed, // 时效型（如持续3回合）
        OneShot // 一次性（触发后立即失效）
    }
}