using UnityEngine;
using UnityEngine.UI;
using BetCity.Card;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer artworkSpriteRenderer;

    public void Bind(Card card)
    {
        if (card == null) return;
        var s = card.Image;
        if (artworkSpriteRenderer != null) artworkSpriteRenderer.sprite = s;
    }
}
