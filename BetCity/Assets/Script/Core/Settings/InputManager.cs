using BetCity.Core.Tools;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.Core.Settings
{
    //输入控制器
    public class InputManager : MonoSingleton<InputManager>
    {
        // 序列化字典：作为所有按键的统一配置源
        [field: SerializeField]
        private Dictionary<string, KeyCode> keyCodes;
        // 左移按键（只读，内部关联字典，无性能损耗）
        private KeyCode MoveLeftKey => keyCodes["MoveLeft"];
        // 右移按键
        private KeyCode MoveRightKey => keyCodes["MoveRight"];
        // 退出按键
        private KeyCode QuitKey => keyCodes["Quit"];

        // 输入状态（供外部访问）
        public bool IsMoveLeft { get; private set; }   // 左移状态
        public bool IsMoveRight { get; private set; }  // 右移状态
        public bool IsQuit { get; private set; }       // 退出触发状态
        protected override void Awake()
        {
            base.Awake();
            InitKeyCodeDictionary();
        }
        /// <summary>
        /// 初始化按键字典，确保默认键值对存在
        /// </summary>
        private void InitKeyCodeDictionary()
        {
            keyCodes = new Dictionary<string, KeyCode>()
            {
                { "MoveLeft", KeyCode.A},
                {"MoveRight", KeyCode.D},
                {"Quit", KeyCode.Escape}
            };
        }
        private void Update()
        {
            UpdateInput();
        }
        private void UpdateInput()
        {
            IsMoveLeft = Input.GetKey(MoveLeftKey);
            IsMoveRight = Input.GetKey(MoveRightKey);
            IsQuit = Input.GetKeyDown(QuitKey);
        }
        /// <summary>
        /// 外部查询接口
        /// </summary>
        /// <param name="keyName">按键名称</param>
        /// <returns>对应的KeyCode，不存在则返回None</returns>
        public KeyCode GetKeyCode(string keyName)
        {
            if (keyCodes == null || !keyCodes.ContainsKey(keyName))
            {
                Debug.LogWarning($"[InputManager]按键名称[{keyName}]不存在！");
                return KeyCode.None;
            }
            return keyCodes[keyName];
        }
        /// <summary>
        /// 外部修改接口
        /// </summary>
        /// <param name="keyName">按键名称（对应常量：MoveLeft/MoveRight/Quit）</param>
        /// <param name="newKeyCode">新的按键值</param>
        /// <returns>是否修改成功</returns>
        public bool SetKeyCode(string keyName, KeyCode newKeyCode)
        {
            if (keyCodes==null)
            {
                Debug.LogError("[InputManager]案件字典未创建");
                return false;
            }

            // 存在则更新
            if (keyCodes.ContainsKey(keyName))
            {
                keyCodes[keyName] = newKeyCode;
            }
            else
            {
                Debug.LogError("[InputManager]未找到按键名称对应的按键");
                return false;
            }

            Debug.Log($"[InputManager]成功修改按键 [{keyName}] 为 [{newKeyCode}]");
            return true;
        }
    }
}
