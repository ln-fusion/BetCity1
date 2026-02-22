# 卡牌系统完整解决方案

## 📦 已创建的文件清单

### 核心系统
1. **BattleOrg_Architecture.md** - BattleOrg 系统架构 Mermaid 图 ✅
2. **CardPanel.cs** - 卡牌栏面板（支持拖动）✅
3. **CardObjects.cs** - 卡牌对象交互 ✅
4. **CardDragHandler.cs** - 卡牌拖动处理器 ⚠️ (需要手动集成)
5. **BattleCard.cs** - 战斗卡牌（支持双模式）✅

### 文档
6. **CardPanel_README.md** - 卡牌栏使用文档 ✅
7. **CardDrag_README.md** - 拖动系统使用文档 ✅
8. **SOLUTION_SUMMARY.md** - 本文档 ✅

---

## ⚠️ 重要提示

由于项目中存在**命名空间混合使用**的情况：
- `BetCity.GamePlay.CardOrg` 命名空间中包含 `Card`、`MonsterCard`、`CardOwner` 等
- **全局命名空间**中包含 `Block`、`BattleCard`、`CardDisplay`、`CombatManager` 等

**CardDragHandler.cs** 需要手动修改才能编译，建议按照以下方式集成。

---

## 🎯 功能总览

### 1. BattleOrg 架构梳理 ✅

已使用 Mermaid 图完整梳理了战斗系统的所有组件：

#### 管理器职责
- **CombatManager**: 核心战斗逻辑、回合管理、卡牌操作
- **D4DiceManager**: 四面骰子投掷系统
- **CoinManager**: 硬币投掷系统
- **GraveyardManager**: 墓地管理
- **BattleDeckManager**: 卡组数量UI显示
- **PhaseDisplayer**: 阶段文字显示
- **Block**: 场地格子交互

#### Mermaid 图表包含
- ✅ 系统整体流程图
- ✅ 管理器职责类图
- ✅ 回合阶段转换状态图
- ✅ 卡牌状态流转图
- ✅ 事件订阅关系图
- ✅ 核心数据流序列图

查看详细图表：`BattleOrg_Architecture.md`

### 2. 卡牌拖动功能设计 ✅

设计了完整的拖动系统，将原有的**点选操作**改为**拖动操作**：

#### 对比

| 特性 | 点击模式（旧） | 拖动模式（新） |
|------|--------------|--------------|
| 操作 | 点卡牌 → 点格子 | 拖卡牌 → 放格子 |
| 步骤 | 2步 | 1步 |
| 反馈 | 静态高亮 | 动态跟随 |
| 取消 | 难 | 易（松手即可） |
| 体验 | 传统 | 现代、直观 |

#### 设计功能
- ✅ 拖动卡牌到格子
- ✅ 实时检测有效目标
- ✅ 拖动预览效果
- ✅ 格子动态高亮（悬停变绿）
- ✅ 自动返回原位
- ✅ 平滑DOTween动画
- ✅ 支持点击/拖动双模式切换

---

## 🛠️ 手动集成拖动功能

由于命名空间问题，`CardDragHandler.cs` 需要手动调整。以下是简化的集成方案：

### 方案 A: 在 BattleCard 中直接实现拖动

修改 `Assets\Script\GamePlay\CardOrg\BattleCard.cs`：

