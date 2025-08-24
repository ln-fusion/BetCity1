using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    begin, playerDraw, playerAction, playerPlay, playerDecide,
    enemyDraw, enemyAction, enemyPlay, enemyDecide, endPhase
}

public class CombatManager : MonoBehaviour
{
    [Header("数据配置")]
    public PlayerData playerData;
    public PlayerData enemyData;
    public GameObject cardPrefab;

    [Header("UI 组件")]
    public Transform playerHand;
    public Transform enemyHand;
    public GameObject playerIcon;
    public GameObject enemyIcon;

    [Header("游戏区域")]
    public GameObject[] Blocks;

    [Header("骰子系统")]
    public D4DiceManager d4DiceManager;      

    private int d4DiceResult;               
    private bool isRollingD4Dice = false;    
    private CardOwner currentTurnPlayer;   

    [Header("状态")]
    public GamePhase GamePhase = GamePhase.begin;

    // 卡牌列表
    private List<Card> playerDeckList = new List<Card>();
    private List<Card> enemyDeckList = new List<Card>();
    private List<Card> publicDeck = new List<Card>();
    private List<Card> playerHandList = new List<Card>();
    private List<Card> enemyHandList = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    void Start()
    {
        Debug.Log("游戏初始化开始");
        if (playerHand == null) Debug.LogError("未设置玩家手牌区域");
        if (enemyHand == null) Debug.LogError("未设置敌人手牌区域");
        if (cardPrefab == null) Debug.LogError("未设置卡牌预制体");

        GameStart();
    }

    void OnDestroy()
    {
        ClearAllHands();
        Resources.UnloadUnusedAssets();
    }

    public void GameStart()
    {
        Debug.Log("游戏开始");

        // 初始化列表
        playerDeckList.Clear();
        enemyDeckList.Clear();
        publicDeck.Clear();
        playerHandList.Clear();
        enemyHandList.Clear();
        discardPile.Clear();

        // 清空手牌区域
        ClearHand(playerHand);
        ClearHand(enemyHand);

        // 读卡组数据
        ReadDeck();

        // 洗牌
        ShuffleDeck();

        // 双方抽初始卡
        int playerDrawCount = DrawCardsWithoutReset(CardOwner.PlayerA, 5);
        int enemyDrawCount = DrawCardsWithoutReset(CardOwner.PlayerB, 5);

        Debug.Log($"游戏初始化完成: 玩家A抽{playerDrawCount}张, 玩家B抽{enemyDrawCount}张");

        // 设置初始回合玩家
        currentTurnPlayer = CardOwner.PlayerA; // 玩家先手
        GamePhase = GamePhase.playerDraw;
    }

    // 更新方法，处理AI自动行动
    void Update()
    {
        // 处理敌人抽卡阶段
        if (GamePhase == GamePhase.enemyDraw && !isRollingD4Dice)
        {
            StartCoroutine(EnemyDrawPhase());
        }
    }

    // 敌人抽卡阶段
    private IEnumerator EnemyDrawPhase()
    {
        isRollingD4Dice = true;
        Debug.Log("敌人开始抽卡阶段");

        // 自动投掷四面骰子
        d4DiceManager.OnD4DiceRollFinished += HandleEnemyD4DiceRollFinished;
        d4DiceManager.RollD4Dice();

        // 等待骰子投掷完成
        while (isRollingD4Dice)
        {
            yield return null;
        }
    }

    // 处理敌人骰子投掷完成
    private void HandleEnemyD4DiceRollFinished(int result)
    {
        d4DiceManager.OnD4DiceRollFinished -= HandleEnemyD4DiceRollFinished;

        d4DiceResult = result;
        isRollingD4Dice = false;

        Debug.Log($"敌人骰子投掷完成，点数: {result}");

        // 敌人抽卡
        StartCoroutine(ExecuteEnemyDrawPhase());
    }

