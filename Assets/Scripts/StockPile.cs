using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPile : List<Card>
{
    private PileType _pileType;
    private int _pileIndex;
    protected int _faceDownCount;

    public PileType PileType
    {
        get
        {
            return _pileType;
        }
    }

    public int PileIndex
    {
        get
        {
            return _pileIndex;
        }
    }
    public int FaceDownCount { get { return Math.Min(_faceDownCount, Count); } }

    public CardPile(PileType pileType, int pileIndex, int faceDownCount)
    {
        _pileType = pileType;
        _pileIndex = pileIndex;
        _faceDownCount = faceDownCount;
    }

    public LocatedCard Pop()
    {
        var c = Peek();
        RemoveAt(Count - 1);
        return c;
    }

    public LocatedCard Peek()
    {
        int order = Count - 1;
        var card = this[order];
        return new LocatedCard(card, new Location(PileType, PileIndex, order, order >= _faceDownCount));
    }

    /// <summary>
    /// The location of a card if we were to drop it onto this pile.
    /// </summary>
    public Location GetDropCardLocation()
    {
        return new Location(PileType, PileIndex, Count, Count >= _faceDownCount);
    }

    public IEnumerable<LocatedCard> LocatedCards()
    {
        for (int order = 0; order < Count; order++)
        {
            yield return new LocatedCard(this[order], new Location(PileType, PileIndex, order, order >= _faceDownCount));
        }
    }
}

[Serializable]
public class StockAndWastePile
{
    public CardPile stock = new CardPile(PileType.STOCK, 0, int.MaxValue);
    public CardPile waste = new CardPile(PileType.WASTE, 0, 0);

    public StockAndWastePile(int randomSeed)
    {
        stock.AddRange(Deck.GetShuffledDeck(randomSeed));
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