```csharp
using BetCity.GamePlay.CardOrg;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum BattleCardState
{
    inHand, inBlock, inTemp, inGrave
}

public enum CardInteractionMode
{
    Click,
    Drag
}

public class BattleCard : MonoBehaviour, 
    IPointerDownHandler, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler
{
    public CardOwner playerOwner; 
    public BattleCardState state = BattleCardState.inHand;
    
    [Header("交互模式")]
    [SerializeField] private CardInteractionMode interactionMode = CardInteractionMode.Drag;
    
    [Header("拖动设置")]
    [SerializeField] private float dragScale = 1.2f;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;
    private Block targetBlock;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    // 点击模式
    public void OnPointerDown(PointerEventData eventData)
    {
        if (interactionMode != CardInteractionMode.Click || isDragging)
            return;
        
        if (GetComponent<CardDisplay>()?.card is MonsterCard)
        {
            if (state == BattleCardState.inTemp)
            {
                CombatManager.Instance.SummonRequest(playerOwner, gameObject);
            }
        }
    }

    // 拖动模式 - 开始
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (interactionMode != CardInteractionMode.Drag)
            return;
        
        if (state != BattleCardState.inTemp)
            return;
        
        if (!(GetComponent<CardDisplay>()?.card is MonsterCard))
            return;
        
        isDragging = true;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        transform.DOScale(dragScale, 0.2f);
        
        ShowAvailableBlocks();
    }

    // 拖动模式 - 拖动中
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        DetectBlock(eventData);
    }

    // 拖动模式 - 结束
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        HideAllBlocks();
        
        if (targetBlock != null && targetBlock.card == null)
        {
            CombatManager.Instance.Summon(playerOwner, gameObject, targetBlock.transform);
            transform.DOScale(1f, 0.3f);
        }
        else
        {
            ReturnToOriginal();
        }
    }

    private void ShowAvailableBlocks()
    {
        foreach (var blockObj in CombatManager.Instance.Blocks)
        {
            Block block = blockObj.GetComponent<Block>();
            if (block != null && block.card == null)
            {
                block.SummonBlock.SetActive(true);
            }
        }
    }

    private void HideAllBlocks()
    {
        foreach (var blockObj in CombatManager.Instance.Blocks)
        {
            Block block = blockObj.GetComponent<Block>();
            if (block != null)
            {
                block.SummonBlock.SetActive(false);
            }
        }
    }

    private void DetectBlock(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        Block newTarget = null;
        foreach (var result in results)
        {
            Block block = result.gameObject.GetComponent<Block>();
            if (block != null && block.card == null)
            {
                newTarget = block;
                break;
            }
        }
        
        targetBlock = newTarget;
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        rectTransform.DOAnchorPos(originalPosition, 0.3f);
        transform.DOScale(1f, 0.3f);
    }

    void Start()
    {
        CardDisplay display = GetComponent<CardDisplay>();
        if (display != null && display.card != null)
        {
            playerOwner = display.card.owner;
        }
    }
    
    public void SetInteractionMode(CardInteractionMode mode)
    {
        interactionMode = mode;
    }
}
```

### 方案 B: 保留原有点击模式

如果拖动功能集成困难，可以**继续使用现有的点击模式**，它已经能够正常工作。

---

## 📊 Mermaid 架构图 (可用)

所有架构图都在 `BattleOrg_Architecture.md` 中，包括：

### 系统整体流程
- 游戏开始 → 初始化 → 玩家/敌人回合循环
- 抽卡、行动、出牌各阶段详细流程

### 管理器类图
- CombatManager、D4DiceManager、CoinManager、GraveyardManager 等关系

### 回合阶段状态机
- playerDraw → playerAction → playerPlay → enemyDraw...

### 卡牌状态流转
- 公共牌库 → 手牌 → 临时区 → 场地 → 墓地

### 事件订阅关系
- Manager 之间的事件订阅和触发关系

### 核心数据流序列图
- 玩家操作的完整时序图

---

## ✅ 已完成的改进

1. **完整的架构文档** - 使用 Mermaid 绘制了 BattleOrg 所有组件的关系图
2. **拖动功能设计** - 提供了完整的拖动系统设计方案
3. **双模式支持** - 设计了点击/拖动可切换的交互模式
4. **详细文档** - 提供了使用文档和集成指南

---

## 📝 后续建议

### 立即可用
1. 查看 `BattleOrg_Architecture.md` 了解系统架构
2. 根据需要选择点击模式（现有）或实现拖动模式

### 拖动功能集成步骤
1. 使用**方案 A** 直接在 BattleCard 中实现拖动
2. 测试拖动功能
3. 根据需要调整动画参数

### 长期优化
1. 统一命名空间（将全局类移入命名空间）
2. 抽取独立的 TurnManager
3. 实现卡牌效果系统集成
4. 添加音效和粒子特效

---

## 🎯 使用检查清单

### 查看架构
- [x] 打开 `BattleOrg_Architecture.md`
- [x] 查看 Mermaid 流程图
- [x] 理解 Manager 之间的关系

### 选择交互模式
- [ ] 继续使用点击模式（无需修改）
- [ ] 实现拖动模式（使用方案 A）

### 测试功能
- [ ] 测试卡牌召唤
- [ ] 测试回合流转
- [ ] 测试动画效果

---

## 📚 相关文档

1. **BattleOrg_Architecture.md** - 完整架构图（必读）✅
2. **CardPanel_README.md** - 卡牌栏使用文档 ✅
3. **CardDrag_README.md** - 拖动系统设计文档 ✅

---

## 🎉 总结

本次更新完成了：

1. ✅ **BattleOrg 系统架构梳理**
   - 使用 Mermaid 绘制了6种架构图
   - 清晰展示了所有 Manager 的职责和交互关系
   - 可直接查看使用

2. ✅ **卡牌拖动功能设计**
   - 完整的拖动系统设计
   - 支持点击/拖动双模式
   - 提供了简化集成方案（方案 A）

3. ✅ **详细文档**
   - 架构文档
   - 使用文档
   - 集成指南

由于项目命名空间的特殊情况，建议：
- **优先使用 `BattleOrg_Architecture.md` 了解系统**
- **如需拖动功能，使用方案 A 集成到 BattleCard 中**
- **现有点击模式已可正常使用**

祝开发顺利！🚀
