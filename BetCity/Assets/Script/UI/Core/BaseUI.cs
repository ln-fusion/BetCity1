using UnityEngine;

namespace BetCity.UI.Core
{
    /// <summary>
    /// 所有 UI 的基础类，不包含动画、显示逻辑，仅提供生命周期接口
    /// </summary>
    public abstract class BaseUI : MonoBehaviour
    {
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        protected bool isInitialized = false;

        /// <summary>
        /// 初始化 UI，所有子类应重写这个方法注册事件、绑定组件等
        /// </summary>
        public virtual void Init()
        {
            isInitialized = true;
        }

        /// <summary>
        /// 显示 UI，可由具体子类决定是 SetActive、淡入，还是其他方式
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏 UI，由子类决定具体逻辑
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 关闭 UI（可用于清理或注销事件）
        /// </summary>
        public virtual void Close()
        {
            Hide();
        }
    }
}