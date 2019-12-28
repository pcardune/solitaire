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

[Serializable]
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

    public string Id
    {
        get
        {
            return Rank + "-" + Suit;
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

    public static Card FromByte(byte b)
    {
        return new Card((Suit)(b / 13), b % 13);
    }
}

public class Deck
{
    public static List<Card> GetShuffledDeck(int randomSeed)
    {
        List<Card> shuffledCards = new List<Card>();
        List<Card> cards = new List<Card>();
        foreach (var suit in (Suit[])Enum.GetValues(typeof(Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                cards.Add(new Card(suit, value));
            }
        }
        System.Random random = new System.Random(randomSeed);
        while (cards.Count > 0)
        {
            int cardIndex = random.Next(0, cards.Count);
            shuffledCards.Add(cards[cardIndex]);
            cards.RemoveAt(cardIndex);
        }
        return shuffledCards;
    }
}


[Serializable]
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

[Serializable]
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
        return "" + PileType.ToString() + "_" + (PileIndex + 1) + "[" + (Order) + "]";
    }
}

[Serializable]
public struct LocatedCard
{
    public Card Card;
    public Location Location;

    public LocatedCard(Card card, Location location)
    {
        Card = card;
        Location = location;
    }

    public (Card Card, Location Location) AsTuple()
    {
        return (Card, Location);
    }
}

public enum MoveType
{
    SingleCard,
    StockPileReset,
}
public class CardMovement
{
    public Card Card;
    public Location Source;
    public Location Destination;

    public MoveType Type;

    public CardMovement(Card card, Location source, Location destination, MoveType type = MoveType.SingleCard)
    {
        Card = card;
        Source = source;
        Destination = destination;
        Type = type;
    }

    public CardMovement(LocatedCard locatedCard, Location destination, MoveType type = MoveType.SingleCard)
    {
        Card = locatedCard.Card;
        Source = locatedCard.Location;
        Destination = destination;
        Type = type;
    }

    override public string ToString()
    {
        return "CardMovement(" + Card.ToString() + ", " + Source.ToString() + ", " + Destination.ToString() + ")";
    }
}
public class ScoredCardMovement : CardMovement
{
    public int Score;
    public ScoredCardMovement(LocatedCard locatedCard, Location destination, int score, MoveType type = MoveType.SingleCard) : base(locatedCard, destination, type: type)
    {
        Score = score;
    }
}

[Serializable]
public class SolitaireJSON
{
    public List<string> waste = new List<string>();
    public List<string> stock = new List<string>();
    public List<List<string>> tableauFaceDown = new List<List<string>>();
    public List<List<string>> tableauFaceUp = new List<List<string>>();
    public List<List<string>> foundations = new List<List<string>>();
}

public class SolitairePacker
{
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
            var pile = solitaire.tableau.piles[j];
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
            var pile = solitaire.tableau.piles[j];
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

    public static byte[] Pack(Solitaire solitaire)
    {
        // slots:
        //   foundation: 13 * 4 = 52
        //   tableau face down: 0+1+2+3+4+5+6 = 21
        //   tableau face up = 13*7 = 91
        //   waste = 52 - 28 = 24
        //   stock = 52 - 28 = 24
        // total slots: 212
        byte[] slots = new byte[219];
        int i = 0;

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
            var pile = solitaire.tableau.piles[j];
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
            var pile = solitaire.tableau.piles[j];
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

        return slots;
    }
}

[Serializable]
public class Solitaire
{
    public StockAndWastePile stockPile;
    public Tableau tableau = new Tableau();

    public List<FoundationPile> foundations = new List<FoundationPile>();

    List<CardMovement> possibleMovesCache;
    CardMovement randomMoveCache;
    CardMovement smartMoveCache;

    public List<CardMovement> moveHistory = new List<CardMovement>();
    public int RandomSeed { get; private set; }

    public Solitaire(int randomSeed)
    {
        RandomSeed = randomSeed;
        stockPile = new StockAndWastePile(RandomSeed);
        for (int i = 0; i < 4; i++)
        {
            foundations.Add(new FoundationPile(i));
        }
    }

