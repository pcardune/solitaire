using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TableauPile : CardPile
{
    public TableauPile(int pileIndex) : base(PileType.TABLEAU, pileIndex, pileIndex) { }

    public IEnumerable<Card> FaceDownCards
    {
        get
        {
            for (int i = 0; i < FaceDownCount; i++)
            {
                yield return this[i];
            }
        }
    }

    public IEnumerable<Card> FaceUpCards
    {
        get
        {
            for (int i = FaceDownCount; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }

    public Location PushFaceDown(Card card)
    {
        Add(card);
        _faceDownCount = Count;
        return Peek().Location;
    }

    public Location PushFaceUp(Card card)
    {
        Add(card);
        _faceDownCount = Math.Min(_faceDownCount, Count - 1);
        return Peek().Location;
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (_faceDownCount == 0)
        {
            return card.Rank == Rank.KING;
        }
        var lastCard = Peek().Card;
        return lastCard.Rank == card.Rank + 1 && lastCard.Color != card.Color;
    }

    public Location GetNextCardLocation()
    {
        return GetDropCardLocation();
    }

    public List<Card> PopAllAfter(int combinedIndex)
    {
        var cards = GetRange(combinedIndex, Count - combinedIndex);
        RemoveRange(combinedIndex, Count - combinedIndex);
        if (_faceDownCount == Count)
        {
            _faceDownCount--;
        }
        return cards;
    }

    public void PushAllOnto(List<Card> cards)
    {
        AddRange(cards);
    }

    public IEnumerable<(Card card, Location source)> GetMovableCards()
    {
        foreach (var locatedCard in EnumerateLocatedCards())
        {
            if (locatedCard.Location.FaceUp)
            {
                yield return locatedCard.AsTuple();
            }
        }
    }
}