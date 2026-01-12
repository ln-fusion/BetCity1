using UnityEngine;
using DG.Tweening;  // 引入DOTween动画库（需先在Unity Asset Store下载导入）

namespace BetCity.UI.Core
{
    /// <summary>
    /// 所有UI组件的基础基类
    /// 核心职责：
    /// 1. 封装UI通用生命周期（显示/隐藏）、动画逻辑、层级控制
    /// 2. 提供统一的初始化、动画控制接口，子类只需重写扩展逻辑
    /// 3. 内置防重复触发、空引用校验等鲁棒性逻辑，避免常见UI Bug
    /// 使用规范：
    /// - 所有UI预制体需挂载此类（或其子类）
    /// - 子类通过重写虚属性/方法实现个性化逻辑，禁止修改基类核心代码
    /// - UIManager只需调用Show/Hide/SetSortingOrder，无需关心内部动画细节
    /// </summary>
    [RequireComponent(typeof(Canvas))]       // 强制依赖Canvas组件（控制UI层级）
    [RequireComponent(typeof(CanvasGroup))] // 强制依赖CanvasGroup组件（控制透明度/交互）
    public class UIBase : MonoBehaviour
    {
        #region 基础配置字段（Inspector面板可配置）
        /// <summary>
        /// UI默认显示层级（数值越大，UI显示越靠前）
        /// 可被UIManager动态修改，适用于弹窗/菜单等需要层级排序的场景
        /// </summary>
        [Header("UI基础配置")]
        [Tooltip("UI默认显示层级（数值越大越靠前，可被UIManager动态修改）")]
        [SerializeField] protected int defaultSortingOrder = 10;
        #endregion

        #region 核心组件与状态标记（子类可直接使用，无需重复获取）
        /// <summary>
        /// Canvas组件：控制UI的显示层级
        /// Awake中自动获取，无需手动赋值
        /// </summary>
        protected Canvas canvas;

        /// <summary>
        /// CanvasGroup组件：控制UI透明度、交互性、射线阻挡
        /// 核心用途：淡入淡出动画、防止未显示的UI被点击
        /// </summary>
        protected CanvasGroup canvasGroup;

        /// <summary>
        /// 当前播放的动画对象：用于控制动画暂停/终止/恢复
        /// 避免多个动画叠加导致的UI异常
        /// </summary>
        protected Tween currentTween;

        /// <summary>
        /// 动画播放状态标记：防止快速点击导致的动画重复触发
        /// 示例：连续点击显示/隐藏按钮，只会执行一次动画
        /// </summary>
        protected bool isPlayingAnimation = false;

        /// <summary>
        /// 初始化完成标记：确保Init方法只执行一次
        /// 避免重复注册事件、初始化数据等问题
        /// </summary>
        private bool isInited = false;
        #endregion

        #region 动画参数（子类可重写的虚属性，自定义动画效果）
        /// <summary>
        /// 淡入/淡出动画时长（单位：秒）
        /// 子类重写此属性即可自定义动画时长，无需修改基类逻辑
        /// 示例：弹窗用0.5秒，提示框用0.2秒
        /// </summary>
        protected virtual float FadeDuration => 0.3f;

        /// <summary>
        /// 动画缓动类型（控制动画的运动曲线）
        /// 子类重写此属性自定义缓动效果，DOTween支持多种预设（如OutBack=回弹、InCubic=先慢后快）
        /// 参考文档：https://dotween.demigiant.com/documentation.php#easeTypes
        /// </summary>
        protected virtual Ease AnimationEase => Ease.Linear;

        /// <summary>
        /// 是否允许子类完全覆盖基类默认动画逻辑
        /// 设为true时，基类的淡入淡出动画会失效，子类需在OnShowBeforeAnimation/OnHideBeforeAnimation中实现自定义动画
        /// 适用场景：需要缩放+位移+淡入的复杂弹窗动画
        /// </summary>
        protected virtual bool AllowOverrideAnimation => false;
        #endregion

