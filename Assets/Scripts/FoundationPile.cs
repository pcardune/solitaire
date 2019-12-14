using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationPile
{
    public Stack<Card> Cards = new Stack<Card>();
    public readonly int PileIndex;

    public FoundationPile(int pileIndex)
    {
        PileIndex = pileIndex;
    }

    public Location Push(Card card)
    {
        var destination = GetNextCardLocation();
        Cards.Push(card);
        return destination;
    }
    public (Card card, Location source) Pop()
    {
        return (Cards.Pop(), GetNextCardLocation());
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (Cards.Count == 0)
        {
            return card.Rank == Rank.ACE;
        }
        var lastCard = Cards.Peek();
        return lastCard.Rank == card.Rank - 1 && lastCard.Suit == card.Suit;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.FOUNDATION, PileIndex, Cards.Count, true);
    }

    public (Card card, Location source) Peek()
    {
        return (
            Cards.Peek(),
            new Location(PileType.FOUNDATION, PileIndex, Cards.Count - 1, true)
        );
    }
}