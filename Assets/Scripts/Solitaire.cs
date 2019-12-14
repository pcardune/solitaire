using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit
{
    Clubs,
    Hearts,
    Diamonds,
    Spades,
}

public enum CardColor
{
    RED,
    BLACK
}

public class Rank
{
    public const int KING = 13;
    public const int QUEEN = 12;
    public const int JACK = 11;
    public const int ACE = 1;
}

[System.Serializable]
public struct Card
{
    public Suit Suit;
    public CardColor Color;

    [Range(1, 13)]
    public int Rank;

    public Card(Suit aSuit, int aValue)
    {
        Suit = aSuit;
        Rank = aValue;
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
}

public class Deck
{
    public static Stack<Card> GetShuffledDeck()
    {
        Stack<Card> shuffledCards = new Stack<Card>();
        List<Card> cards = new List<Card>();
        foreach (var suit in (Suit[])Enum.GetValues(typeof(Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                cards.Add(new Card(suit, value));
            }
        }
        System.Random random = new System.Random(1);
        while (cards.Count > 0)
        {
            int cardIndex = random.Next(0, cards.Count);
            shuffledCards.Push(cards[cardIndex]);
            cards.RemoveAt(cardIndex);
        }
        return shuffledCards;
    }
}



public class Tableau
{
    public List<TableauPile> piles = new List<TableauPile>();
    public Tableau()
    {
        for (int i = 0; i < 7; i++)
        {
            piles.Add(new TableauPile(i));
        }
    }
}

public enum PileType
{
    STOCK,
    WASTE,
    TABLEAU,
    FOUNDATION,
}

[System.Serializable]
public struct Location
{
    public PileType PileType;
    public int PileIndex;
    public int Order;

    public bool FaceUp;

    public Location(PileType pileType, int pileIndex, int order, bool faceUp)
    {
        PileType = pileType;
        PileIndex = pileIndex;
        Order = order;
        FaceUp = faceUp;
    }

    override public string ToString()
    {
        return "" + PileType.ToString() + "_" + (PileIndex + 1) + "[" + (Order + 1) + "]";
    }
}

public class CardMovement
{
    public Card Card;
    public Location Source;
    public Location Destination;

    public CardMovement(Card card, Location source, Location destination)
    {
        Card = card;
        Source = source;
        Destination = destination;
    }

    override public string ToString()
    {
        return "CardMovement(" + Card.ToString() + ", " + Source.ToString() + ", " + Destination.ToString() + ")";
    }
}

public class Solitaire
{
    public StockPile stockPile = new StockPile();
    public Tableau tableau = new Tableau();

    public List<FoundationPile> foundations = new List<FoundationPile>();

    List<CardMovement> possibleMovesCache;

    public List<CardMovement> moveHistory = new List<CardMovement>();

    public Solitaire()
    {
        for (int i = 0; i < 4; i++)
        {
            foundations.Add(new FoundationPile(i));
        }
    }

    public List<CardMovement> Deal()
    {
        List<CardMovement> moves = new List<CardMovement>();
        for (int i = 0; i < tableau.piles.Count; i++)
        {
            TableauPile pile = tableau.piles[i];
            for (int j = 0; j < i + 1; j++)
            {
                (Card card, Location source) = stockPile.PopFromStock();
                Location destination;
                if (j < i)
                {
                    destination = pile.PushFaceDown(card);
                }
                else
                {
                    destination = pile.PushFaceUp(card);
                }
                var move = new CardMovement(card, source, destination);
                Debug.Log("Move: " + move.ToString());
                moves.Add(move);
            }
        }
        return moves;
    }

