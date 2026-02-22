# CardPanel 卡牌栏系统使用文档

## 📋 功能特性

### 1. 核心功能
- ✅ 显示/隐藏动画（支持 DOTween）
- ✅ 卡牌添加/移除动画
- ✅ 卡牌悬停效果（缩放+位移）
- ✅ 自动布局和位置刷新
- ✅ 批量清空动画

### 2. 动画效果
- **面板动画**: 滑入/滑出 + 淡入淡出
- **卡牌添加**: 从下方飞入 + 淡入
- **卡牌移除**: 缩放消失 + 淡出
- **悬停效果**: 卡牌放大 + 向上浮动
- **点击效果**: 按压动画

---

## 🛠️ 快速开始

### Step 1: 创建卡牌栏 UI

1. 在 Hierarchy 中创建 UI Canvas
2. 创建一个空的 GameObject，命名为 `CardPanel`
3. 添加组件：
   - `RectTransform` (自动添加)
   - `CardPanel` 脚本
   - `CanvasGroup` (可选，脚本会自动添加)
   - `HorizontalLayoutGroup` 或 `GridLayoutGroup` (用于自动排列卡牌)

### Step 2: 配置 CardPanel 组件

在 Inspector 中设置：

```
CardPanel
├─ [✓] Visible                    // 初始可见性
├─ Panel Rect                      // 自动获取或手动设置
├─ Canvas Group                    // 自动获取或手动设置
├─ Card Container                  // 卡牌容器（通常是自身）
│
├─ Show Duration: 0.5              // 显示动画时长
├─ Hide Duration: 0.3              // 隐藏动画时长
├─ Show Ease: OutBack              // 显示缓动
├─ Hide Ease: InBack               // 隐藏缓动
│
├─ Card Add Duration: 0.3          // 卡牌添加动画时长
├─ Card Remove Duration: 0.2       // 卡牌移除动画时长
├─ Card Spacing: 10                // 卡牌间距
├─ Card Add Start Offset: (0,-100,0) // 添加起始偏移
│
├─ [✓] Enable Hover Effect         // 启用悬停
├─ Hover Scale: 1.1                // 悬停缩放
├─ Hover Duration: 0.2             // 悬停动画时长
└─ Hover Offset: (0,20,0)          // 悬停偏移
```

### Step 3: 创建卡牌预制体

1. 创建卡牌 UI（Image + Text 等）
2. 添加组件：
   - `CardDisplay` (显示卡牌信息)
   - `CardObject` (处理交互)
   - `CanvasGroup` (控制透明度)
3. 保存为 Prefab

---

## 📖 API 使用

### 显示/隐藏

```csharp
// 显示卡牌栏（带动画）
cardPanel.Show();

// 隐藏卡牌栏（带动画）
cardPanel.Hide();

// 无动画显示/隐藏
cardPanel.Show(animated: false);
cardPanel.Hide(animated: false);

// 切换显示状态
cardPanel.Toggle();
```

### 卡牌管理

```csharp
// 添加卡牌
cardPanel.AddCard(cardPrefab, animated: true);

// 移除指定卡牌对象
cardPanel.RemoveCard(cardGameObject, animated: true);

// 移除指定索引的卡牌
cardPanel.RemoveCardAtIndex(0, animated: true);

// 清空所有卡牌
cardPanel.ClearCards(animated: true);

// 获取卡牌数量
int count = cardPanel.CardCount;

// 获取所有卡牌对象
List<GameObject> cards = cardPanel.GetCards();

// 检查是否可见
bool visible = cardPanel.IsVisible;
```

---

## 🎨 CardObject 卡牌对象

### 事件订阅

```csharp
CardObject cardObj = cardGameObject.GetComponent<CardObject>();

// 点击事件
cardObj.OnCardClicked += (card) => {
    Debug.Log($"点击了卡牌: {card.cardData.cardName}");
};

// 悬停进入
cardObj.OnCardHoverEnter += (card) => {
    Debug.Log("鼠标进入卡牌");
};

// 悬停离开
cardObj.OnCardHoverExit += (card) => {
    Debug.Log("鼠标离开卡牌");
};
```

### 控制方法

```csharp
// 设置选中状态
cardObj.SetSelected(true);

// 设置可交互性
cardObj.SetInteractable(false);

// 播放抖动动画（例如无法使用时）
cardObj.PlayShakeAnimation();

// 播放进入动画
cardObj.PlayEnterAnimation(delay: 0.1f);

// 播放退出动画
cardObj.PlayExitAnimation(() => {
    Debug.Log("动画完成");
});
```

---

## 🔗 与战斗系统集成

### 示例：从 CombatManager 添加手牌到卡牌栏

