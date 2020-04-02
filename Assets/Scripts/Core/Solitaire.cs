using System;
using System.Collections.Generic;
using System.Data.Linq;
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
    public PileType pileType;
    public int pileIndex;
    public int order;

    public bool faceUp;

    public Location(PileType pileType, int pileIndex, int order, bool faceUp)
    {
        this.pileType = pileType;
        this.pileIndex = pileIndex;
        this.order = order;
        this.faceUp = faceUp;
    }

    override public string ToString()
    {
        return "" + pileType.ToString() + "_" + (pileIndex + 1) + "[" + (order) + "]";
    }

    public bool Equals(Location other)
    {
        return pileType == other.pileType && pileIndex == other.pileIndex && order == other.order && faceUp == other.faceUp;
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

// Keep track of how many cards should be drawn from the stock at a time.
public enum DrawType
{
    SingleCardDraw,
    ThreeCardDraw
}

public class ScoredMove
{
    public int Score;
    public CardMovement Move;

    public ScoredMove(CardMovement move, int score)
    {
        Move = move;
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

[Serializable]
public class Solitaire
{
    public StockAndWastePile stockPile;

    public List<FoundationPile> foundations = new List<FoundationPile>();
    public List<TableauPile> tableauPiles = new List<TableauPile>();

    public DrawType drawType = DrawType.SingleCardDraw;

    [NonSerialized]
    List<CardMovement> possibleMovesCache;
    [NonSerialized]
    CardMovement randomMoveCache;

    public List<CardMovement> moveHistory = new List<CardMovement>();
    public Dictionary<Binary, HashSet<CardMovement>> visitedStates = new Dictionary<Binary, HashSet<CardMovement>>();
    private PackedSolitaire packedState;
    public int RandomSeed { get; private set; }

    public Solitaire(int randomSeed, DrawType drawType = DrawType.SingleCardDraw)
    {
        RandomSeed = randomSeed;
        this.drawType = drawType;
        stockPile = new StockAndWastePile(RandomSeed);
        for (int i = 0; i < 4; i++)
        {
            foundations.Add(new FoundationPile(i));
        }
        for (int i = 0; i < 7; i++)
        {
            tableauPiles.Add(new TableauPile(i));
        }
    }

    public IEnumerable<CardMovement> Deal()
    {
        for (int i = 0; i < tableauPiles.Count; i++)
        {
            TableauPile pile = tableauPiles[i];
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
        packedState = new PackedSolitaire(this);
        visitedStates.Add(packedState.data, new HashSet<CardMovement>());
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
        foreach (var pile in tableauPiles) { yield return pile; }
    }

    public List<CardMovement> GetPossibleMovesForCard(LocatedCard locatedCard)
    {
        // Debug.Log("Checking possible moves for " + locatedCard.Card.ToString() + " at " + locatedCard.Location);
        List<CardMovement> moves = new List<CardMovement>();
        if (locatedCard.Location.pileType == PileType.STOCK)
        {
            moves.Add(new CardMovement(locatedCard, stockPile.waste.GetDropCardLocation()));
        }
        else if (locatedCard.Location.faceUp)
        {
            foreach (var pile in tableauPiles)
            {
                if (pile.CanPushCardOntoPile(locatedCard.Card))
                {
                    var move = new CardMovement(locatedCard, pile.GetNextCardLocation());
                    moves.Add(move);
                }
            }
            bool maybeFoundation = true;
            if (locatedCard.Location.pileType == PileType.FOUNDATION)
            {
                maybeFoundation = false;
            }
            if (locatedCard.Location.pileType == PileType.TABLEAU && tableauPiles[locatedCard.Location.pileIndex].GetNextCardLocation().order - 1 != locatedCard.Location.order)
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
        // Debug.Log("  ---> Found " + moves.Count + " possible moves for " + locatedCard.Card);
        return moves;
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

            foreach (var pile in tableauPiles)
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

    public bool MaybePerformMove(CardMovement move)
    {
        if (move.type == MoveType.StockPileReset)
        {
            stockPile.Reset();
            return true;
        }
        else if (move.source.pileType == PileType.STOCK)
        {
            // you can only move from the stock to the waste
            var topCard = stockPile.stock.Pop();
            stockPile.waste.Add(topCard.Card);
            if (drawType == DrawType.ThreeCardDraw)
            {
                for (int i = 0; i < 2 && stockPile.stock.Count > 0; i++)
                {
                    stockPile.waste.Add(stockPile.stock.Pop().Card);
                }
            }
            return true;
        }
        else if (move.source.pileType == PileType.WASTE)
        {
            if (move.destination.pileType == PileType.TABLEAU)
            {
                stockPile.waste.Pop();
                tableauPiles[move.destination.pileIndex].PushFaceUp(move.card);
                return true;
            }
            if (move.destination.pileType == PileType.FOUNDATION)
            {
                stockPile.waste.Pop();
                foundations[move.destination.pileIndex].Push(move.card);
                return true;
            }
        }
        else if (move.source.pileType == PileType.FOUNDATION)
        {
            if (move.destination.pileType == PileType.TABLEAU)
            {
                foundations[move.source.pileIndex].Pop();
                tableauPiles[move.destination.pileIndex].PushFaceUp(move.card);
                return true;
            }
            else if (move.destination.pileType == PileType.FOUNDATION)
            {
                foundations[move.source.pileIndex].Pop();
                foundations[move.destination.pileIndex].Push(move.card);
                return true;
            }
        }
        else if (move.source.pileType == PileType.TABLEAU)
        {
            if (move.destination.pileType == PileType.FOUNDATION)
            {
                tableauPiles[move.source.pileIndex].PopAllAfter(move.source.order);
                foundations[move.destination.pileIndex].Push(move.card);
                return true;

            }
            else if (move.destination.pileType == PileType.TABLEAU)
            {
                (var cards, var flippedCard) = tableauPiles[move.source.pileIndex].PopAllAfter(move.source.order);
                tableauPiles[move.destination.pileIndex].PushAllOnto(cards);
                return true;
            }
        }
        return false;
    }

    public bool PerformMove(CardMovement move)
    {
        if (packedState == null)
        {
            packedState = new PackedSolitaire(this);
        }
        // Debug.Log("Moving from state: " + packedState.data);
        HashSet<CardMovement> previouslyAttemptedMoves;
        if (visitedStates.TryGetValue(packedState.data, out previouslyAttemptedMoves))
        {
            // Debug.Log("Found " + previouslyAttemptedMoves.Count + " previous moves from here.");
        }
        else
        {
            // Debug.Log("No previous moves have been made from here");
            previouslyAttemptedMoves = new HashSet<CardMovement>();
            visitedStates.Add(packedState.data, previouslyAttemptedMoves);
        }

        // Debug.Log("Searching for " + move + " in previously attempted moves");
        if (previouslyAttemptedMoves.Contains(move))
        {
            // Debug.LogWarning("This move was previously attempted " + move);
        }
        else
        {
            // Debug.Log("This move has not been done before");
        }

        bool success = MaybePerformMove(move);
        if (success)
        {
            possibleMovesCache = null;
            randomMoveCache = null;
            moveHistory.Add(move);
            previouslyAttemptedMoves.Add(move);
            packedState = new PackedSolitaire(this);
            if (!visitedStates.ContainsKey(packedState.data))
            {
                visitedStates.Add(packedState.data, new HashSet<CardMovement>());
            }
        }
        else
        {
            // Debug.LogWarning("Failed to perform move " + move);
        }
        return success;
    }

    public IEnumerable<CardMovement> PerformSmartMoves(IMoveSelector moveSelector, int numMovesToPerform, out bool success)
    {
        Debug.unityLogger.logEnabled = false;
        var moves = new List<CardMovement>();
        success = true;
        for (int i = 0; i < numMovesToPerform && success && !IsGameOver(); i++)
        {
            var move = moveSelector.GetMove(this);
            success = PerformMove(move);
            if (success)
            {
                moves.Add(move);
            }
        }
        Debug.unityLogger.logEnabled = true;
        return moves;
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
        foreach (var pile in tableauPiles)
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

    public byte[] ToBytes()
    {
        return SolitairePacker.Pack(this);
    }

    public static Solitaire FromBytes(byte[] bytes)
    {
        return SolitairePacker.Unpack(bytes);
    }

    public bool IsGameOver()
    {
        if (stockPile.stock.Count + stockPile.waste.Count > 0)
        {
            return false;
        }
        foreach (var pile in tableauPiles)
        {
            if (pile.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

}