    public IEnumerable<CardMovement> Deal()
    {
        for (int i = 0; i < tableau.piles.Count; i++)
        {
            TableauPile pile = tableau.piles[i];
            for (int j = 0; j < i + 1; j++)
            {
                var topCard = stockPile.stock.Pop();
                Location destination;
                if (j < i)
                {
                    destination = pile.PushFaceDown(topCard.Card);
                }
                else
                {
                    destination = pile.PushFaceUp(topCard.Card);
                }
                var move = new CardMovement(topCard, destination);
                yield return move;
            }
        }
    }

    public List<CardMovement> DealAll()
    {
        return new List<CardMovement>(Deal());
    }

    public IEnumerable<LocatedCard> AllCards()
    {
        foreach (var pile in AllPiles())
        {
            foreach (var locatedCard in pile.LocatedCards())
            {
                yield return locatedCard;
            }
        }
    }

    public IEnumerable<CardPile> AllPiles()
    {
        yield return stockPile.stock;
        yield return stockPile.waste;
        foreach (var pile in foundations) { yield return pile; }
        foreach (var pile in tableau.piles) { yield return pile; }
    }

    public List<CardMovement> GetPossibleMovesForCard(LocatedCard locatedCard)
    {
        Debug.Log("Checking possible moves for " + locatedCard.Card.ToString() + " at " + locatedCard.Location);
        List<CardMovement> moves = new List<CardMovement>();
        if (locatedCard.Location.PileType == PileType.STOCK)
        {
            moves.Add(new CardMovement(locatedCard, stockPile.waste.GetDropCardLocation()));
        }
        else if (locatedCard.Location.FaceUp)
        {
            foreach (var pile in tableau.piles)
            {
                if (pile.CanPushCardOntoPile(locatedCard.Card))
                {
                    var move = new CardMovement(locatedCard, pile.GetNextCardLocation());
                    moves.Add(move);
                }
            }
            bool maybeFoundation = true;
            if (locatedCard.Location.PileType == PileType.FOUNDATION)
            {
                maybeFoundation = false;
            }
            if (locatedCard.Location.PileType == PileType.TABLEAU && tableau.piles[locatedCard.Location.PileIndex].GetNextCardLocation().Order - 1 != locatedCard.Location.Order)
            {
                maybeFoundation = false;
            }
            if (maybeFoundation)
            {
                foreach (var pile in foundations)
                {
                    if (pile.CanPushCardOntoPile(locatedCard.Card))
                    {
                        var move = new CardMovement(locatedCard, pile.GetDropCardLocation());
                        moves.Add(move);
                    }
                }
            }

        }
        Debug.Log("  ---> Found " + moves.Count + " possible moves for " + locatedCard.Card);
        return moves;
    }

    public int GetScoreForMove(CardMovement move)
    {
        // higher score means better move

        // it's always a good idea to move aces onto the foundation
        if (move.Card.Rank == Rank.ACE && move.Destination.PileType == PileType.FOUNDATION)
        {
            return 10;
        }
        // it's always good to uncover cards in the tableau
        if (move.Source.PileType == PileType.TABLEAU && move.Source.Order > 0 && move.Source.Order == tableau.piles[move.Source.PileIndex].FaceDownCount)
        {
            return 10;
        }

        // it's always good to move a card onto the tableau from the waste if it allows a new card to be uncovered
        if (move.Destination.PileType == PileType.TABLEAU && move.Source.PileType == PileType.WASTE)
        {
            foreach (var pile in tableau.piles)
            {
                if (pile.Count > 0 && pile.FaceDownCount < pile.Count)
                {
                    var faceUpCard = pile[pile.FaceDownCount];
                    if (move.Card.Color != faceUpCard.Color && move.Card.Rank == faceUpCard.Rank + 1)
                    {
                        return 9;
                    }
                }
            }
        }

        // it's typically good to move cards onto the foundation
        if (move.Destination.PileType == PileType.FOUNDATION)
        {
            return 7;
        }

        // it's useless to move aces off the foundation
        if (move.Source.PileType == PileType.FOUNDATION && move.Card.Rank == Rank.ACE)
        {
            return -1;
        }

        // it's useless to move kings in the tableau when they are already at order 0
        if (move.Card.Rank == Rank.KING && move.Source.PileType == PileType.TABLEAU && move.Source.Order == 0 && move.Destination.PileType == PileType.TABLEAU)
        {
            return -1;
        }

        return 0;
    }