```csharp
public class BattleUIManager : MonoBehaviour
{
    [SerializeField] private CardPanel playerHandPanel;
    [SerializeField] private GameObject cardPrefab;
    
    public void ShowPlayerHand()
    {
        // 获取玩家手牌
        var playerHand = CombatManager.Instance.GetPlayerHandList();
        
        // 清空当前显示
        playerHandPanel.ClearCards(false);
        
        // 添加手牌到卡牌栏
        foreach (var card in playerHand)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            
            // 设置卡牌显示
            var display = cardObj.GetComponent<CardDisplay>();
            display.card = card;
            display.UpdateCardDisplay();
            
            // 设置卡牌对象
            var cardObject = cardObj.GetComponent<CardObject>();
            cardObject.cardData = card;
            cardObject.OnCardClicked += OnCardClicked;
            
            playerHandPanel.AddCard(cardObj, true);
        }
        
        // 显示卡牌栏
        playerHandPanel.Show();
    }
    
    private void OnCardClicked(CardObject cardObj)
    {
        // 处理卡牌点击（例如出牌）
        if (cardObj.cardData is MonsterCard)
        {
            CombatManager.Instance.SummonRequest(
                CardOwner.PlayerA, 
                cardObj.gameObject
            );
        }
    }
}
```

---

## 🎯 使用场景

### 场景1: 玩家手牌栏
```csharp
// 回合开始时显示手牌
void OnTurnStart()
{
    playerHandPanel.Show();
    RefreshPlayerHand();
}

// 回合结束时隐藏手牌
void OnTurnEnd()
{
    playerHandPanel.Hide();
}
```

### 场景2: 卡牌选择界面
```csharp
// 显示可选卡牌
void ShowCardSelection(List<Card> cards)
{
    selectionPanel.ClearCards();
    
    foreach (var card in cards)
    {
        GameObject cardObj = CreateCardObject(card);
        var cardComponent = cardObj.GetComponent<CardObject>();
        cardComponent.OnCardClicked += OnCardSelected;
        
        selectionPanel.AddCard(cardObj);
    }
    
    selectionPanel.Show();
}
```

### 场景3: 临时抽取区域
```csharp
// 显示从对手抽取的卡牌
IEnumerator ShowDrawnCards(List<Card> drawnCards)
{
    tempPanel.Show();
    
    foreach (var card in drawnCards)
    {
        GameObject cardObj = CreateCardObject(card);
        tempPanel.AddCard(cardObj, true);
        yield return new WaitForSeconds(0.2f);
    }
    
    yield return new WaitForSeconds(1f);
    tempPanel.Hide();
}
```

---

## ⚙️ 高级定制

### 自定义隐藏位置

```csharp
// 在 CardPanel 中修改 SetupPositions()
private void SetupPositions()
{
    visiblePosition = panelRect.anchoredPosition;
    
    // 向左隐藏
    hiddenPosition = visiblePosition + new Vector3(-panelRect.rect.width - 50, 0, 0);
    
    // 向右隐藏
    // hiddenPosition = visiblePosition + new Vector3(panelRect.rect.width + 50, 0, 0);
    
    // 向上隐藏
    // hiddenPosition = visiblePosition + new Vector3(0, panelRect.rect.height + 50, 0);
}
```

### 自定义缓动曲线

```csharp
// 使用自定义 AnimationCurve
[SerializeField] private AnimationCurve customEase;

// 在动画中使用
panelRect.DOAnchorPos(visiblePosition, showDuration).SetEase(customEase);
```

---

## 🐛 常见问题

### Q: 卡牌栏不显示？
A: 检查以下项：
1. Canvas 是否激活
2. `visible` 是否为 true
3. `panelRect` 和 `canvasGroup` 是否正确引用
4. 层级是否被其他 UI 遮挡

### Q: 悬停效果不生效？
A: 检查：
1. `enableHoverEffect` 是否勾选
2. 卡牌是否有 `EventTrigger` 组件
3. GraphicRaycaster 是否存在

### Q: 卡牌位置混乱？
A: 确保：
1. `cardContainer` 有 LayoutGroup 组件
2. 调用了 `RefreshCardPositions()`
3. RectTransform 的 Pivot 和 Anchor 设置正确

---

## 📝 TODO / 扩展建议

- [ ] 支持拖拽卡牌
- [ ] 支持卡牌重新排序
- [ ] 添加卡牌筛选功能
- [ ] 支持多页显示
- [ ] 添加音效支持
- [ ] 性能优化（对象池）

---

## 📦 依赖

- **DOTween** (v1.2+)
- **Unity UI** (TextMeshPro 推荐)

---

## 📄 示例快捷键（CardPanelExample）

| 按键 | 功能 |
|------|------|
| `Space` | 切换显示/隐藏 |
| `A` | 添加卡牌 |
| `R` | 移除最后一张卡牌 |
| `C` | 清空所有卡牌 |
