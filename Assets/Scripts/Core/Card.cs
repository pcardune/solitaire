using System;

// TODO: ditch UnityEngine dependency (used for Range decorator)
using UnityEngine;

[Serializable]
public struct Card
{
    public Suit Suit;
    public CardColor Color;

    [Range(1, 13)]
    public int Rank;

    public Card(Suit aSuit, int aRank)
    {
        if (aRank < 1 || aRank > 13)
        {
            throw new ArgumentOutOfRangeException("aRank", aRank, "Card rank must be between 1 and 13 inclusive.");
        }
        Suit = aSuit;
        Rank = aRank;
        if (Suit == Suit.Spades || Suit == Suit.Clubs)
        {
            Color = CardColor.BLACK;
        }
        else
        {
            Color = CardColor.RED;
        }
    }

    override public string ToString()
    {
        return Rank + " of " + Suit.ToString();
    }

    public string Id
    {
        get
        {
            return Rank + "-" + (int)Suit;
        }
    }

    override public int GetHashCode()
    {
        return ToByte();
    }

    public byte ToByte()
    {
        return (byte)((int)Suit * 13 + Rank);
    }

    public bool Equals(Card other)
    {
        return ToByte() == other.ToByte();
    }

    public static Card FromByte(byte b)
    {
        if (b < 1 || b > 52)
        {
            throw new ArgumentOutOfRangeException("b", b, "valid card bytes should be between 1 and 52 inclusive.");
        }
        return new Card((Suit)((b - 1) / 13), (b - 1) % 13 + 1);
    }
}