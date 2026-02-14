using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using Newtonsoft.Json;
using System.IO;

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
                var jsons = Resources.LoadAll<TextAsset>(path);
                _data.Clear();
                _dict.Clear();
                if (jsons != null && jsons.Length > 0)
                {
                    foreach (var t in jsons)
                    {
                        try
                        {
                            var d = JsonConvert.DeserializeObject<DialogueData>(t.text);
                            if (d == null) continue;
                            _data.Add(d);
                            _dict[d.Id] = d;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[DialogueDataManager] 解析 Dialogue json {t.name} 失败：{e}");
                        }
                    }
                    return;
                }

                string saPath = Path.Combine(Application.streamingAssetsPath, path);
                if (!Directory.Exists(saPath))
                {
                    Debug.LogWarning($"[DialogueDataManager] 未在 Resources 或 StreamingAssets/{path} 路径下找到对话数据");
                    return;
                }
                var files = Directory.GetFiles(saPath, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var text = File.ReadAllText(file);
                        var d = JsonConvert.DeserializeObject<DialogueData>(text);
                        if (d == null) continue;
                        _data.Add(d);
                        _dict[d.Id] = d;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DialogueDataManager] 解析 Dialogue json 文件 {file} 失败：{e}");
                    }
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
