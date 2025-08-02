using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    begin, playerDraw, playerAction, playerPlay, playerDecide,
    enemyDraw, enemyAction, enemyPlay, enemyDecide, endPhase
}

public class CombatManager : MonoBehaviour
{
    public PlayerData playerData;
    public PlayerData enemyData;

    public List<Card> playerDeckList;      // 玩家原始卡组
    public List<Card> enemyDeckList;       // 敌人原始卡组
    public List<Card> publicDeck = new List<Card>(); // 公共牌库

    // 添加以下缺失的成员变量
    public List<Card> playerHandList = new List<Card>(); // 玩家A手牌
    public List<Card> enemyHandList = new List<Card>();   // 玩家B手牌
    public List<Card> discardPile = new List<Card>();     // 弃牌堆

    public Transform playerHand;  // 玩家手牌的UI容器
    public Transform enemyHand;   // 敌人手牌的UI容器

    public GameObject[] Blocks;   // 场地网格块

    public GameObject playerIcon; // 玩家头像
    public GameObject enemyIcon;  // 敌人头像

    public GamePhase GamePhase = GamePhase.begin;

    void Start()
    {
        // 初始化列表
        playerDeckList = new List<Card>();
        enemyDeckList = new List<Card>();

        ReadDeck();
        ShuffleDeck();
    }

    void Update() { }

    public void GameStart() { }

    public void ReadDeck()
    {
        // 读取玩家卡组
        for (int i = 0; i < playerData.playerDeck.Length; i++)
        {
            if (playerData.playerDeck[i] != 0)
            {
                int count = playerData.playerDeck[i];
                for (int j = 0; j < count; j++)
                {
                    Card newCard = playerData.CardStore.CopyCard(i);
                    newCard.owner = CardOwner.PlayerA;
                    playerDeckList.Add(newCard);
                }
            }
        }

        // 读取敌人卡组
        for (int i = 0; i < enemyData.playerDeck.Length; i++)
        {
            if (enemyData.playerDeck[i] != 0)
            {
                int count = enemyData.playerDeck[i];
                for (int j = 0; j < count; j++)
                {
                    Card newCard = enemyData.CardStore.CopyCard(i);
                    newCard.owner = CardOwner.PlayerB;
                    enemyDeckList.Add(newCard);
                }
            }
        }
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

    public int DrawCards(CardOwner player, int count)
    {
        int drawnCount = 0;

        // 处理抽卡
        for (int i = 0; i < count; i++)
        {
            if (publicDeck.Count == 0)
            {
                // 触发牌库重置
                ResetDeck();

                // 重置后跳过剩余抽卡（根据规则）
                Debug.Log($"牌库重置后跳过剩余抽卡（原本需抽{count}张，实际抽了{drawnCount}张）");
                break;
            }

            // 从公共牌库顶部抽卡（列表最后一张）
            Card drawnCard = publicDeck[publicDeck.Count - 1];
            publicDeck.RemoveAt(publicDeck.Count - 1);

            // 根据玩家类型添加到对应手牌
            if (player == CardOwner.PlayerA)
            {
                playerHandList.Add(drawnCard);
                // 这里需要添加UI更新逻辑
                Debug.Log($"玩家A抽到卡：{drawnCard.cardName}");
            }
            else
            {
                enemyHandList.Add(drawnCard);
                // 这里需要添加UI更新逻辑
                Debug.Log($"玩家B抽到卡：{drawnCard.cardName}");
            }

            drawnCount++;
        }

        return drawnCount;
    }

    // 牌库重置功能
    public void ResetDeck()
    {
        Debug.Log("触发牌库重置");

        // 1. 双方弃置所有手牌
        DiscardAllHands();

        // 2. 将弃牌堆加入公共牌库
        publicDeck.AddRange(discardPile);
        discardPile.Clear();

        // 3. 重新洗牌
        ShuffleDeck();

        // 4. 双方各抽5张牌
        int playerDrawCount = DrawCards(CardOwner.PlayerA, 5);
        int enemyDrawCount = DrawCards(CardOwner.PlayerB, 5);

        Debug.Log($"牌库重置完成：玩家A抽{playerDrawCount}张，玩家B抽{enemyDrawCount}张");
    }

    // 弃置所有手牌
    private void DiscardAllHands()
    {
        // 记录弃牌数量
        int playerHandCount = playerHandList.Count;
        int enemyHandCount = enemyHandList.Count;

        // 玩家A弃牌
        discardPile.AddRange(playerHandList);
        playerHandList.Clear();
        Debug.Log($"玩家A弃置{playerHandCount}张手牌");

        // 玩家B弃牌
        discardPile.AddRange(enemyHandList);
        enemyHandList.Clear();
        Debug.Log($"玩家B弃置{enemyHandCount}张手牌");
    }

    // 从对手手牌抽卡（抛硬币机制）
    public List<Card> DrawFromOpponent(CardOwner currentPlayer, out bool punishmentTriggered)
    {
        punishmentTriggered = false;
        List<Card> drawnCards = new List<Card>();

        // 确定对手
        CardOwner opponent = currentPlayer == CardOwner.PlayerA ? CardOwner.PlayerB : CardOwner.PlayerA;
        List<Card> opponentHand = opponent == CardOwner.PlayerA ? playerHandList : enemyHandList;

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

            // 惩罚1：当前玩家从手牌打出一张卡（这里需要后续实现具体打出逻辑）
            // 惩罚2：对手抽5张卡
            int actualDrawCount = DrawCards(opponent, 5);
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