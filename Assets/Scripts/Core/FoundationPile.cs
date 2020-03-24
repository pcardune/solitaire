using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FoundationPile : CardPile
{

    public FoundationPile(int pileIndex) : base(PileType.FOUNDATION, pileIndex, 0) { }

    public Location Push(Card card)
    {
        var destination = GetDropCardLocation();
        Add(card);
        return destination;
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (Count == 0)
        {
            return card.Rank == Rank.ACE;
        }
        var lastCard = this[Count - 1];
        return lastCard.Rank == card.Rank - 1 && lastCard.Suit == card.Suit;
    }
}