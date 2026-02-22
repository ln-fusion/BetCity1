# BattleOrg 系统架构图

## 系统整体流程

```mermaid
flowchart TB
    Start([游戏开始]) --> CM[CombatManager]
    
    CM --> Init[初始化]
    Init --> ReadDeck[读取卡组]
    Init --> ShuffleDeck[洗牌]
    Init --> InitDraw[初始抽卡5张]
    
    Init --> PlayerTurn[玩家回合]
    
    subgraph PlayerTurn[玩家回合流程]
        PD[PlayerDraw 抽卡阶段]
        PA[PlayerAction 行动阶段]
        PP[PlayerPlay 出牌阶段]
        
        PD -->|投骰子| D4M[D4DiceManager]
        D4M -->|抽N张卡| DrawCards
        DrawCards --> PA
        
        PA -->|投硬币| CoinM[CoinManager]
        CoinM -->|正面2张/反面1张| DrawFromOpponent
        DrawFromOpponent -->|卡牌进入临时区| PP
        
        PP -->|拖动卡牌| DragCard
        DragCard -->|显示可用格子| Blocks
        Blocks -->|释放到目标格子| Summon
        Summon -->|卡牌进入场地| EndPlayerTurn
    end
    
    PlayerTurn --> EnemyTurn[敌人回合]
    
    subgraph EnemyTurn[敌人回合流程]
        ED[EnemyDraw 抽卡阶段]
        EA[EnemyAction 行动阶段]
        EP[EnemyPlay 出牌阶段]
        
        ED -->|自动投骰子| D4M2[D4DiceManager]
        D4M2 --> EA
        EA -->|自动投硬币| CoinM2[CoinManager]
        CoinM2 --> EP
        EP -->|AI出牌| EndEnemyTurn
    end
    
    EnemyTurn --> CheckWin{检查胜负}
    CheckWin -->|继续| PlayerTurn
    CheckWin -->|结束| End([游戏结束])
    
    Summon -.卡牌销毁.-> GraveyardM[GraveyardManager]
    
    style CM fill:#ff6b6b
    style D4M fill:#4ecdc4
    style CoinM fill:#4ecdc4
    style GraveyardM fill:#95e1d3
```

## 管理器职责划分

```mermaid
classDiagram
    class CombatManager {
        +GamePhase currentPhase
        +CardOwner currentTurnPlayer
        +List~Card~ publicDeck
        +List~Card~ playerHandList
        +List~Card~ enemyHandList
        +GameObject[] Blocks
        +D4DiceManager d4DiceManager
        +CoinManager coinManager
        +Transform temporaryBlock
        --
        +GameStart()
        +DrawCards(player, count)
        +SummonRequest(player, monster)
        +Summon(player, monster, block)
        +ResetDeck()
        +EndPlayerTurn()
        +EndEnemyTurn()
    }
    
    class D4DiceManager {
        +D4DiceState state
        +Sprite[] d4DiceFaces
        +float d4RollDuration
        +Action~int~ OnD4DiceRollFinished
        --
        +RollD4Dice()
        +SetInteractable(bool)
    }
    
    class CoinManager {
        +CoinState state
        +Sprite headsSprite
        +Sprite tailsSprite
        +float flipDuration
        +Action~CoinResult~ OnCoinFlipFinished
        --
        +FlipCoin()
        +SetInteractable(bool)
    }
    
    class GraveyardManager {
        +List~Card~ graveyardCards
        +GameObject graveyardPanel
        +Transform cardListContainer
        --
        +SendCardToGraveyard(card)
        +OpenGraveyardPanel()
        +CloseGraveyardPanel()
    }
    
    class BattleDeckManager {
        +TextMeshProUGUI deckCountText
        +string displayFormat
        --
        +UpdateDeckCountDisplay()
        +OnDeckCountChanged(count)
    }
    
    class PhaseDisplayer {
        +TextMeshProUGUI phaseText
        --
        +UpdateText()
    }
    
    class Block {
        +GameObject card
        +GameObject SummonBlock
        --
        +slotDataOnly
    }
    
    CombatManager --> D4DiceManager : 使用
    CombatManager --> CoinManager : 使用
    CombatManager --> GraveyardManager : 调用
    CombatManager --> Block : 管理
    CombatManager ..> BattleDeckManager : 触发事件
    CombatManager ..> PhaseDisplayer : 触发事件
```

## 回合阶段转换

