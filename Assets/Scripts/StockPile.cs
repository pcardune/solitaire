using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StockPile
{
    public List<Card> stock = Deck.GetShuffledDeck();
    public List<Card> waste = new List<Card>();

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
        Debug.Log($"Before popping, stock had {stock.Count} cards.");
        var card = stock[stock.Count - 1];
        stock.RemoveAt(stock.Count - 1);
        Debug.Log($"After popping, stock has {stock.Count} cards");
        return (card, new Location(PileType.STOCK, 0, stock.Count, false));
    }

    public (Card Card, Location Source) PopFromWaste()
    {
        var card = waste[waste.Count - 1];
        waste.RemoveAt(waste.Count - 1);
        return (card, new Location(PileType.WASTE, 0, stock.Count, true));
    }

    public Location PushOntoWaste(Card card)
    {
        waste.Add(card);
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
            yield return (waste[waste.Count - 1], new Location(PileType.WASTE, 0, waste.Count - 1, true));
        }
        if (stock.Count > 0)
        {
            yield return (stock[stock.Count - 1], new Location(PileType.STOCK, 0, stock.Count - 1, false));
        }
    }
}