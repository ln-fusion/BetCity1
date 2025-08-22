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

        // 确保UI组件已设置
        if (playerHand == null) Debug.LogError("未设置玩家手牌区域");
        if (enemyHand == null) Debug.LogError("未设置敌人手牌区域");
        if (cardPrefab == null) Debug.LogError("未设置卡牌预制体");

        GameStart();
    }

    void OnDestroy()
    {
        // 清理所有资源
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

        // 双方抽初始卡 - 使用不会触发重置的版本
        int playerDrawCount = DrawCardsWithoutReset(CardOwner.PlayerA, 5);
        int enemyDrawCount = DrawCardsWithoutReset(CardOwner.PlayerB, 5);

        Debug.Log($"游戏初始化完成: 玩家A抽{playerDrawCount}张, 玩家B抽{enemyDrawCount}张");
    }

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
    public List<Card> DrawFromOpponent(CardOwner currentPlayer, out bool punishmentTriggered)
    {
        punishmentTriggered = false;
        List<Card> drawnCards = new List<Card>();

        // 确定当前玩家和对手的归属
        CardOwner opponentOwner = (currentPlayer == CardOwner.PlayerA) ? CardOwner.PlayerB : CardOwner.PlayerA;

        // 获取对手手牌列表
        List<Card> opponentHand = (opponentOwner == CardOwner.PlayerA) ? playerHandList : enemyHandList;

        // 抛硬币决定抽卡数量
        int y = UnityEngine.Random.Range(0, 2) == 0 ? 2 : 1; // 0:正面(抽2), 1:反面(抽1)

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

    public void EndPhase() // 回合结束
    {
        if (GamePhase == GamePhase.playerDecide)
        {
            GamePhase = GamePhase.enemyDraw;
        }
        else if (GamePhase == GamePhase.enemyDecide)
        {
            GamePhase = GamePhase.playerDraw;
        }
    }
}