```mermaid
stateDiagram-v2
    [*] --> begin
    
    begin --> playerDraw : 游戏开始
    
    playerDraw --> playerAction : 投骰子完成 & 抽卡完成
    playerAction --> playerPlay : 投硬币完成 & 抽对手卡完成
    playerPlay --> playerDecide : 出牌完成(待实现)
    playerDecide --> enemyDraw : 回合结束
    
    enemyDraw --> enemyAction : 自动投骰子 & 抽卡
    enemyAction --> enemyPlay : 自动投硬币 & 抽对手卡
    enemyPlay --> enemyDecide : AI出牌
    enemyDecide --> playerDraw : 回合结束
    
    playerDraw --> endPhase : 牌库耗尽
    enemyDraw --> endPhase : 牌库耗尽
    endPhase --> [*] : 游戏结束
```

## 卡牌状态流转

```mermaid
stateDiagram-v2
    [*] --> publicDeck : 洗牌
    
    publicDeck --> playerHand : DrawCards(PlayerA)
    publicDeck --> enemyHand : DrawCards(PlayerB)
    
    playerHand --> temporaryBlock : 投硬币抽取
    enemyHand --> temporaryBlock : 投硬币抽取
    
    temporaryBlock --> Block : Summon(Monster)
    playerHand --> Block : 直接出牌(待实现)
    
    Block --> Graveyard : 销毁/弃置
    playerHand --> Graveyard : 弃牌
    enemyHand --> Graveyard : 弃牌
    
    Graveyard --> publicDeck : ResetDeck重置
    
    note right of temporaryBlock
        只有临时区域的
        MonsterCard 可召唤
    end note
```

## 事件订阅关系

```mermaid
graph LR
    subgraph CombatManager
        CM[CombatManager]
        CMEvents["onDeckCountChanged<br/>phaseChangeEvent"]
    end
    
    subgraph D4DiceManager
        D4M[D4DiceManager]
        D4MEvents[OnD4DiceRollFinished]
    end
    
    subgraph CoinManager
        CoinM[CoinManager]
        CoinEvents[OnCoinFlipFinished]
    end
    
    subgraph UI Managers
        BDM[BattleDeckManager]
        PD[PhaseDisplayer]
    end
    
    D4MEvents -->|订阅| CM
    CoinEvents -->|订阅| CM
    
    CMEvents -.触发.-> BDM
    CMEvents -.触发.-> PD
    
    CM -->|调用| D4M
    CM -->|调用| CoinM
    
    style CM fill:#ff6b6b
    style D4M fill:#4ecdc4
    style CoinM fill:#4ecdc4
    style BDM fill:#95e1d3
    style PD fill:#95e1d3
```

## 核心数据流

```mermaid
sequenceDiagram
    participant Player
    participant CM as CombatManager
    participant D4M as D4DiceManager
    participant CoinM as CoinManager
    participant Block
    participant Drag as CardDragHandler
    
    Note over CM: 玩家回合开始
    CM->>CM: SetGamePhase(playerDraw)
    
    Player->>D4M: 点击骰子
    D4M->>D4M: RollD4Dice()
    D4M-->>CM: OnD4DiceRollFinished(result)
    CM->>CM: DrawCards(PlayerA, result)
    CM->>CM: SetGamePhase(playerAction)
    
    Player->>CoinM: 点击硬币
    CoinM->>CoinM: FlipCoin()
    CoinM-->>CM: OnCoinFlipFinished(result)
    CM->>CM: DrawFromOpponentAndStore(result)
    CM->>CM: SetGamePhase(playerPlay)
    Player->>Drag: 拖动临时区怪物卡
    Drag->>Drag: ShowAvailableBlocks()
    Drag->>CM: Summon(player, monster, block)
    
    Player->>CM: 点击临时区卡牌
    CM->>Block: 显示可用格子
    Player->>Block: 点击格子
    Block->>CM: SummonConfirm()
    CM->>CM: Summon()
    CM->>CM: EndPlayerTurn()
```

## 问题与改进建议

### 当前存在的问题

1. **回合管理耦合**
   - 回合逻辑全部在 `CombatManager` 中
   - 建议：抽取 `TurnManager`

2. **事件未正确触发**
   - `SetGamePhase()` 没有触发 `phaseChangeEvent`
   - 导致 `PhaseDisplayer` 无法自动更新

3. **阶段流转不完整**
   - `Update()` 只处理敌人回合
   - 玩家回合依赖手动触发

4. **卡牌效果未集成**
   - `Summon()` 时未调用效果系统
   - `EffectManager` 与战斗系统隔离

### 改进建议

```mermaid
graph TB
    subgraph 建议新增
        TM[TurnManager 回合管理器]
        CE[CardEffectExecutor 卡牌效果执行器]
        AI[AIController AI控制器]
    end
    
    subgraph 现有系统
        CM[CombatManager]
        EM[EffectManager]
    end
    
    TM -.管理.-> CM
    CE -.连接.-> CM
    CE -.连接.-> EM
    AI -.控制.-> CM
    
    style TM fill:#ffd93d
    style CE fill:#ffd93d
    style AI fill:#ffd93d
```
