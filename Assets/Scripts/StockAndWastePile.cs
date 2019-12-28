using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StockAndWastePile
{
    public CardPile stock = new CardPile(PileType.STOCK, 0, int.MaxValue);
    public CardPile waste = new CardPile(PileType.WASTE, 0, 0);

    public StockAndWastePile(int randomSeed)
    {
        stock.AddRange(Deck.GetShuffledDeck(randomSeed));
    }

    public IEnumerable<LocatedCard> GetMovableCards()
    {
        if (waste.Count > 0)
        {
            yield return new LocatedCard(waste[waste.Count - 1], new Location(PileType.WASTE, 0, waste.Count - 1, true));
        }
        if (stock.Count > 0)
        {
            yield return new LocatedCard(stock[stock.Count - 1], new Location(PileType.STOCK, 0, stock.Count - 1, false));
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