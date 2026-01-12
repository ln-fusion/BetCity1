using System.Collections.Generic;
using UnityEngine;
using BetCity.UI.Core;
using System.Linq;

/// <summary>
/// UI管理器核心类（单例模式）
/// 核心职责：
/// 1. 统一管理所有UI的加载、显示、隐藏、销毁
/// 2. 控制UI层级，避免层级冲突
/// 3. 批量管理UI动画（暂停/恢复/终止）
/// 4. 缓存UI实例，避免重复加载销毁
/// 使用规范：
/// - 全局唯一实例，通过UIManager.Instance调用
/// - 所有UI操作必须通过此类，禁止直接调用UIBase的Show/Hide
/// - UI预制体需放在Resources/UI目录下
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 单例实例（全局唯一）
    /// <summary>
    /// UIManager单例实例
    /// 访问方式：UIManager.Instance.XXX()
    /// </summary>
    public static UIManager Instance { get; private set; }

    /// <summary>
    /// UI根节点：所有UI都会挂载到此节点下，统一管理
    /// 建议在场景中创建空物体命名为UIRoot，拖入此字段
    /// </summary>
    [Header("UI根节点配置")]
    [Tooltip("所有UI的父节点，用于统一管理UI层级")]
    [SerializeField] private Transform uiRoot;

    /// <summary>
    /// 默认UI层级起始值（弹窗/菜单从此值开始递增）
    /// 避免不同类型UI层级冲突
    /// </summary>
    [Header("UI层级配置")]
    [Tooltip("弹窗默认起始层级（数值越大越靠前）")]
    [SerializeField] private int defaultPopupOrder = 100;
    [Tooltip("菜单默认起始层级")]
    [SerializeField] private int defaultMenuOrder = 50;
    [Tooltip("HUD默认起始层级")]
    [SerializeField] private int defaultHUDOrder = 10;

    /// <summary>
    /// UI实例缓存池：键=UI名称，值=UI实例
    /// 用于缓存已加载的UI，避免重复加载
    /// </summary>
    private Dictionary<string, UIBase> uiInstanceCache = new Dictionary<string, UIBase>();

    /// <summary>
    /// 当前显示的弹窗栈：用于弹窗层级递增、返回关闭等逻辑
    /// </summary>
    private Stack<UIBase> popupStack = new Stack<UIBase>();

    /// <summary>
    /// 层级计数器：确保新弹窗层级始终高于已有弹窗
    /// </summary>
    private int currentPopupOrder;
    #endregion

    #region 生命周期（单例初始化）
    private void Awake()
    {
        // 单例模式：确保全局唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景保留

        // 初始化层级计数器
        currentPopupOrder = defaultPopupOrder;

        // 初始化UI根节点（如果未配置，自动创建）
        if (uiRoot == null)
        {
            GameObject uiRootObj = new GameObject("UIRoot");
            uiRoot = uiRootObj.transform;
            DontDestroyOnLoad(uiRootObj);
            Debug.LogWarning($"【UIManager】未配置UI根节点，已自动创建UIRoot", this);
        }
    }

    private void OnDestroy()
    {
        // 销毁时清空缓存，避免内存泄漏
        ClearAllUICache();
        popupStack.Clear();
        uiInstanceCache.Clear();
    }
    #endregion

    #region 核心API：显示UI（对外暴露的主接口）
    /// <summary>
    /// 显示指定名称的UI（通用版，适配所有UI类型）
    /// </summary>
    /// <param name="uiName">UI预制体名称（需和Resources/UI目录下的预制体名一致）</param>
    /// <param name="uiType">UI类型（弹窗/菜单/HUD）</param>
    /// <param name="withAnimation">是否启用动画</param>
    /// <param name="isCache">是否缓存UI实例（true=隐藏不销毁，false=隐藏即销毁）</param>
    public void ShowUI(string uiName, UIType uiType = UIType.Popup, bool withAnimation = true, bool isCache = true)
    {
        // 安全校验：UI名称为空直接返回
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError($"【UIManager】显示UI失败：UI名称为空", this);
            return;
        }

        // 检查缓存：如果已加载，直接显示
        if (uiInstanceCache.TryGetValue(uiName, out UIBase cachedUI))
        {
            ShowCachedUI(cachedUI, uiType, withAnimation);
            return;
        }

        // 未缓存：加载UI预制体（Resources加载）
        LoadUIByResources(uiName, uiType, withAnimation, isCache);
    }

    /// <summary>
    /// 隐藏指定名称的UI
    /// </summary>
    /// <param name="uiName">UI预制体名称</param>
    /// <param name="withAnimation">是否启用动画</param>
    /// <param name="isDestroy">是否销毁实例（true=销毁，false=保留缓存）</param>
    public void HideUI(string uiName, bool withAnimation = true, bool isDestroy = false)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError($"【UIManager】隐藏UI失败：UI名称为空", this);
            return;
        }

        if (!uiInstanceCache.TryGetValue(uiName, out UIBase ui))
        {
            Debug.LogWarning($"【UIManager】隐藏UI失败：{uiName} 未加载/不存在", this);
            return;
        }

        // 执行隐藏逻辑
        HideTargetUI(ui, withAnimation, isDestroy);
    }

    /// <summary>
    /// 隐藏当前最上层的弹窗（适配弹窗栈逻辑）
    /// </summary>
    /// <param name="withAnimation">是否启用动画</param>
    public void HideTopPopup(bool withAnimation = true)
    {
        if (popupStack.Count == 0)
        {
            Debug.LogWarning($"【UIManager】隐藏弹窗失败：弹窗栈为空", this);
            return;
        }

        UIBase topPopup = popupStack.Pop();
        HideTargetUI(topPopup, withAnimation, false);

        // 更新弹窗层级计数器（如果还有弹窗，重置为最后一个弹窗的层级+1）
        if (popupStack.Count > 0)
        {
            currentPopupOrder = popupStack.Peek().GetSortingOrder() + 1;
        }
        else
        {
            currentPopupOrder = defaultPopupOrder;
        }
    }

    /// <summary>
    /// 隐藏所有UI
    /// </summary>
    /// <param name="withAnimation">是否启用动画</param>
    /// <param name="isDestroyAll">是否销毁所有实例（true=清空缓存，false=保留缓存）</param>
    public void HideAllUI(bool withAnimation = true, bool isDestroyAll = false)
    {
        if (uiInstanceCache.Count == 0) return;

        // 遍历所有缓存的UI，执行隐藏逻辑
        foreach (var kvp in uiInstanceCache)
        {
            HideTargetUI(kvp.Value, withAnimation, isDestroyAll);
        }

        // 如果销毁所有，清空缓存和弹窗栈
        if (isDestroyAll)
        {
            ClearAllUICache();
            popupStack.Clear();
            currentPopupOrder = defaultPopupOrder;
        }
    }
    #endregion

    #region 动画管理：批量控制UI动画
    /// <summary>
    /// 暂停指定UI的动画
    /// </summary>
    /// <param name="uiName">UI名称</param>
    public void PauseUIAnimation(string uiName)
    {
        if (uiInstanceCache.TryGetValue(uiName, out UIBase ui))
        {
            ui.PauseAnimation();
        }
        else
        {
            Debug.LogWarning($"【UIManager】暂停动画失败：{uiName} 未加载", this);
        }
    }

    /// <summary>
    /// 恢复指定UI的动画
    /// </summary>
    /// <param name="uiName">UI名称</param>
    public void ResumeUIAnimation(string uiName)
    {
        if (uiInstanceCache.TryGetValue(uiName, out UIBase ui))
        {
            ui.ResumeAnimation();
        }
        else
        {
            Debug.LogWarning($"【UIManager】恢复动画失败：{uiName} 未加载", this);
        }
    }

    /// <summary>
    /// 暂停所有UI的动画（如游戏暂停时）
    /// </summary>
    public void PauseAllUIAnimation()
    {
        foreach (var kvp in uiInstanceCache)
        {
            kvp.Value.PauseAnimation();
        }
    }

    /// <summary>
    /// 恢复所有UI的动画（如游戏恢复时）
    /// </summary>
    public void ResumeAllUIAnimation()
    {
        foreach (var kvp in uiInstanceCache)
        {
            kvp.Value.ResumeAnimation();
        }
    }
    #endregion

    #region 内部辅助方法（私有，禁止外部调用）
    /// <summary>
    /// 加载UI预制体（Resources方式）
    /// </summary>
    /// <param name="uiName">UI名称</param>
    /// <param name="uiType">UI类型</param>
    /// <param name="withAnimation">是否启用动画</param>
    /// <param name="isCache">是否缓存</param>
    private void LoadUIByResources(string uiName, UIType uiType, bool withAnimation, bool isCache)
    {
        // 加载预制体（路径：Resources/UI/XXX.prefab）
        GameObject uiPrefab = Resources.Load<GameObject>($"UI/{uiName}");
        if (uiPrefab == null)
        {
            Debug.LogError($"【UIManager】加载UI失败：Resources/UI/{uiName} 预制体不存在", this);
            return;
        }

        // 实例化UI
        GameObject uiObj = Instantiate(uiPrefab, uiRoot);
        uiObj.name = uiName; // 重命名实例，避免后缀(Clone)

        // 获取UIBase组件
        UIBase uiBase = uiObj.GetComponent<UIBase>();
        if (uiBase == null)
        {
            Debug.LogError($"【UIManager】{uiName} 预制体未挂载UIBase组件", this);
            Destroy(uiObj);
            return;
        }

        // 缓存UI实例（如果开启缓存）
        if (isCache && !uiInstanceCache.ContainsKey(uiName))
        {
            uiInstanceCache.Add(uiName, uiBase);
        }

        // 设置UI层级
        SetUILayer(uiBase, uiType);

        // 显示UI
        uiBase.Show(withAnimation);

        // 如果是弹窗，加入弹窗栈
        if (uiType == UIType.Popup)
        {
            popupStack.Push(uiBase);
            currentPopupOrder++; // 层级递增，确保新弹窗在最上层
        }

        Debug.Log($"【UIManager】成功加载并显示UI：{uiName}", this);
    }

    /// <summary>
    /// 显示缓存中的UI
    /// </summary>
    /// <param name="cachedUI">缓存的UI实例</param>
    /// <param name="uiType">UI类型</param>
    /// <param name="withAnimation">是否启用动画</param>
    private void ShowCachedUI(UIBase cachedUI, UIType uiType, bool withAnimation)
    {
        // 设置层级（防止层级错乱）
        SetUILayer(cachedUI, uiType);

        // 显示UI
        cachedUI.Show(withAnimation);

        // 如果是弹窗，加入弹窗栈
        if (uiType == UIType.Popup && !popupStack.Contains(cachedUI))
        {
            popupStack.Push(cachedUI);
            currentPopupOrder++;
        }

        Debug.Log($"【UIManager】成功显示缓存UI：{cachedUI.gameObject.name}", this);
    }

    /// <summary>
    /// 隐藏指定UI实例
    /// </summary>
    /// <param name="ui">UI实例</param>
    /// <param name="withAnimation">是否启用动画</param>
    /// <param name="isDestroy">是否销毁</param>
    private void HideTargetUI(UIBase ui, bool withAnimation, bool isDestroy)
    {
        // 1. 执行隐藏逻辑（调用UIBase的Hide）
        ui.Hide(withAnimation);

        // 2. 如果是弹窗，从栈中移除（修复Stack.Remove的依赖）
        if (popupStack.Contains(ui))
        {
            // 注意：Stack<T>本身没有Remove方法，这是Linq的扩展方法，必须加using System.Linq;
            popupStack = new Stack<UIBase>(popupStack.Where(item => item != ui));

            // 3. 更新层级计数器（逻辑修正：确保栈顶弹窗是最上层）
            if (popupStack.Count > 0)
            {
                // 当前弹窗层级 = 栈顶弹窗的层级 + 1（新弹窗会在栈顶之上）
                currentPopupOrder = popupStack.Peek().GetSortingOrder() + 1;
            }
            else
            {
                // 弹窗栈为空，重置为默认层级
                currentPopupOrder = defaultPopupOrder;
            }
        }

        // 4. 如果需要销毁UI，从缓存中移除并销毁实例（修复Dictionary.Remove）
        if (isDestroy)
        {
            string uiName = ui.gameObject.name;
            // 正确调用Dictionary.Remove（只传Key）
            uiInstanceCache.Remove(uiName);
            Destroy(ui.gameObject);
            Debug.Log($"【UIManager】销毁UI：{uiName}", this);
        }
        else
        {
            Debug.Log($"【UIManager】隐藏UI：{ui.gameObject.name}", this);
        }
    }

    /// <summary>
    /// 设置UI层级（根据类型自动分配）
    /// </summary>
    /// <param name="ui">UI实例</param>
    /// <param name="uiType">UI类型</param>
    private void SetUILayer(UIBase ui, UIType uiType)
    {
        switch (uiType)
        {
            case UIType.Popup:
                ui.SetSortingOrder(currentPopupOrder);
                break;
            case UIType.Menu:
                ui.SetSortingOrder(defaultMenuOrder);
                break;
            case UIType.HUD:
                ui.SetSortingOrder(defaultHUDOrder);
                break;
            default:
                ui.SetSortingOrder(defaultPopupOrder);
                break;
        }
    }

    /// <summary>
    /// 清空所有UI缓存并销毁实例
    /// </summary>
    private void ClearAllUICache()
    {
        foreach (var kvp in uiInstanceCache)
        {
            Destroy(kvp.Value.gameObject);
        }
        uiInstanceCache.Clear();
        Debug.Log($"【UIManager】清空所有UI缓存", this);
    }
    #endregion

    #region 辅助工具方法（对外暴露）
    /// <summary>
    /// 检查指定UI是否正在显示
    /// </summary>
    /// <param name="uiName">UI名称</param>
    /// <returns>是否显示</returns>
    public bool IsUIShowing(string uiName)
    {
        if (uiInstanceCache.TryGetValue(uiName, out UIBase ui))
        {
            return ui.gameObject.activeSelf;
        }
        return false;
    }

    /// <summary>
    /// 获取指定UI的实例（慎用，仅特殊场景使用）
    /// 建议：优先通过UIManager的API操作，避免直接操作UI实例
    /// </summary>
    /// <param name="uiName">UI名称</param>
    /// <returns>UI实例</returns>
    public UIBase GetUIInstance(string uiName)
    {
        if (uiInstanceCache.TryGetValue(uiName, out UIBase ui))
        {
            return ui;
        }
        Debug.LogWarning($"【UIManager】{uiName} 实例不存在", this);
        return null;
    }
    #endregion
}

/// <summary>
/// UI类型枚举 暂定 后续添加
/// </summary>
public enum UIType
{
    /// <summary>
    /// 弹窗（如登录弹窗、提示弹窗）
    /// </summary>
    Popup,
    /// <summary>
    /// 菜单（如主菜单、设置菜单）
    /// </summary>
    Menu,
    /// <summary>
    /// HUD（如血条、金币显示、常驻UI）
    /// </summary>
    HUD
}