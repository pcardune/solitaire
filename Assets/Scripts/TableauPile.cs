using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TableauPile
{
    public List<Card> faceDownCards = new List<Card>();
    public List<Card> faceUpCards = new List<Card>();

    public readonly int PileIndex;

    public TableauPile(int pileIndex)
    {
        PileIndex = pileIndex;
    }

    public Location PushFaceDown(Card card)
    {
        faceDownCards.Add(card);
        return new Location(PileType.TABLEAU, PileIndex, faceDownCards.Count - 1, false);
    }

    public Location PushFaceUp(Card card)
    {
        var destination = GetNextCardLocation();
        faceUpCards.Add(card);
        return destination;
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (faceUpCards.Count == 0)
        {
            return faceDownCards.Count == 0 && card.Rank == Rank.KING;
        }
        var lastCard = faceUpCards[faceUpCards.Count - 1];
        return lastCard.Rank == card.Rank + 1 && lastCard.Color != card.Color;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.TABLEAU, PileIndex, faceDownCards.Count + faceUpCards.Count, true);
    }

    public (List<Card> poppedCards, Card? flippedCard) PopAllAfter(int combinedIndex)
    {
        var index = combinedIndex - faceDownCards.Count;
        var cards = faceUpCards.GetRange(index, faceUpCards.Count - index);
        faceUpCards.RemoveRange(index, faceUpCards.Count - index);
        Card? flippedCard = null;
        if (faceUpCards.Count == 0 && faceDownCards.Count > 0)
        {
            faceUpCards.Add(faceDownCards[faceDownCards.Count - 1]);
            faceDownCards.RemoveAt(faceDownCards.Count - 1);
        }
        return (cards, flippedCard);
    }

    public void PushAllOnto(List<Card> cards)
    {
        faceUpCards.AddRange(cards);
    }

    public IEnumerable<(Card card, Location source)> GetMovableCards()
    {
        for (int i = 0; i < faceUpCards.Count; i++)
        {
            yield return (faceUpCards[i], new Location(PileType.TABLEAU, PileIndex, faceDownCards.Count + i, true));
        }
    }
}