        #region Unity内置生命周期（仅做基础初始化，禁止写业务逻辑）
        /// <summary>
        /// 组件唤醒时执行（Unity内置方法）
        /// 核心逻辑：
        /// 1. 自动获取Canvas/CanvasGroup组件
        /// 2. 设置UI初始状态（透明+禁用交互），避免未显示时被点击/可见
        /// 3. 初始化默认显示层级
        /// 注意：禁止在此方法中写业务逻辑，业务初始化请重写Init()
        /// </summary>
        protected virtual void Awake()
        {
            // 自动获取依赖组件，无需手动拖入Inspector
            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();

            // 初始状态配置：
            // alpha=0 → 透明；interactable=false → 禁用交互；blocksRaycasts=false → 不阻挡射线
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // 设置UI默认显示层级
            SetSortingOrder(defaultSortingOrder);
        }

        /// <summary>
        /// 组件销毁时执行（Unity内置方法）
        /// 核心逻辑：清理动画对象，防止内存泄漏
        /// 注意：所有动画相关引用需在此清空
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 终止所有未完成的动画
            KillAnimation();
            // 清空动画引用，避免空指针/内存泄漏
            currentTween = null;
        }
        #endregion

        #region 初始化方法（子类重写实现业务初始化，仅执行一次）
        /// <summary>
        /// UI初始化方法（对外暴露，子类可重写）
        /// 核心特性：
        /// 1. 内置重复执行校验，确保只初始化一次
        /// 2. 检查Canvas/CanvasGroup组件是否存在，避免空引用异常
        /// 子类使用规范：
        /// - 必须先调用base.Init()，再写自定义初始化逻辑
        /// - 仅用于注册事件、初始化数据等，禁止写显示/隐藏逻辑
        /// </summary>
        public virtual void Init()
        {
            // 防止重复初始化（比如多次调用Show()）
            if (isInited) return;

            // 组件依赖检查：缺失核心组件直接报错，方便定位问题
            if (canvas == null || canvasGroup == null)
            {
                Debug.LogError($"【UIBase】{gameObject.name} 缺少 Canvas 或 CanvasGroup 组件！请检查UI预制体", this);
                return;
            }

            // 标记初始化完成
            isInited = true;

            // 子类初始化示例（需重写此方法）：
            // 1. 注册按钮点击事件：closeBtn.onClick.AddListener(OnClose);
            // 2. 初始化数据：InitUIData();
            // 3. 加载资源：LoadUIAssets();
        }
        #endregion

        #region 显示/隐藏核心方法（对外暴露的主接口，UIManager调用）
        /// <summary>
        /// 显示UI（带动画效果）
        /// 核心逻辑：
        /// 1. 安全校验（组件存在性、状态合法性）
        /// 2. 延迟初始化（第一次显示时执行Init）
        /// 3. 执行淡入动画（子类可自定义动画参数/逻辑）
        /// 4. 动画完成后恢复交互
        /// 注意：禁止直接修改此方法，自定义显示逻辑请重写OnShowBeforeAnimation/OnShowComplete
        /// </summary>
        public virtual void Show()
        {
            // 安全校验1：CanvasGroup组件缺失直接报错
            if (canvasGroup == null)
            {
                Debug.LogError($"【UIBase】{gameObject.name} 缺少CanvasGroup组件，无法显示！", this);
                return;
            }

            // 安全校验2：动画中/已显示则直接返回，避免重复触发
            if (isPlayingAnimation || gameObject.activeSelf)
            {
                Debug.LogWarning($"【UIBase】{gameObject.name} 无法显示：动画播放中/UI已激活", this);
                return;
            }

            // 延迟初始化：第一次显示时执行Init（避免提前初始化导致依赖缺失）
            if (!isInited)
            {
                Init();
                // 初始化失败则终止显示逻辑
                if (!isInited) return;
            }

            // 激活UI对象（使其可见）
            gameObject.SetActive(true);
            // 标记动画开始，防止重复触发
            isPlayingAnimation = true;

            // 执行子类自定义的显示前逻辑（如：初始化数据、播放音效、自定义动画）
            OnShowBeforeAnimation();

            // 如果子类开启了“覆盖动画”，则跳过基类默认淡入动画
            if (AllowOverrideAnimation) return;

            // 淡入动画准备：重置透明度+禁用交互
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // 终止旧动画（防止多个动画叠加）
            currentTween?.Kill();
            // 执行淡入动画（使用子类自定义的时长/缓动类型）
            currentTween = canvasGroup.DOFade(1, FadeDuration)
                .SetEase(AnimationEase) // 应用自定义缓动效果
                .OnComplete(() =>
                {
                    // 动画完成：恢复交互+允许射线阻挡+标记动画结束
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    isPlayingAnimation = false;

                    // 执行子类自定义的显示完成逻辑
                    OnShowComplete();
                });
        }

