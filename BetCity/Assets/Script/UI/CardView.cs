using UnityEngine;
using UnityEngine.UI;
using BetCity.Card;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMPro.TMP_Text nameText;
    [SerializeField] private TMPro.TMP_Text descriptionText;

    public void Bind(Card card)
    {
        if (card == null) return;
        var s = card.Image;
        if (image != null) image.sprite = s;
        if (nameText != null) nameText.text = card.CardName ?? string.Empty;
        if (descriptionText != null) descriptionText.text = card.Description ?? string.Empty;
    }
}
