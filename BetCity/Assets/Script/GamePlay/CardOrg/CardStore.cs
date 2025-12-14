using System.Collections.Generic;
using UnityEngine;

public class CardStore : MonoBehaviour
{
    public TextAsset cardData;
    public List<Card> cardList = new List<Card>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadCardData()
    {
        string[] dataRow = cardData.text.Split('\n');
        foreach (var row in dataRow)
        {
            string[] rowArray = row.Split(',');
            if (rowArray[0]=="#")
            {
                continue;
            } 
            else if (rowArray[0]=="monster")
               //怪兽卡
            {
                int id = int.Parse(rowArray[1]);
                string cardName = rowArray[2];
                string description = rowArray[3];                
                int cardArtworkid = int.Parse(rowArray[4]);     
                int score = int.Parse(rowArray[5]);              
                CardSeries series = ParseCardSeries(rowArray[6]);
                MonsterCard monsterCard = new MonsterCard(id,cardName,description,cardArtworkid,score,CardOwner.PlayerA,series) ;
                cardList.Add(monsterCard);
                Debug.Log(monsterCard.series);

            }
            else if (rowArray[0]=="spell")
            //魔法卡
            {
                int id = int.Parse(rowArray[1]);
                string cardName = rowArray[2];
                string description = rowArray[3];
                int cardArtworkid = int.Parse(rowArray[4]);
                CardSeries series = ParseCardSeries(rowArray[5]);

                SpellCard spellCard = new SpellCard(id, cardName, description, cardArtworkid, CardOwner.PlayerA, series);
                cardList.Add(spellCard);
                Debug.Log(spellCard.cardName);
            }
        }
    }
    public static CardSeries ParseCardSeries(string seriesString)
    {
        if (string.IsNullOrEmpty(seriesString))
            return CardSeries.None;

        // 尝试解析枚举
        if (System.Enum.TryParse(seriesString, true, out CardSeries result))
        {
            return result;
        }

        Debug.LogWarning($"无法解析CardSeries: {seriesString}，使用默认值None");
        return CardSeries.None;
    }

    public Card RandomCard()
    {
        Card card = cardList[Random.Range(0, cardList.Count)];
        return card;
    }

    public Card CopyCard(int _id)
    {
        // 首先通过ID查找原始卡片
        Card originalCard = cardList.Find(card => card.id == _id);
        if (originalCard == null)
        {
            Debug.LogError($"未找到ID为{_id}的卡片");
            return null;
        }

        // 根据原始卡片类型创建对应的副本
        if (originalCard is MonsterCard originalMonster)
        {
            // 复制怪兽卡的所有属性
            return new MonsterCard(
                originalMonster.id,
                originalMonster.cardName,
                originalMonster.description,
                originalMonster.cardArtworkid,
                originalMonster.score,  
                originalMonster.owner,
                originalMonster.series
            );
        }
        else if (originalCard is SpellCard originalSpell)
        {
            // 复制魔法卡的所有属性
            return new SpellCard(
                originalSpell.id,
                originalSpell.cardName,
                originalSpell.description,
                originalSpell.cardArtworkid,
                originalSpell.owner,
                originalSpell.series
            );
        }

        Debug.LogError($"不支持的卡片类型: {originalCard.GetType()}");
        return null;
    }

}