        /// <summary>
        /// 显示UI（重载：控制是否启用动画）
        /// 适用场景：
        /// - withAnimation=true → 带动画（弹窗/菜单）
        /// - withAnimation=false → 无动画（HUD/常驻UI）
        /// </summary>
        /// <param name="withAnimation">是否启用淡入动画</param>
        public void Show(bool withAnimation)
        {
            if (!withAnimation)
            {
                // 安全校验：CanvasGroup缺失直接报错
                if (canvasGroup == null)
                {
                    Debug.LogError($"【UIBase】{gameObject.name} 缺少CanvasGroup组件，无法显示！", this);
                    return;
                }

                // 防重复激活：UI已显示则返回
                if (gameObject.activeSelf) return;

                // 延迟初始化
                if (!isInited)
                {
                    Init();
                    if (!isInited) return;
                }

                // 无动画直接显示：激活+全透明+恢复交互
                gameObject.SetActive(true);
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                // 执行显示完成逻辑
                OnShowComplete();
                return;
            }

            // 启用动画则调用默认Show方法
            Show();
        }

        /// <summary>
        /// 隐藏UI（带动画效果）
        /// 核心逻辑：
        /// 1. 安全校验（组件存在性、状态合法性）
        /// 2. 执行淡出动画（子类可自定义动画参数/逻辑）
        /// 3. 动画完成后隐藏UI+禁用交互
        /// 注意：禁止直接修改此方法，自定义隐藏逻辑请重写OnHideBeforeAnimation/OnHideComplete
        /// </summary>
        public virtual void Hide()
        {
            // 安全校验1：CanvasGroup组件缺失直接报错
            if (canvasGroup == null)
            {
                Debug.LogError($"【UIBase】{gameObject.name} 缺少CanvasGroup组件，无法隐藏！", this);
                return;
            }

            // 安全校验2：动画中/已隐藏则直接返回
            if (isPlayingAnimation || !gameObject.activeSelf)
            {
                Debug.LogWarning($"【UIBase】{gameObject.name} 无法隐藏：动画播放中/UI已隐藏", this);
                return;
            }

            // 标记动画开始
            isPlayingAnimation = true;

            // 执行子类自定义的隐藏前逻辑
            OnHideBeforeAnimation();

            // 如果子类开启了“覆盖动画”，则跳过基类默认淡出动画
            if (AllowOverrideAnimation) return;

            // 淡出动画准备：禁用交互
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // 终止旧动画
            currentTween?.Kill();
            // 执行淡出动画（使用子类自定义的时长/缓动类型）
            currentTween = canvasGroup.DOFade(0, FadeDuration)
                .SetEase(AnimationEase)
                .OnComplete(() =>
                {
                    // 动画完成：隐藏UI+标记动画结束
                    gameObject.SetActive(false);
                    isPlayingAnimation = false;

                    // 执行子类自定义的隐藏完成逻辑
                    OnHideComplete();
                });
        }

        /// <summary>
        /// 隐藏UI（重载：控制是否启用动画）
        /// 适用场景：
        /// - withAnimation=true → 带动画（弹窗/菜单）
        /// - withAnimation=false → 无动画（HUD/常驻UI）
        /// </summary>
        /// <param name="withAnimation">是否启用淡出动画</param>
        public void Hide(bool withAnimation)
        {
            if (!withAnimation)
            {
                // 安全校验：CanvasGroup缺失直接报错
                if (canvasGroup == null)
                {
                    Debug.LogError($"【UIBase】{gameObject.name} 缺少CanvasGroup组件，无法隐藏！", this);
                    return;
                }

                // 防重复隐藏：UI已隐藏则返回
                if (!gameObject.activeSelf) return;

                // 无动画直接隐藏：隐藏+透明+禁用交互
                gameObject.SetActive(false);
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                // 执行隐藏完成逻辑
                OnHideComplete();
                return;
            }

            // 启用动画则调用默认Hide方法
            Hide();
        }
        #endregion

