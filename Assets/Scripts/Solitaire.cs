using System;
using System.Collections;
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

[Serializable]
public class CardMovement : IEquatable<CardMovement>
{
    public Card card;
    public Location source;
    public Location destination;

    public MoveType type;

    public CardMovement(Card card, Location source, Location destination, MoveType type = MoveType.SingleCard)
    {
        this.card = card;
        this.source = source;
        this.destination = destination;
        this.type = type;
    }

    public CardMovement(LocatedCard locatedCard, Location destination, MoveType type = MoveType.SingleCard)
    {
        card = locatedCard.Card;
        source = locatedCard.Location;
        this.destination = destination;
        this.type = type;
    }

    public override bool Equals(object obj)
    {
        return obj is CardMovement && Equals((CardMovement)obj);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public bool Equals(CardMovement other)
    {
        return card.Equals(other.card) && source.Equals(other.source) && destination.Equals(other.destination) && type.Equals(other.type);
    }

    override public string ToString()
    {
        return "CardMovement(" + card.ToString() + ", " + source.ToString() + ", " + destination.ToString() + ")";
    }
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

[Serializable]
public class Solitaire
{
    public StockAndWastePile stockPile;

    public List<FoundationPile> foundations = new List<FoundationPile>();
    public List<TableauPile> tableauPiles = new List<TableauPile>();

    [NonSerialized]
    List<CardMovement> possibleMovesCache;
    [NonSerialized]
    CardMovement randomMoveCache;
    [NonSerialized]
    CardMovement smartMoveCache;

    public List<CardMovement> moveHistory = new List<CardMovement>();
    public Dictionary<Binary, HashSet<CardMovement>> visitedStates = new Dictionary<Binary, HashSet<CardMovement>>();
    private PackedSolitaire packedState;
    public int RandomSeed { get; private set; }

    public Solitaire(int randomSeed)
    {
        RandomSeed = randomSeed;
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

    public int GetScoreForMove(CardMovement move)
    {
        // higher score means better move

        // it's always a good idea to move aces onto the foundation
        if (move.card.Rank == Rank.ACE && move.destination.pileType == PileType.FOUNDATION)
        {
            return 10;
        }
        // it's always good to uncover cards in the tableau
        if (move.source.pileType == PileType.TABLEAU && move.source.order > 0 && move.source.order == tableauPiles[move.source.pileIndex].FaceDownCount)
        {
            return 10;
        }

        // it's always good to move a card onto the tableau from the waste if it allows a new card to be uncovered
        if (move.destination.pileType == PileType.TABLEAU && move.source.pileType == PileType.WASTE)
        {
            foreach (var pile in tableauPiles)
            {
                if (pile.Count > 0 && pile.FaceDownCount < pile.Count)
                {
                    var faceUpCard = pile[pile.FaceDownCount];
                    if (move.card.Color != faceUpCard.Color && move.card.Rank == faceUpCard.Rank + 1)
                    {
                        return 9;
                    }
                }
            }
        }

        // it's typically good to move cards onto the foundation
        if (move.destination.pileType == PileType.FOUNDATION)
        {
            return 7;
        }

        var numFaceDown = 0;
        foreach (var pile in tableauPiles)
        {
            numFaceDown += pile.FaceDownCount;
        }
        // once all cards have been revealed
        if (numFaceDown == 0)
        {
            // it's useless to move cards around among the same type of pile
            if (move.source.pileType == move.destination.pileType)
            {
                return 0;
            }
            // It's useless to move cards off the foundation once all cards have been revealed
            if (move.source.pileType == PileType.FOUNDATION)
            {
                return 0;
            }
        }

        // it's useless to move aces off the foundation
        if (move.source.pileType == PileType.FOUNDATION && move.card.Rank == Rank.ACE)
        {
            return 0;
        }

        // it's useless to move kings in the tableau when they are already at order 0
        if (move.card.Rank == Rank.KING && move.source.pileType == PileType.TABLEAU && move.source.order == 0 && move.destination.pileType == PileType.TABLEAU)
        {
            return 0;
        }

        return 1;
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

    public IEnumerable<ScoredMove> GetScoredMoves()
    {
        var moves = GetAllPossibleMoves();
        foreach (var move in moves)
        {
            yield return new ScoredMove(move, GetScoreForMove(move));
        }
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
            List<ScoredMove> movesToConsider = new List<ScoredMove>();
            var moves = GetScoredMoves();
            ScoredMove bestMove = null;
            foreach (var scoredMove in moves)
            {
                if (bestMove == null)
                {
                    bestMove = scoredMove;
                }
                if (scoredMove.Score > bestMove.Score)
                {
                    bestMove = scoredMove;
                    movesToConsider.Clear();
                }
                if (scoredMove.Score == bestMove.Score)
                {
                    movesToConsider.Add(scoredMove);
                }
            }
            smartMoveCache = movesToConsider[random.Next(0, movesToConsider.Count)].Move;
        }
        return smartMoveCache;

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
            smartMoveCache = null;
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

    public IEnumerable<CardMovement> PerformSmartMoves(System.Random random, int numMovesToPerform, out bool success)
    {
        Debug.unityLogger.logEnabled = false;
        var moves = new List<CardMovement>();
        success = true;
        for (int i = 0; i < numMovesToPerform && success && !IsGameOver(); i++)
        {
            var move = GetSmartMove(random);
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