    // 执行敌人抽卡阶段
    private IEnumerator ExecuteEnemyDrawPhase()
    {
        Debug.Log($"敌人开始抽卡，需要抽{d4DiceResult}张卡");

        // 检查牌库是否为空
        if (publicDeck.Count == 0)
        {
            Debug.Log("牌库为空，触发重置");
            ResetDeck(true);
        }
        else
        {
            // 实际抽卡数量（不能超过牌库数量）
            int actualDraw = Mathf.Min(d4DiceResult, publicDeck.Count);
            DrawCards(CardOwner.PlayerB, actualDraw);
        }

        // 等待一小段时间让抽卡完成
        yield return new WaitForSeconds(0.5f);

        // 测试：跳过后续阶段，直接结束敌人回合********************************************************
        Debug.Log("敌人抽卡完成，跳过后续阶段");
        EndEnemyTurn();
    }

    // 结束敌人回合
    private void EndEnemyTurn()
    {
        Debug.Log("敌人回合结束");

        // 检查手牌上限
        CheckHandLimit(CardOwner.PlayerB);

        // 检查失败条件
        CheckLoseCondition(CardOwner.PlayerB);

        // 切换到玩家回合
        currentTurnPlayer = CardOwner.PlayerA;
        GamePhase = GamePhase.playerDraw;
    }

    // 点击四面骰子开始投掷（玩家）
    public void OnD4DiceClicked()
    {
        if (isRollingD4Dice || GamePhase != GamePhase.playerDraw) return;

        Debug.Log("玩家开始投掷四面骰子");
        isRollingD4Dice = true;

        // 注册四面骰子完成事件
        d4DiceManager.OnD4DiceRollFinished += HandlePlayerD4DiceRollFinished;

        // 投掷四面骰子
        d4DiceManager.RollD4Dice();
    }

    // 处理玩家骰子投掷完成
    private void HandlePlayerD4DiceRollFinished(int result)
    {
        d4DiceManager.OnD4DiceRollFinished -= HandlePlayerD4DiceRollFinished;

        d4DiceResult = result;
        isRollingD4Dice = false;

        Debug.Log($"玩家骰子投掷完成，点数: {result}");

        // 玩家抽卡
        StartCoroutine(ExecutePlayerDrawPhase());
    }

    // 执行玩家抽卡阶段
    private IEnumerator ExecutePlayerDrawPhase()
    {
        Debug.Log($"玩家开始抽卡，需要抽{d4DiceResult}张卡");

        // 检查牌库是否为空
        if (publicDeck.Count == 0)
        {
            Debug.Log("牌库为空，触发重置");
            ResetDeck(true);
        }
        else
        {
            // 实际抽卡数量（不能超过牌库数量）
            int actualDraw = Mathf.Min(d4DiceResult, publicDeck.Count);
            DrawCards(CardOwner.PlayerA, actualDraw);
        }

        // 等待一小段时间让抽卡完成
        yield return new WaitForSeconds(0.5f);

        // 测试：跳过后续阶段，直接结束玩家回合********************************************************
        Debug.Log("玩家抽卡完成，跳过后续阶段");
        EndPlayerTurn();
    }

    // 结束玩家回合
    private void EndPlayerTurn()
    {
        Debug.Log("玩家回合结束");

        // 检查手牌上限
        CheckHandLimit(CardOwner.PlayerA);

        // 检查失败条件
        CheckLoseCondition(CardOwner.PlayerA);

        // 切换到敌人回合
        currentTurnPlayer = CardOwner.PlayerB;
        GamePhase = GamePhase.enemyDraw;
    }

    // 检查手牌上限
    private void CheckHandLimit(CardOwner player)
    {
        List<Card> handList = player == CardOwner.PlayerA ? playerHandList : enemyHandList;
        if (handList.Count > 8)
        {
            int discardCount = handList.Count - 8;
            Debug.Log($"{player}手牌超过8张，需要弃置{discardCount}张");
            // 这里需要实现弃牌逻辑
        }
    }

    // 检查失败条件
    private void CheckLoseCondition(CardOwner player)
    {
        // 这里需要实现检查场上卡牌数量的逻辑
    }

