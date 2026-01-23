using BetCity.Core.ProgressSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.Core.EventSystem
{
    /// <summary>
    /// 任何对应的管理器需要继承的类
    /// </summary>
    public abstract class BaseEventManager<T> : MonoSingleton<BaseEventManager<T>> where T : BaseEvent
    {
        /// <summary>
        /// 当前事件
        /// </summary>
        public T CurrentEvent {  get; protected set; }

        /// <summary>
        /// 当前事件状态,无事件状态赋值为"None"
        /// </summary>
        public string CurrentEventState
        {
            get { return _currentEventState; }
            protected set
            {
                if (_currentEventState != value)
                {
                    _currentEventState = value;
                    OnEventStateChange();
                }
            }
        }
        private string _currentEventState = "None";

        /// <summary>
        /// 事件状态变化
        /// </summary>
        public virtual void OnEventStateChange()
        {
            if (CurrentEvent.Dialogues.ContainsKey(CurrentEventState))
            {
                throw new NotImplementedException();
            }
            else if(CurrentEventState == "End")
            {
                CurrentEventState = "None";
            }
            return;
        }

        /// <summary>
        /// 触发指定id的事件默认将事件状态转变为Start
        /// </summary>
        public virtual UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            if(CurrentEventState != "None")
            {
                Debug.LogWarning("[EventManager]在一个事件未结束试图触发一个相同类型的事件!");
                return UniTask.CompletedTask;
            }
            ProgressManager.Instance.EnterEvent(id);
            CurrentEventState = "Start";
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 结束当前事件,将状态转换为None
        /// </summary>
        public virtual UniTask ExitEvent(CancellationToken cancellationToken)
        {
            if (CurrentEventState == "None")
            {
                Debug.LogWarning("[EventManager]当前无事件需要被结束!");
                return UniTask.CompletedTask;
            }
            CurrentEventState = "None";
            return UniTask.CompletedTask;
        }
    }

}