    public List<CardMovement> GetAllPossibleMoves()
    {
        if (possibleMovesCache == null)
        {
            List<CardMovement> moves = new List<CardMovement>();
            foreach (var pile in foundations)
            {
                if (pile.Count > 0)
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

            if (stockPile.CanReset())
            {
                moves.Add(stockPile.GetResetMove());
            }
            Debug.Log("Found " + moves.Count + " possible moves");
            possibleMovesCache = moves;
        }
        return possibleMovesCache;
    }

    public CardMovement GetRandomMove(System.Random random)
    {
        if (randomMoveCache == null)
        {
            var moves = GetAllPossibleMoves();
            randomMoveCache = moves[random.Next(0, moves.Count)];
        }
        return randomMoveCache;
    }

    public CardMovement GetSmartMove(System.Random random)
    {
        if (smartMoveCache == null)
        {
            List<CardMovement> movesToConsider = new List<CardMovement>();
            int maxScore = int.MinValue;
            var moves = GetAllPossibleMoves();
            foreach (var move in moves)
            {
                var score = GetScoreForMove(move);
                if (score == maxScore)
                {
                    movesToConsider.Add(move);
                }
                else if (score > maxScore)
                {
                    movesToConsider.Clear();
                    movesToConsider.Add(move);
                    maxScore = score;
                }
            }
            smartMoveCache = movesToConsider[random.Next(0, movesToConsider.Count)];
        }
        return smartMoveCache;

    }

    public bool MaybePerformMove(CardMovement move)
    {
        if (move.Type == MoveType.StockPileReset)
        {
            stockPile.Reset();
            return true;
        }
        else if (move.Source.PileType == PileType.STOCK)
        {
            // you can only move from the stock to the waste
            var topCard = stockPile.stock.Pop();
            stockPile.waste.Add(topCard.Card);
            return true;
        }
        else if (move.Source.PileType == PileType.WASTE)
        {
            if (move.Destination.PileType == PileType.TABLEAU)
            {
                stockPile.waste.Pop();
                tableau.piles[move.Destination.PileIndex].PushFaceUp(move.Card);
                return true;
            }
            if (move.Destination.PileType == PileType.FOUNDATION)
            {
                stockPile.waste.Pop();
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
            randomMoveCache = null;
            smartMoveCache = null;
            moveHistory.Add(move);
        }
        else
        {
            Debug.LogWarning("Failed to perform move " + move);
        }
        return success;
    }


    public SolitaireJSON ToJSON()
    {
        var json = new SolitaireJSON();
        foreach (var card in stockPile.waste)
        {
            json.waste.Add(card.Id);
        }
        foreach (var card in stockPile.stock)
        {
            json.stock.Add(card.Id);
        }
        foreach (var pile in foundations)
        {
            List<string> cards = new List<string>();
            foreach (var card in pile)
            {
                cards.Add(card.Id);
            }
            json.foundations.Add(cards);
        }
        foreach (var pile in tableau.piles)
        {
            List<string> cards = new List<string>();
            int i = 0;
            for (; i < pile.FaceDownCount; i++)
            {
                cards.Add(pile[i].Id);
            }
            json.tableauFaceDown.Add(cards);

            cards = new List<string>();
            for (; i < pile.Count; i++)
            {
                cards.Add(pile[i].Id);
            }
            json.tableauFaceUp.Add(cards);
        }
        return json;
    }

    public bool IsGameOver()
    {
        if (stockPile.stock.Count + stockPile.waste.Count > 0)
        {
            return false;
        }
        foreach (var pile in tableau.piles)
        {
            if (pile.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

}