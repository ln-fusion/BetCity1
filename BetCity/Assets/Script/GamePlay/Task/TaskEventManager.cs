using BetCity.Core.ActionSystem;
using BetCity.Core.CheckSystem;
using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Data.ConfigModels;
using BetCity.Tools.Test;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BetCity.GamePlay.Task
{
    /// <summary>
    /// 任务板事件管理器
    /// </summary>
    public class TaskEventManager: BaseEventManager<TaskEvent, TaskEventManager>
    {
        private TaskManager TaskManager => TaskManager.Instance;
        private List<TaskConfig> availableTaskConfigs;

        /// <summary>
        /// 任务板事件合集
        /// </summary>
        public IReadOnlyDictionary<int, TaskEvent> TaskEvents => EventLoader.Instance.TaskEvents;
        /// <summary>
        /// 当前可接任务合集
        /// </summary>
        public IReadOnlyList<TaskConfig> AvailableTaskConfigs => availableTaskConfigs;

        protected override void OnEventStateChange()
        {
            base.OnEventStateChange();
            throw new System.NotImplementedException();
        }

        //加载可接任务合集
        private void LoadAvailableTaskConfigs()
        {
            foreach(int id in CurrentEvent.TaskIds)
            {
                TaskConfig config = TaskManager.TaskConfigs[id];
                if(config == null)
                {
                    Debug.LogError($"[TaskEventManager]不存在id为{id}的任务！");
                }
                if (TaskManager.TaskDataList.First(t  => t.Id == id) == null &&
                    !ProgressManager.Instance.TaskProgress.Contains(id) &&
                    ConditionChecker.Instance.Check(config.Conditions.Init()))
                {
                    availableTaskConfigs.Add(config);
                }
            }
        }

        /// <summary>
        /// （OnEnterTaskNodeAction）进入任务板事件
        /// </summary>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {   
            if(TaskEvents.TryGetValue(id, out TaskEvent taskEvent))
            {
                base.EnterEvent(cancellationToken, id);
                CurrentEvent = taskEvent;
            }
            else
            {
                Debug.LogError($"[StoreEventManager]对应Id为{id}的商店事件不存在！");
                CurrentEventState = "None";
                return UniTask.CompletedTask;
            }
            
            LoadAvailableTaskConfigs();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 创建接受任务动作，并在动作结束后将事件状态设置为"Accept" + 任务id
        /// </summary>
        public void CreateAcceptTaskAction(int id)
        {
            TaskConfig config = availableTaskConfigs.Find(t => t.Id == id);
            if (config == null)
            {
                Debug.LogError($"[TaskEventManager]对应id为{id}的任务不可接取！");
                return;
            }

            OnAcceptTaskAction onAcceptTaskAction = new OnAcceptTaskAction(new GameActionContext(this, id, null));
            ActionManager.Instance.Perform(onAcceptTaskAction, () => { if (onAcceptTaskAction.IsValid) { CurrentEventState = "Accept" + id; } });
        }
    }
}