        #region 层级控制（供UIManager调用，统一管理UI显示顺序）
        /// <summary>
        /// 设置UI显示层级
        /// 核心作用：控制多个UI的前后显示顺序，数值越大越靠前
        /// 示例：弹窗层级=100，菜单层级=50，HUD层级=10
        /// </summary>
        /// <param name="order">目标层级值</param>
        public virtual void SetSortingOrder(int order)
        {
            // 安全校验：Canvas组件缺失直接报错
            if (canvas == null)
            {
                Debug.LogError($"【UIBase】{gameObject.name} 缺少Canvas组件，无法设置层级！", this);
                return;
            }

            // 启用层级覆盖：避免受父节点Canvas的层级影响
            canvas.overrideSorting = true;
            // 设置目标层级
            canvas.sortingOrder = order;
        }

        /// <summary>
        /// 获取当前UI的显示层级
        /// 适用场景：UIManager排序时获取当前层级，避免层级冲突
        /// </summary>
        /// <returns>当前层级值</returns>
        public int GetSortingOrder()
        {
            // 有Canvas则返回当前层级，无则返回默认值
            return canvas != null ? canvas.sortingOrder : defaultSortingOrder;
        }
        #endregion

        #region 动画控制（供UIManager批量管理，如暂停所有UI动画）
        /// <summary>
        /// 暂停当前UI的所有动画
        /// 适用场景：游戏暂停、切场景时暂停动画
        /// </summary>
        public void PauseAnimation()
        {
            // 仅当动画正在播放时暂停
            if (currentTween != null && currentTween.IsPlaying())
            {
                currentTween.Pause();
            }
        }

        /// <summary>
        /// 恢复当前UI的动画
        /// 适用场景：游戏恢复、切场景完成后恢复动画
        /// </summary>
        public void ResumeAnimation()
        {
            // 仅当动画处于暂停状态时恢复
            if(currentTween != null && !currentTween.IsPlaying() && !currentTween.IsComplete())
            {
                currentTween.Play();
            }
        }

        /// <summary>
        /// 终止当前UI的所有动画（清理内存）
        /// 适用场景：UI隐藏/销毁时，防止动画残留导致内存泄漏
        /// </summary>
        public void KillAnimation()
        {
            // 仅当动画处于激活状态时终止
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                // 清空引用，避免空指针
                currentTween = null;
            }
        }
        #endregion

        #region 子类扩展钩子方法（空实现，子类按需重写）
        /// <summary>
        /// 显示动画执行前的钩子方法
        /// 子类重写场景：
        /// 1. 初始化UI数据（如：刷新弹窗内容）
        /// 2. 播放显示音效
        /// 3. 实现自定义动画（如：缩放、位移）
        /// 注意：必须先调用base.OnShowBeforeAnimation()
        /// </summary>
        protected virtual void OnShowBeforeAnimation() { }

        /// <summary>
        /// 显示完成后的钩子方法
        /// 子类重写场景：
        /// 1. 绑定数据（如：监听数据变化刷新UI）
        /// 2. 开启定时器（如：提示框自动关闭）
        /// 注意：必须先调用base.OnShowComplete()
        /// </summary>
        protected virtual void OnShowComplete() { }

        /// <summary>
        /// 隐藏动画执行前的钩子方法
        /// 子类重写场景：
        /// 1. 保存UI数据（如：保存用户输入的内容）
        /// 2. 停止音效/特效
        /// 3. 实现自定义隐藏动画
        /// 注意：必须先调用base.OnHideBeforeAnimation()
        /// </summary>
        protected virtual void OnHideBeforeAnimation() { }

        /// <summary>
        /// 隐藏完成后的钩子方法
        /// 子类重写场景：
        /// 1. 清理数据（如：清空输入框内容）
        /// 2. 关闭定时器/取消监听
        /// 注意：必须先调用base.OnHideComplete()
        /// </summary>
        protected virtual void OnHideComplete() { }
        #endregion
    }
}