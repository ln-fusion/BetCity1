using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardState
{
    Library,Deck
}

public class ClickCard : MonoBehaviour,IPointerDownHandler
{
    private DeckManager DeckManager;
    public CardState state;
    // Start is called before the first frame update
    void Start()
    {
        GameObject deckManagerObj = GameObject.Find("DeckManager");
        if (deckManagerObj != null)
        {
            DeckManager = deckManagerObj.GetComponent<DeckManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (DeckManager == null)
        {
            return;
        }

        CardDisplay display = GetComponent<CardDisplay>();
        if (display == null || display.card == null)
        {
            return;
        }

        int id = display.card.id;
        DeckManager.UpdateCard(state, id);

    }
}
