using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum GamePhase
{
    begin, playerDraw, playerAction, playerPlay, playerDecide,
    enemyDraw, enemyAction, enemyPlay, enemyDecide, endPhase
}

public class CombatManager : MonoSingleton<CombatManager>
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

    public Dictionary<CardOwner, int> SummonCountMax = new Dictionary<CardOwner, int>()
    {
        { CardOwner.PlayerA, 0 }, // 玩家
        { CardOwner.PlayerB, 0 }  // 敌人
    };

    private Dictionary<CardOwner, int> SummonCounter = new Dictionary<CardOwner, int>()
    {
        { CardOwner.PlayerA, 0 },
        { CardOwner.PlayerB, 0 }
    };

    private GameObject waitingMonster;
    private CardOwner waitingPlayer; 

    [Header("骰子系统")]
    public D4DiceManager d4DiceManager;

    [Header("硬币系统")]
    public CoinManager coinManager;            // 硬币管理器

    [Header("抽出区")]
    public Transform temporaryBlock;           // 临时区域（用于存放抽到的牌）

    private int d4DiceResult;
    private bool isRollingD4Dice = false;
    private bool isFlippingCoin = false;       // 标记是否正在投掷硬币
    private CardOwner currentTurnPlayer;
    private List<Card> tempCards = new List<Card>(); // 临时存储抽到的牌

    [Header("状态")]
    public static CombatManager Instance;
    public GamePhase GamePhase = GamePhase.begin;
    public UnityEvent phaseChangeEvent = new UnityEvent();

    // 卡牌列表
    private List<Card> playerDeckList = new List<Card>();
    private List<Card> enemyDeckList = new List<Card>();
    private List<Card> publicDeck = new List<Card>();
    private List<Card> playerHandList = new List<Card>();
    private List<Card> enemyHandList = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    private bool isProcessing = false; // 通用处理标志




    void Start()
    {
        Debug.Log("游戏初始化开始");
        if (playerHand == null) Debug.LogError("未设置玩家手牌区域");
        if (enemyHand == null) Debug.LogError("未设置敌人手牌区域");
        if (cardPrefab == null) Debug.LogError("未设置卡牌预制体");

        GameStart();
    }

    private void Awake()
    {
        Instance = this;
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

    void Update()
    {
        // 处理敌人抽卡阶段
        if (GamePhase == GamePhase.enemyDraw && !isRollingD4Dice && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(EnemyDrawPhase());
        }
        // 处理敌人行动阶段（硬币投掷）
        else if (GamePhase == GamePhase.enemyAction && !isFlippingCoin && !isProcessing)
        {
            isProcessing = true;
            StartCoroutine(EnemyActionPhase());
        }

        // 控制骰子的可点击性
        if (d4DiceManager != null && d4DiceManager.d4DiceObject != null)
        {
            Button diceButton = d4DiceManager.d4DiceObject.GetComponent<Button>();
            if (diceButton != null)
            {
                diceButton.interactable = (GamePhase == GamePhase.playerDraw && !isRollingD4Dice);
            }
        }

        // 控制硬币的可点击性
        if (coinManager != null)
        {
            coinManager.SetInteractable(GamePhase == GamePhase.playerAction && !isFlippingCoin);
        }
    }

    // 设置游戏阶段的方法
    private void SetGamePhase(GamePhase newPhase)
    {
        GamePhase = newPhase;
        Debug.Log($"游戏阶段切换到: {newPhase}");
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

        // 切换到敌人行动阶段
        SetGamePhase(GamePhase.enemyAction);

        isProcessing = false;
    }

    // 敌人行动阶段
    private IEnumerator EnemyActionPhase()
    {
        Debug.Log("敌人开始行动阶段（投掷硬币）");
        isFlippingCoin = true;

        // 注册硬币完成事件
        coinManager.OnCoinFlipFinished += HandleEnemyCoinFlipFinished;

        // 投掷硬币
        coinManager.FlipCoin();

        // 等待硬币投掷完成
        while (isFlippingCoin)
        {
            yield return null;
        }
    }

    // 处理敌人硬币投掷完成
    private void HandleEnemyCoinFlipFinished(CoinResult result)
    {
        coinManager.OnCoinFlipFinished -= HandleEnemyCoinFlipFinished;

        isFlippingCoin = false;
        Debug.Log($"敌人硬币投掷完成，结果: {result}");

        // 根据硬币结果从对手抽卡
        int cardsToDraw = (result == CoinResult.Heads) ? 2 : 1;
        StartCoroutine(DrawFromOpponentAndStore(CardOwner.PlayerB, cardsToDraw));
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

        // 切换到玩家行动阶段
        SetGamePhase(GamePhase.playerAction);

        isProcessing = false;
    }

    // 玩家行动阶段（硬币投掷）
    public void OnCoinClicked()
    {
        if (isFlippingCoin || GamePhase != GamePhase.playerAction) return;

        Debug.Log("玩家开始投掷硬币");
        isFlippingCoin = true;

        // 注册硬币完成事件
        coinManager.OnCoinFlipFinished += HandlePlayerCoinFlipFinished;

        // 投掷硬币
        coinManager.FlipCoin();
    }

    // 处理玩家硬币投掷完成
    private void HandlePlayerCoinFlipFinished(CoinResult result)
    {
        coinManager.OnCoinFlipFinished -= HandlePlayerCoinFlipFinished;

        isFlippingCoin = false;
        Debug.Log($"玩家硬币投掷完成，结果: {result}");

        // 根据硬币结果从对手抽卡
        int cardsToDraw = (result == CoinResult.Heads) ? 2 : 1;
        StartCoroutine(DrawFromOpponentAndStore(CardOwner.PlayerA, cardsToDraw));
    }



    // 清空临时区域
    private void ClearTemporaryBlock()
    {
        if (temporaryBlock == null) return;

        int childCount = temporaryBlock.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(temporaryBlock.GetChild(i).gameObject);
        }

        // 清理后强制刷新布局
        if (temporaryBlock.GetComponent<RectTransform>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(temporaryBlock.GetComponent<RectTransform>());
        }
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
        SetGamePhase(GamePhase.playerDraw);
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
        SetGamePhase(GamePhase.enemyDraw);
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
 private GameObject InstantiateCardUI(Card drawnCard, Transform parentTransform)
    {
        try
        {
            GameObject cardObj = Instantiate(cardPrefab, parentTransform);

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

            // 统一设置BattleCard组件
            BattleCard battleCard = cardObj.GetComponent<BattleCard>();
            if (battleCard != null)
            {
                // 始终使用卡牌的原始所有者
                battleCard.playerOwner = drawnCard.owner;
                
                // 根据父节点统一设置状态
                if (parentTransform == temporaryBlock)
                {
                    battleCard.state = BattleCardState.inTemp;
                    Debug.Log($"卡牌 {drawnCard.cardName} 状态设置为临时区");
                }
                else if (parentTransform == playerHand)
                {
                    battleCard.state = BattleCardState.inHand;
                    Debug.Log($"卡牌 {drawnCard.cardName} 状态设置为玩家手牌");
                }
                else if (parentTransform == enemyHand)
                {
                    battleCard.state = BattleCardState.inHand;
                    Debug.Log($"卡牌 {drawnCard.cardName} 状态设置为敌人手牌");
                }
                else
                {
                    battleCard.state = BattleCardState.inHand;
                }
            }
            else
            {
                Debug.LogWarning($"卡牌 {drawnCard.cardName} 缺少 BattleCard 组件");
            }

            // 强制刷新布局
            if (parentTransform.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentTransform.GetComponent<RectTransform>());

                GridLayoutGroup gridLayout = parentTransform.GetComponent<GridLayoutGroup>();
                if (gridLayout != null)
                {
                    Canvas.ForceUpdateCanvases();
                }
            }
            
            return cardObj;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"实例化卡牌失败: {e.Message}");
            return null;
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
            Debug.Log($"牌库重置完成: 玩家A抽{playerDrawCount}张, 玩家B抽{enemyDrawCount}张");
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

    // 从对手抽卡并存储到临时区域
    private IEnumerator DrawFromOpponentAndStore(CardOwner currentPlayer, int count)
    {
        Debug.Log($"从对手抽{count}张卡到临时区域");

        // 确定对手
        CardOwner opponent = (currentPlayer == CardOwner.PlayerA) ? CardOwner.PlayerB : CardOwner.PlayerA;

        // 获取对手手牌列表
        List<Card> opponentHand = (opponent == CardOwner.PlayerA) ? playerHandList : enemyHandList;

        // 清空临时卡片列表
        tempCards.Clear();

        // 清空临时区域
        ClearTemporaryBlock();

        // 检查对手手牌是否足够
        if (opponentHand.Count < count)
        {
            Debug.Log($"对手手牌不足，需求{count}张，实际只有{opponentHand.Count}张，触发惩罚机制");

            // 执行惩罚机制
            yield return StartCoroutine(ExecutePunishmentMechanism(currentPlayer, opponent));

            // 惩罚后，重新获取对手手牌列表（因为惩罚机制可能让对手抽了卡）
            opponentHand = (opponent == CardOwner.PlayerA) ? playerHandList : enemyHandList;

            // 惩罚后继续抽剩余可抽的卡牌
            int remainingCardsToDraw = Mathf.Min(count, opponentHand.Count);
            if (remainingCardsToDraw > 0)
            {

                // 直接抽卡到临时区域
                for (int i = 0; i < remainingCardsToDraw; i++)
                {

                    // 随机选择一张卡
                    int randomIndex = Random.Range(0, opponentHand.Count);
                    Card drawnCard = opponentHand[randomIndex];
                    opponentHand.RemoveAt(randomIndex);

                    // 添加到临时卡片列表
                    tempCards.Add(drawnCard);

                    // 实例化卡牌UI到临时区域
                    GameObject cardObj = InstantiateCardUI(drawnCard, temporaryBlock);

                    Debug.Log($"抽到卡牌: {drawnCard.cardName}");

                    // 等待一小段时间
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
        else
        {
            // 正常抽卡到临时区域
            for (int i = 0; i < count; i++)
            {
                // 随机选择一张卡
                int randomIndex = Random.Range(0, opponentHand.Count);
                Card drawnCard = opponentHand[randomIndex];
                opponentHand.RemoveAt(randomIndex);

                // 添加到临时卡片列表
                tempCards.Add(drawnCard);

                // 实例化卡牌UI到临时区域
                GameObject cardObj = InstantiateCardUI(drawnCard, temporaryBlock);

                Debug.Log($"抽到卡牌: {drawnCard.cardName}");

                // 等待一小段时间
                yield return new WaitForSeconds(0.3f);
            }
        }

        // 切换到下一个阶段
        if (currentPlayer == CardOwner.PlayerA)
        {
            SetGamePhase(GamePhase.playerPlay);
        }
        else
        {
            SetGamePhase(GamePhase.enemyPlay);
        }

        isProcessing = false;
    }

    // 执行惩罚机制
    private IEnumerator ExecutePunishmentMechanism(CardOwner currentPlayer, CardOwner opponent)
    {
        Debug.Log("执行惩罚机制");

        // 惩罚1：当前玩家从手牌打出一张卡（并结算效果）
        // TODO: 实现当前玩家从手牌打出一张卡的逻辑
        // 示例代码（需要根据实际UI交互实现）：
        // if (currentPlayer == CardOwner.PlayerA && playerHandList.Count > 0)
        // {
        //     // 等待玩家选择一张卡打出
        //     yield return StartCoroutine(WaitForPlayerToPlayCard());
        // }

        // 惩罚2：对手抽5张卡
        Debug.Log("惩罚2：对手抽5张卡");
        int actualDrawCount = DrawCards(opponent, 5);
        Debug.Log($"对手抽{actualDrawCount}张卡作为惩罚");

        yield return new WaitForSeconds(0.5f);
    }

    public void SummonRequest(CardOwner player, GameObject monster)
    {
        GameObject[] blocks;
        bool hasEmptyBlock = false;
        blocks = Blocks;

        if (SummonCounter[player] > 0)
        {
            foreach (var block in blocks)
            {
                if (block.GetComponent<Block>().card == null)
                {
                    block.GetComponent<Block>().SummonBlock.SetActive(true); // 等待召唤显示
                    hasEmptyBlock = true;
                }
            }
        }

        if (hasEmptyBlock)
        {
            waitingMonster = monster;
            waitingPlayer = player; // 直接存储 CardOwner
        }
        else
        {
            Debug.LogWarning($"玩家{player}没有空位进行召唤");
        }
    }

    public void SummonConfirm(Transform block)
    {
        Summon(waitingPlayer, waitingMonster, block);
    }

    public void Summon(CardOwner player, GameObject monster, Transform block)
    {
        monster.transform.SetParent(block);
        monster.transform.localPosition = Vector3.zero;

        BattleCard battleCard = monster.GetComponent<BattleCard>();
        if (battleCard != null)
        {
            battleCard.state = BattleCardState.inBlock;
        }

        block.GetComponent<Block>().card = monster;
        SummonCounter[player]--;

        // 隐藏所有召唤提示
        foreach (var b in Blocks)
        {
            b.GetComponent<Block>().SummonBlock.SetActive(false);
        }

        // 重置等待状态
        waitingMonster = null;
       // waitingPlayer = none; // 无等待玩家

        Debug.Log($"玩家{player}成功召唤卡牌到位置");
    }
}