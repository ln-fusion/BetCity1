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

    public Transform playerHand;
    public Transform enemyHand;

    public GameObject[] Blocks;

    public GameObject playerIcon;
    public GameObject enemyIcon;

    public GamePhase GamePhase = GamePhase.begin;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
