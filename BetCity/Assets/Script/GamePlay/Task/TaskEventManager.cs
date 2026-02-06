using BetCity.Core.EventSystem;
using BetCity.Data.ConfigModels;
using BetCity.Tools.Test;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 任务板事件管理器
    /// </summary>
    public class TaskEventManager: BaseEventManager<TaskEvent, TaskEventManager>
    {
        public IReadOnlyDictionary<int, TaskEvent> TaskEvent => EventLoader.Instance.TaskEvents;

        /// <summary>
        /// 进入任务板事件
        /// </summary>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            base.EnterEvent(cancellationToken, id);
            return UniTask.CompletedTask;
        }
    }
}
