using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBehaviour : MonoBehaviour
{

    public CardBehaviour cardPrefab;
    public CardTargetBehaviour cardTarget;

    // Start is called before the first frame update
    void Awake()
    {
        int n = 0;
        foreach (var suit in (CardSuit[])Enum.GetValues(typeof(CardSuit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                CardBehaviour card = Instantiate<CardBehaviour>(cardPrefab);
                card.transform.parent = transform;
                card.transform.localPosition = Vector3.zero;
                card.card = new Card(suit, value);
                card.gameObject.name = card.card.ToString();
                card.SetFaceUp(false);
                card.GetComponent<SpriteRenderer>().sortingOrder = n++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public CardBehaviour GetTopCard()
    {
        return transform.GetChild(transform.childCount - 1).GetComponent<CardBehaviour>();
    }

    public void FlipCardOver(CardBehaviour card)
    {
        card.GetComponent<SpriteRenderer>().sortingOrder = 100;
        card.GetComponent<MoveBehaviour>().MoveTo(cardTarget.transform.position, .1f, cardTarget.transform.parent.childCount);
        card.transform.parent = cardTarget.transform.parent;
        card.SetFaceUp(true);
    }
}