    // 以下为原有方法，保持不变
    private void ClearHand(Transform hand)
    {
        if (hand == null) return;

        int childCount = hand.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(hand.GetChild(i).gameObject);
        }
    }

    private void ClearAllHands()
    {
        ClearHand(playerHand);
        ClearHand(enemyHand);
    }

    public void ReadDeck()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData 未设置");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogError("EnemyData 未设置");
            return;
        }

        // 读取玩家卡组
        for (int i = 0; i < playerData.playerDeck.Length; i++)
        {
            if (playerData.playerDeck[i] != 0)
            {
                int count = playerData.playerDeck[i];
                for (int j = 0; j < count; j++)
                {
                    Card newCard = playerData.CardStore.CopyCard(i);
                    if (newCard != null)
                    {
                        newCard.owner = CardOwner.PlayerA;
                        playerDeckList.Add(newCard);
                    }
                    else
                    {
                        Debug.LogError($"创建玩家卡牌 {i} 失败");
                    }
                }
            }
        }
        Debug.Log($"玩家卡组读取完成，卡牌数量: {playerDeckList.Count}");

        // 读取敌人卡组
        for (int i = 0; i < enemyData.playerDeck.Length; i++)
        {
            if (enemyData.playerDeck[i] != 0)
            {
                int count = enemyData.playerDeck[i];
                for (int j = 0; j < count; j++)
                {
                    Card newCard = enemyData.CardStore.CopyCard(i);
                    if (newCard != null)
                    {
                        newCard.owner = CardOwner.PlayerB;
                        enemyDeckList.Add(newCard);
                    }
                    else
                    {
                        Debug.LogError($"创建敌人卡牌 {i} 失败");
                    }
                }
            }
        }
        Debug.Log($"敌人卡组读取完成，卡牌数量: {enemyDeckList.Count}");
    }

    public void ShuffleDeck()
    {
        publicDeck.Clear();
        publicDeck.AddRange(playerDeckList);
        publicDeck.AddRange(enemyDeckList);

        // Fisher-Yates洗牌算法
        System.Random rng = new System.Random();
        int n = publicDeck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = publicDeck[k];
            publicDeck[k] = publicDeck[n];
            publicDeck[n] = value;
        }

        Debug.Log($"公共牌库已洗牌，共{publicDeck.Count}张卡");
    }

    // 主抽卡方法（可能触发重置）
    public int DrawCards(CardOwner player, int count)
    {
        if (playerHand == null || enemyHand == null)
        {
            Debug.LogError("手牌区域未设置");
            return 0;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("卡牌预制体未设置");
            return 0;
        }

        Transform handTransform = player == CardOwner.PlayerA ? playerHand : enemyHand;
        int drawnCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (publicDeck.Count == 0)
            {
                Debug.Log("牌库为空，尝试重置");
                ResetDeck(false); // 重置但不抽初始卡

                // 重置后如果牌库仍为空，则跳出循环
                if (publicDeck.Count == 0)
                {
                    Debug.LogError("重置后牌库仍为空");
                    break;
                }

                // 继续抽当前这张卡
                i--; // 保持循环计数不变
                continue;
            }

            // 抽卡
            Card drawnCard = publicDeck[publicDeck.Count - 1];
            publicDeck.RemoveAt(publicDeck.Count - 1);

            // 添加到手牌列表
            if (player == CardOwner.PlayerA)
            {
                playerHandList.Add(drawnCard);
            }
            else
            {
                enemyHandList.Add(drawnCard);
            }

            // 实例化卡牌UI
            InstantiateCardUI(drawnCard, handTransform);

            drawnCount++;
        }
        return drawnCount;
    }

    // 不会触发重置的抽卡方法（用于重置后的初始抽卡）
    private int DrawCardsWithoutReset(CardOwner player, int count)
    {
        Transform handTransform = player == CardOwner.PlayerA ? playerHand : enemyHand;
        int drawnCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (publicDeck.Count == 0)
            {
                Debug.Log("牌库为空，无法继续抽卡");
                break;
            }

            // 抽卡
            Card drawnCard = publicDeck[publicDeck.Count - 1];
            publicDeck.RemoveAt(publicDeck.Count - 1);

            // 添加到手牌列表
            if (player == CardOwner.PlayerA)
            {
                playerHandList.Add(drawnCard);
            }
            else
            {
                enemyHandList.Add(drawnCard);
            }

            // 实例化卡牌UI
            InstantiateCardUI(drawnCard, handTransform);

            drawnCount++;
        }
        return drawnCount;
    }

    // 卡牌UI实例化方法
    private void InstantiateCardUI(Card drawnCard, Transform handTransform)
    {
        try
        {
            GameObject cardObj = Instantiate(cardPrefab, handTransform);

            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.card = drawnCard;
                display.UpdateCardDisplay();
            }
            else
            {
                Debug.LogError("卡牌预制体缺少 CardDisplay 组件");
            }

            // 强制刷新布局
            if (handTransform.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(handTransform.GetComponent<RectTransform>());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"实例化卡牌失败: {e.Message}");
        }
    }

    // 重置牌库方法（添加参数控制是否抽初始卡）
    public void ResetDeck(bool drawInitialCards = true)
    {
        Debug.Log("触发牌库重置");

        // 1. 双方弃置所有手牌
        DiscardAllHands();

        // 2. 将弃牌堆加入公共牌库
        publicDeck.AddRange(discardPile);
        discardPile.Clear();

        // 3. 重新洗牌
        ShuffleDeck();

        // 4. 只有在需要时才抽初始卡
        if (drawInitialCards)
        {
            int playerDrawCount = DrawCardsWithoutReset(CardOwner.PlayerA, 5);
            int enemyDrawCount = DrawCardsWithoutReset(CardOwner.PlayerB, 5);
            Debug.Log($"牌库重置完成：玩家A抽{playerDrawCount}张，玩家B抽{enemyDrawCount}张");
        }
    }

    private void DiscardAllHands()
    {
        // 玩家A弃牌
        int playerHandCount = playerHandList.Count;
        discardPile.AddRange(playerHandList);
        playerHandList.Clear();
        ClearHand(playerHand);
        Debug.Log($"玩家A弃置{playerHandCount}张手牌");

        // 玩家B弃牌
        int enemyHandCount = enemyHandList.Count;
        discardPile.AddRange(enemyHandList);
        enemyHandList.Clear();
        ClearHand(enemyHand);
        Debug.Log($"玩家B弃置{enemyHandCount}张手牌");
    }

    // 修改DrawFromOpponent方法，添加抽卡数量参数
    public List<Card> DrawFromOpponent(CardOwner currentPlayer, out bool punishmentTriggered, int y = -1)
    {
        punishmentTriggered = false;
        List<Card> drawnCards = new List<Card>();

        // 确定当前玩家和对手的归属
        CardOwner opponentOwner = (currentPlayer == CardOwner.PlayerA) ? CardOwner.PlayerB : CardOwner.PlayerA;

        // 获取对手手牌列表
        List<Card> opponentHand = (opponentOwner == CardOwner.PlayerA) ? playerHandList : enemyHandList;

        // 如果未指定y值，随机决定
        if (y < 0)
        {
            y = UnityEngine.Random.Range(0, 2) == 0 ? 2 : 1;
        }

        if (opponentHand.Count >= y)
        {
            // 正常抽卡
            for (int i = 0; i < y; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, opponentHand.Count);
                drawnCards.Add(opponentHand[randomIndex]);
                opponentHand.RemoveAt(randomIndex);
            }
            Debug.Log($"从对手抽到{y}张卡");
        }
        else
        {
            // 触发惩罚机制
            punishmentTriggered = true;
            Debug.Log("触发惩罚机制");

            // 惩罚1：当前玩家从手牌打出一张卡
            // 这里需要后续实现具体打出逻辑

            // 惩罚2：对手抽5张卡
            int actualDrawCount = DrawCards(opponentOwner, 5);
            Debug.Log($"对手抽{actualDrawCount}张卡作为惩罚");
        }

        return drawnCards;
    }

    // 回合结束方法 - 保留但不使用
    public void EndPhase()
    {
        Debug.Log("EndPhase called, but not used in test mode");
    }
}