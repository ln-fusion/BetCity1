using BetCity.Core.Tools;
using BetCity.UI.Core;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.UI.Core
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [Header("UI 预制体注册表")]
        public List<BaseUI> uiPrefabs;

        private Dictionary<string, BaseUI> uiInstances = new Dictionary<string, BaseUI>();

        protected override void Awake()
        {
            //提前实例化所有 UI(可选)
            foreach (var prefab in uiPrefabs)
            {
                CreateUIInstance(prefab);
            }
        }

        private void CreateUIInstance(BaseUI prefab)
        {
            string uiName = prefab.name;

            if (uiInstances.ContainsKey(uiName)) return;

            BaseUI ui = Instantiate(prefab, transform); // 挂在 UIManager 下面
            ui.gameObject.SetActive(false);
            ui.Init();

            uiInstances.Add(uiName, ui);
        }

        public void ShowUI(string uiName)
        {
            if (!uiInstances.ContainsKey(uiName))
            {
                Debug.LogWarning($"UIManager: UI {uiName} not found!");
                return;
            }

            uiInstances[uiName].Show();
        }

        public void HideUI(string uiName)
        {
            if (!uiInstances.ContainsKey(uiName)) return;

            uiInstances[uiName].Hide();
        }

        public void CloseUI(string uiName)
        {
            if (!uiInstances.ContainsKey(uiName)) return;

            uiInstances[uiName].Close();
        }

        public T GetUI<T>(string uiName) where T : BaseUI
        {
            if (uiInstances.TryGetValue(uiName, out BaseUI ui))
            {
                return ui as T;
            }
            return null;
        }
    }
}