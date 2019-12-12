using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSuit
{
    CLUBS,
    HEARTS,
    DIAMONDS,
    SPADES,
}

public enum CardColor
{
    RED,
    BLACK
}

[System.Serializable]
public struct Card
{
    public CardSuit suit;
    private CardColor color;

    [Range(1, 13)]
    public int value;

    public Card(CardSuit aSuit, int aValue)
    {
        suit = aSuit;
        value = aValue;
        if (suit == CardSuit.SPADES || suit == CardSuit.CLUBS)
        {
            color = CardColor.BLACK;
        }
        else
        {
            color = CardColor.RED;
        }
    }

    override public string ToString()
    {
        return suit.ToString() + value;
    }
}

public class CardSpriteManager
{
    private static CardSpriteManager instance;
    private Dictionary<string, Sprite> cardFrontSprites = new Dictionary<string, Sprite>();
    private Sprite backOfCardSprite;
    public static CardSpriteManager Load()
    {
        if (CardSpriteManager.instance == null)
        {
            CardSpriteManager.instance = new CardSpriteManager();

            Dictionary<string, CardSuit> suits = new Dictionary<string, CardSuit>(){
                {"Clubs", CardSuit.CLUBS},
                {"Diamonds", CardSuit.DIAMONDS},
                {"Hearts", CardSuit.HEARTS},
                {"Spades", CardSuit.SPADES},
            };
            string[] values = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            foreach (var suit in suits)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    string path = "CardImages/card" + suit.Key + values[i];
                    Card card = new Card(suit.Value, i + 1);
                    Sprite sprite = Resources.Load<Sprite>(path);
                    Debug.Log("Loaded sprite " + path + ": " + sprite);
                    CardSpriteManager.instance.cardFrontSprites[card.ToString()] = sprite;
                }
            }
            CardSpriteManager.instance.backOfCardSprite = Resources.Load<Sprite>("CardImages/cardBack_red5");
        }
        return CardSpriteManager.instance;
    }

    public Sprite GetSpriteForCard(Card card)
    {
        Debug.Log("Looking up sprite for " + card.ToString());
        Sprite sprite = cardFrontSprites[card.ToString()];
        Debug.Log("Got Sprite " + sprite);
        return sprite;
    }

    public Sprite GetFaceDownSprite()
    {
        return backOfCardSprite;
    }
}

public class CardBehaviour : MonoBehaviour
{
    public Card card;

    public bool faceUp = true;

    void Start()
    {
        SetFaceUp(faceUp);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetFaceUp(bool isFaceUp)
    {
        faceUp = isFaceUp;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (faceUp)
        {
            sr.sprite = CardSpriteManager.Load().GetSpriteForCard(card);
        }
        else
        {
            sr.sprite = CardSpriteManager.Load().GetFaceDownSprite();
        }
    }

    void OnMouseUpAsButton()
    {
        DeckBehaviour deck = GetComponentInParent<DeckBehaviour>();
        if (deck != null)
        {
            deck.FlipCardOver(this);
        }
        Debug.Log("You clicked me! " + card.ToString());
    }

}
