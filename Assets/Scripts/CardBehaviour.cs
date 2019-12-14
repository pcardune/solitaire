﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

            Dictionary<string, Suit> suits = new Dictionary<string, Suit>(){
                {"Clubs", Suit.Clubs},
                {"Diamonds", Suit.Diamonds},
                {"Hearts", Suit.Hearts},
                {"Spades", Suit.Spades},
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
        // Debug.Log("Looking up sprite for " + card.ToString());
        Sprite sprite = cardFrontSprites[card.ToString()];
        // Debug.Log("Got Sprite " + sprite);
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

    public Location cardLocation;

    public SolitaireGameBehaviour solitaireGameBehaviour;

    public bool faceUp = true;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

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
        if (faceUp)
        {
            spriteRenderer.sprite = CardSpriteManager.Load().GetSpriteForCard(card);
        }
        else
        {
            spriteRenderer.sprite = CardSpriteManager.Load().GetFaceDownSprite();
        }
    }

    void OnMouseUpAsButton()
    {
        solitaireGameBehaviour.OnClickCard(this);
    }

    void OnMouseDown()
    {
        solitaireGameBehaviour.OnMouseDownCard(this);
    }

    void OnMouseDrag()
    {
        solitaireGameBehaviour.OnMouseDragCard(this);
    }

    void OnMouseUp()
    {
        solitaireGameBehaviour.OnMouseUpCard(this);
    }

}