    public List<CardMovement> GetPossibleMovesForCard(Card card, Location source)
    {
        Debug.Log("Checking possible moves for " + card.ToString() + " at " + source);
        List<CardMovement> moves = new List<CardMovement>();
        if (source.PileType == PileType.STOCK)
        {
            moves.Add(new CardMovement(card, source, stockPile.GetNextCardLocation()));
        }
        else if (source.FaceUp)
        {
            foreach (var pile in tableau.piles)
            {
                if (pile.CanPushCardOntoPile(card))
                {
                    var move = new CardMovement(card, source, pile.GetNextCardLocation());
                    moves.Add(move);
                }
            }
            if (source.PileType != PileType.FOUNDATION)
            {
                foreach (var pile in foundations)
                {
                    if (pile.CanPushCardOntoPile(card))
                    {
                        var move = new CardMovement(card, source, pile.GetNextCardLocation());
                        moves.Add(move);
                    }
                }
            }

        }
        Debug.Log("  ---> Found " + moves.Count + " possible moves for " + card);
        return moves;
    }

    public List<CardMovement> GetPossibleMovesForCard((Card card, Location source) cardFromSource)
    {
        return GetPossibleMovesForCard(cardFromSource.card, cardFromSource.source);
    }

    public List<CardMovement> GetAllPossibleMoves()
    {
        if (possibleMovesCache == null)
        {
            List<CardMovement> moves = new List<CardMovement>();
            foreach (var pile in foundations)
            {
                if (pile.Cards.Count > 0)
                {
                    moves.AddRange(GetPossibleMovesForCard(pile.Peek()));
                }
            }

            foreach (var pile in tableau.piles)
            {
                foreach (var movableCard in pile.GetMovableCards())
                {
                    moves.AddRange(GetPossibleMovesForCard(movableCard));
                }
            }

            foreach (var movableCard in stockPile.GetMovableCards())
            {
                moves.AddRange(GetPossibleMovesForCard(movableCard));
            }
            Debug.Log("Found " + moves.Count + " possible moves");
            possibleMovesCache = moves;
        }
        return possibleMovesCache;
    }

    public bool MaybePerformMove(CardMovement move)
    {
        if (move.Source.PileType == PileType.STOCK)
        {
            // you can only move from the stock to the waste
            stockPile.TurnOverFromStock();
            return true;
        }
        else if (move.Source.PileType == PileType.WASTE)
        {
            if (move.Destination.PileType == PileType.TABLEAU)
            {
                stockPile.PopFromWaste();
                tableau.piles[move.Destination.PileIndex].PushFaceUp(move.Card);
                return true;
            }
            if (move.Destination.PileType == PileType.FOUNDATION)
            {
                stockPile.PopFromWaste();
                foundations[move.Destination.PileIndex].Push(move.Card);
                return true;
            }
        }
        else if (move.Source.PileType == PileType.FOUNDATION)
        {
            if (move.Destination.PileType == PileType.TABLEAU)
            {
                foundations[move.Source.PileIndex].Pop();
                tableau.piles[move.Destination.PileIndex].PushFaceUp(move.Card);
                return true;
            }
            else if (move.Destination.PileType == PileType.FOUNDATION)
            {
                foundations[move.Source.PileIndex].Pop();
                foundations[move.Destination.PileIndex].Push(move.Card);
                return true;
            }
        }
        else if (move.Source.PileType == PileType.TABLEAU)
        {
            if (move.Destination.PileType == PileType.FOUNDATION)
            {
                tableau.piles[move.Source.PileIndex].PopAllAfter(move.Source.Order);
                foundations[move.Destination.PileIndex].Push(move.Card);
                return true;

            }
            else if (move.Destination.PileType == PileType.TABLEAU)
            {
                (var cards, var flippedCard) = tableau.piles[move.Source.PileIndex].PopAllAfter(move.Source.Order);
                tableau.piles[move.Destination.PileIndex].PushAllOnto(cards);
                return true;
            }
        }
        return false;
    }

    public bool PerformMove(CardMovement move)
    {
        bool success = MaybePerformMove(move);
        if (success)
        {
            possibleMovesCache = null;
            moveHistory.Add(move);
        }
        else
        {
            Debug.LogWarning("Failed to perform move " + move);
        }
        return success;
    }

}