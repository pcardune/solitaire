using System;
using System.Collections.Generic;

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