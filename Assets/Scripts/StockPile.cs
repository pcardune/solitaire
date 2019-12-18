using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StockPile
{
    public List<Card> stock;
    public List<Card> waste = new List<Card>();

    public StockPile(int randomSeed)
    {
        stock = Deck.GetShuffledDeck(randomSeed);
    }

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
        var card = stock[stock.Count - 1];
        stock.RemoveAt(stock.Count - 1);
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

    public bool CanReset()
    {
        return waste.Count > 0 && stock.Count == 0;
    }

    public CardMovement GetResetMove()
    {
        return new CardMovement(
            waste[0],
            new Location(PileType.WASTE, 0, 0, true),
            new Location(PileType.STOCK, 0, waste.Count - 1, false),
            type: MoveType.StockPileReset
        );
    }

    public void Reset()
    {
        for (int i = waste.Count - 1; i >= 0; i--)
        {
            stock.Add(waste[i]);
        }
        waste.Clear();
    }
}