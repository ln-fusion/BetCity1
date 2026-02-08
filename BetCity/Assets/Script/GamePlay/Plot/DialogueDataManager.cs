using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;

namespace BetCity.GamePlay.Plot
{
    public class DialogueDataManager : MonoSingleton<DialogueDataManager>
    {
        public IReadOnlyList<DialogueData> Data => _data;
        private List<DialogueData> _data = new List<DialogueData>();
        private Dictionary<int, DialogueData> _dict = new Dictionary<int, DialogueData>();

        public const string DIALOGUE_DATA_RESOURCES_PATH = "Dialogue";
        [SerializeField] private string dialogueDataResourcesPath = DIALOGUE_DATA_RESOURCES_PATH;

        protected override void Awake()
        {
            base.Awake();
            LoadAllDialogueData();
        }

        private void LoadAllDialogueData()
        {
            try
            {
                string path = string.IsNullOrEmpty(dialogueDataResourcesPath) ? DIALOGUE_DATA_RESOURCES_PATH : dialogueDataResourcesPath;
                var loaded = Resources.LoadAll<DialogueData>(path);
                if (loaded == null || loaded.Length == 0)
                {
                    Debug.LogWarning($"[DialogueDataManager] 未在Resources/{path}路径下找到 DialogueData 资源");
                    return;
                }
                _data.Clear();
                _dict.Clear();
                foreach (var d in loaded)
                {
                    if (d == null) continue;
                    _data.Add(d);
                    _dict[d.Id] = d;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueDataManager] 加载失败：{e}");
            }
        }

        public DialogueData GetDataById(int id)
        {
            if (_dict.TryGetValue(id, out var d)) return d;
            return null;
        }
    }
}
