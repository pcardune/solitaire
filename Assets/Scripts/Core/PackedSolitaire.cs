using System.Data.Linq;
using UnityEngine;

public class PackedSolitaire
{
    public Binary data;

    public PackedSolitaire(Solitaire solitaire)
    {
        // slots:
        //   foundation: 13 * 4 = 52
        //   tableau face down: 0+1+2+3+4+5+6 = 21
        //   tableau face up = 13*7 = 91
        //   waste = 52 - 28 = 24
        //   stock = 52 - 28 = 24
        // total slots: 212
        byte[] slots = new byte[219];
        int i = 0; // slot index

        // pack foundation
        for (int j = 0; j < 4; j++)
        {
            var pile = solitaire.foundations[j];
            int k = 0;
            for (; k < pile.Count; k++)
            {
                slots[i++] = pile[k].ToByte();
            }
            for (; k < 13; k++)
            {
                slots[i++] = 0;
            }
        }

        // pack tableau face down
        for (int j = 1; j < 7; j++)
        {
            var pile = solitaire.tableauPiles[j];
            int k = 0;
            for (; k < pile.FaceDownCount; k++)
            {
                slots[i++] = pile[k].ToByte();
            }
            for (; k < j; k++)
            {
                slots[i++] = 0;
            }
        }

        // pack tableau face up
        for (int j = 0; j < 7; j++)
        {
            var pile = solitaire.tableauPiles[j];
            int k = pile.FaceDownCount;
            for (; k < pile.Count; k++)
            {
                slots[i++] = pile[k].ToByte();
            }
            for (; k < pile.FaceDownCount + 13; k++)
            {
                slots[i++] = 0;
            }
        }

        // pack waste
        {
            var pile = solitaire.stockPile.waste;
            int k = 0;
            for (; k < pile.Count; k++)
            {
                slots[i++] = pile[k].ToByte();
            }
            for (; k < 24; k++)
            {
                slots[i++] = 0;
            }
        }
        Debug.Log("Finished packing waste. byte offset: " + i);
        // pack stock
        {
            var pile = solitaire.stockPile.stock;
            int k = 0;
            for (; k < pile.Count; k++)
            {
                slots[i++] = pile[k].ToByte();
            }
            for (; k < 24; k++)
            {
                slots[i++] = 0;
            }
        }
        data = new Binary(slots);
    }

    public static Solitaire Unpack(byte[] bytes)
    {

        Solitaire solitaire = new Solitaire(1);
        int i = 0;

        // unpack foundation
        for (int j = 0; j < 4; j++)
        {
            var pile = solitaire.foundations[j];
            pile.Clear();
            for (int k = 0; k < 13; k++)
            {
                byte b = bytes[i++];
                if (b != 0)
                {
                    pile.Add(Card.FromByte(b));
                }
            }
        }

        // unpack tableau facedown
        for (int j = 1; j < 7; j++)
        {
            var pile = solitaire.tableauPiles[j];
            pile.Clear();
            for (int k = 0; k < j; k++)
            {
                byte b = bytes[i++];
                if (b != 0)
                {
                    pile.PushFaceDown(Card.FromByte(b));
                }
            }
        }

        // unpack tableau faceup
        for (int j = 0; j < 7; j++)
        {
            var pile = solitaire.tableauPiles[j];
            for (int k = 0; k < 13; k++)
            {
                byte b = bytes[i++];
                if (b != 0)
                {
                    pile.PushFaceUp(Card.FromByte(b));
                }
            }
        }

        // unpack waste
        {
            var pile = solitaire.stockPile.waste;
            pile.Clear();
            for (int k = 0; k < 24; k++)
            {
                byte b = bytes[i++];
                if (b != 0)
                {
                    pile.Add(Card.FromByte(b));
                }
            }
        }
        // unpack stock
        {
            var pile = solitaire.stockPile.stock;
            pile.Clear();
            for (int k = 0; k < 24; k++)
            {
                byte b = bytes[i++];
                if (b != 0)
                {
                    pile.Add(Card.FromByte(b));
                }
            }
        }

        return solitaire;
    }
}

public class SolitairePacker
{
    public static Solitaire Unpack(byte[] bytes)
    {
        return PackedSolitaire.Unpack(bytes);
    }

    public static byte[] Pack(Solitaire solitaire)
    {
        return new PackedSolitaire(solitaire).data.ToArray();
    }
}