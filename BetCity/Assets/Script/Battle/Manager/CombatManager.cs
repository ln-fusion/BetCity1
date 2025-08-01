using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    begin,playerDraw,playerAction,playerPlay,playerDecide, enemyDraw, enemyAction, enemyPlay, enemyDecide,endPhase
}

public class CombatManager : MonoBehaviour
{
    public PlayerData playerData;
    public PlayerData enemyData;

    public List<Card> playerDeckList;
    public List<Card> enemyDeckList;

    public Transform playerHand;
    public Transform enemyHand;

    public GameObject[] Blocks;

    public GameObject playerIcon;
    public GameObject enemyIcon;

    public GamePhase GamePhase = GamePhase.begin;

    // Start is called before the first frame update
    void Start()
    {
        ReadDeck();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {

    }

    public void ReadDeck()
    {
        for(int i=0;i<playerData.playerDeck.Length;i++)
        {
            if (playerData.playerDeck[i] !=0)
            {
                int count = playerData.playerDeck[i];
                for (int j = 0; i < count;j ++)
                {
                    playerDeckList.Add(playerData.CardStore.CopyCard(i));
                }
            }
        }
        for (int i = 0; i < enemyData.playerDeck.Length; i++)
        {
            if (enemyData.playerDeck[i] != 0)
            {
                int count = enemyData.playerDeck[i];
                for (int j = 0; i < count; j++)
                {
                    enemyDeckList.Add(enemyData.CardStore.CopyCard(i));
                }
            }
        }
    }
    public void EndPhase()//回合结束
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
