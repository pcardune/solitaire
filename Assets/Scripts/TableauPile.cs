using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TableauPile : CardPile
{
    public TableauPile(int pileIndex) : base(PileType.TABLEAU, pileIndex, pileIndex) { }

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
        if (Count == 0)
        {
            return card.Rank == Rank.KING;
        }
        var lastCard = this[Count - 1];
        return lastCard.Rank == card.Rank + 1 && lastCard.Color != card.Color;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.TABLEAU, PileIndex, Count, true);
    }

    public (List<Card> poppedCards, Card? flippedCard) PopAllAfter(int index)
    {
        var cards = GetRange(index, Count - index);
        RemoveRange(index, Count - index);
        if (Count > 0)
        {
            _faceDownCount = Math.Min(_faceDownCount, Count - 1);
        }
        return (cards, null);
    }

    public void PushAllOnto(List<Card> cards)
    {
        foreach (var card in cards)
        {
            PushFaceUp(card);
        }
    }

    public IEnumerable<LocatedCard> GetMovableCards()
    {
        for (int i = _faceDownCount; i < Count; i++)
        {
            yield return new LocatedCard(this[i], new Location(PileType.TABLEAU, PileIndex, i, true));
        }
    }
}