using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockPile
{
    public Stack<Card> stock = Deck.GetShuffledDeck();
    public Stack<Card> waste = new Stack<Card>();

    public CardMovement TurnOverFromStock()
    {
        if (stock.Count > 0)
        {
            (var topCard, var source) = PopFromStock();
            var destination = PushOntoWaste(topCard);
            return new CardMovement(topCard, source, destination);
        }
        return null;
    }

    public (Card Card, Location Source) PopFromStock()
    {
        return (stock.Pop(), new Location(PileType.STOCK, 0, stock.Count, false));
    }

    public (Card Card, Location Source) PopFromWaste()
    {
        return (waste.Pop(), new Location(PileType.WASTE, 0, stock.Count, true));
    }

    public Location PushOntoWaste(Card card)
    {
        waste.Push(card);
        return new Location(PileType.WASTE, 0, waste.Count - 1, true);
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.WASTE, 0, waste.Count, true);
    }

    public IEnumerable<(Card card, Location source)> GetMovableCards()
    {
        if (waste.Count > 0)
        {
            yield return (waste.Peek(), new Location(PileType.WASTE, 0, waste.Count - 1, true));
        }
        if (stock.Count > 0)
        {
            yield return (stock.Peek(), new Location(PileType.STOCK, 0, stock.Count - 1, false));
        }
    }
}