using BetCity.Core.Tools;
using BetCity.Data.Storage;
using BetCity.Data.ConfigModels;
using BetCity.Core.DialogueSystem;
using BetCity.Core.ActionSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace BetCity.GamePlay.Plot
{
    /// <summary>
    /// 对话管理器：负责存储已触发/已播放的对话状态，触发对话并将其包装为GameAction交由ActionManager执行
    /// 使用ArchiveDataContainer中的OwnedDialogueDTO列表
    /// </summary>
    public class DialogueManager : MonoSingleton<DialogueManager>, ISubmitArchive<OwnedDialogueDTO>
    {
        private StorageManager StorageManager => StorageManager.Instance;
        private DialogueDataManager DialogueDataManager => DialogueDataManager.Instance;

        // 所有原型
        private IReadOnlyList<DialogueData> allDialogues => DialogueDataManager.Data;

        // 已拥有（或已解锁/已播放）对话的记录
        private Dictionary<int, OwnedDialogueDTO> ownedDialogues = new Dictionary<int, OwnedDialogueDTO>();

        protected override void Awake()
        {
            base.Awake();
            CacheOwnedDialogueInstances();
        }

        private void CacheOwnedDialogueInstances()
        {
            ownedDialogues.Clear();
            var archive = StorageManager?.ArchiveDataContainer;
            if (archive == null) return;
            var dtos = archive.OwnedDialogueDTOs;
            if (dtos == null) return;
            foreach (var dto in dtos)
            {
                if (!ownedDialogues.ContainsKey(dto.Id))
                    ownedDialogues.Add(dto.Id, dto);
            }
        }

        /// <summary>
        /// 尝试触发满足条件的对话，触发后将其包装为DialogueAction并交给ActionManager执行
        /// </summary>
        public bool TryTriggerDialogue(string timing, object source = null, object target = null)
        {
            // 找到所有满足timing且条件匹配的dialogue
            var candidates = allDialogues.Where(d => d.TriggerTiming == timing).ToList();
            if (candidates.Count == 0) return false;

            // 条件检查,目前只检查是否在ownedDialogues中的播放标记
            var valid = candidates.OrderBy(d => d.Priority).FirstOrDefault();
            if (valid == null) return false;

            // 如果一次性且已播放则不触发
            if (valid.IsOneTime && ownedDialogues.ContainsKey(valid.Id) && ownedDialogues[valid.Id].HasPlayed)
                return false;

            // 创建action并交给ActionManager
            var context = new GameActionContext(source, target, null);
            var action = new DialogueAction(context, valid);
            ActionManager.Instance.Perform(action);

            // 标记为已播放（如果是一次性或需要记录）
            MarkPlayed(valid.Id);
            return true;
        }

        private void MarkPlayed(int id)
        {
            if (ownedDialogues.ContainsKey(id))
            {
                var old = ownedDialogues[id];
                var updated = new OwnedDialogueDTO(old.Id, true, old.ExtraData);
                ownedDialogues[id] = updated;
            }
            else
            {
                ownedDialogues[id] = new OwnedDialogueDTO(id, true, new Dictionary<string, object>());
            }
            SaveArchive();
        }

        private void SaveArchive()
        {
            var list = ownedDialogues.Values.ToList();
            SubmitArchive(list);
        }

        public void SubmitArchive(List<OwnedDialogueDTO> t)
        {
            StorageManager.ModifyArchive(t, this);
        }
    }
}
