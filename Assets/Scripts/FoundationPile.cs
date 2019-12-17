using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FoundationPile
{
    public List<Card> Cards = new List<Card>();
    public readonly int PileIndex;

    public FoundationPile(int pileIndex)
    {
        PileIndex = pileIndex;
    }

    public Location Push(Card card)
    {
        var destination = GetNextCardLocation();
        Cards.Add(card);
        return destination;
    }
    public (Card card, Location source) Pop()
    {
        var card = Cards[Cards.Count - 1];
        Cards.RemoveAt(Cards.Count - 1);
        return (card, GetNextCardLocation());
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (Cards.Count == 0)
        {
            return card.Rank == Rank.ACE;
        }
        var lastCard = Cards[Cards.Count - 1];
        return lastCard.Rank == card.Rank - 1 && lastCard.Suit == card.Suit;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.FOUNDATION, PileIndex, Cards.Count, true);
    }

    public (Card card, Location source) Peek()
    {
        return (
            Cards[Cards.Count - 1],
            new Location(PileType.FOUNDATION, PileIndex, Cards.Count - 1, true)
        );
    }
}