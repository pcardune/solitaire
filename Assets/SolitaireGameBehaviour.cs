using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSuit
{
    CLUBS,
    HEARTS,
    DIAMONDS,
    SPADES,
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
    public CardSuit Suit;
    public CardColor Color;

    [Range(1, 13)]
    public int Rank;

    public Card(CardSuit aSuit, int aValue)
    {
        Suit = aSuit;
        Rank = aValue;
        if (Suit == CardSuit.SPADES || Suit == CardSuit.CLUBS)
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
        return Suit.ToString() + Rank;
    }
}

public class Deck
{
    public static Stack<Card> GetShuffledDeck()
    {
        Stack<Card> shuffledCards = new Stack<Card>();
        List<Card> cards = new List<Card>();
        foreach (var suit in (CardSuit[])Enum.GetValues(typeof(CardSuit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                cards.Add(new Card(suit, value));
            }
        }
        System.Random random = new System.Random();
        while (cards.Count > 0)
        {
            int cardIndex = random.Next(0, cards.Count);
            shuffledCards.Push(cards[cardIndex]);
            cards.RemoveAt(cardIndex);
        }
        return shuffledCards;
    }
}

public class StockPile
{
    public Stack<Card> stock = Deck.GetShuffledDeck();
    public Stack<Card> waste = new Stack<Card>();

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
        return (stock.Pop(), new Location(PileType.STOCK, 0, stock.Count, false));
    }

    public (Card Card, Location Source) PopFromWaste()
    {
        return (waste.Pop(), new Location(PileType.WASTE, 0, stock.Count, true));
    }

    public Location PushOntoWaste(Card card)
    {
        waste.Push(card);
        return new Location(PileType.WASTE, 0, waste.Count - 1, true);
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.WASTE, 0, waste.Count, true);
    }
}

public class FoundationPile
{
    public Stack<Card> Cards = new Stack<Card>();
    public readonly int PileIndex;

    public FoundationPile(int pileIndex)
    {
        PileIndex = pileIndex;
    }

    public Location Push(Card card)
    {
        var destination = GetNextCardLocation();
        Cards.Push(card);
        return destination;
    }
    public (Card card, Location source) Pop()
    {
        return (Cards.Pop(), GetNextCardLocation());
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (Cards.Count == 0)
        {
            return card.Rank == Rank.ACE;
        }
        var lastCard = Cards.Peek();
        return lastCard.Rank == card.Rank - 1 && lastCard.Suit == card.Suit;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.FOUNDATION, PileIndex, Cards.Count, true);
    }

}

public class TableauPile
{
    public Stack<Card> faceDownCards = new Stack<Card>();
    public List<Card> faceUpCards = new List<Card>();

    public readonly int PileIndex;

    public TableauPile(int pileIndex)
    {
        PileIndex = pileIndex;
    }

    public Location PushFaceDown(Card card)
    {
        faceDownCards.Push(card);
        return new Location(PileType.TABLEAU, PileIndex, faceDownCards.Count - 1, false);
    }

    public Location PushFaceUp(Card card)
    {
        var destination = GetNextCardLocation();
        faceUpCards.Add(card);
        return destination;
    }

    public (Card card, Location source) PopFaceUp()
    {
        var card = faceUpCards[faceUpCards.Count - 1];
        faceUpCards.RemoveAt(faceUpCards.Count - 1);
        return (card, GetNextCardLocation());
    }

    public bool CanPushCardOntoPile(Card card)
    {
        if (faceUpCards.Count == 0)
        {
            return faceDownCards.Count == 0 && card.Rank == Rank.KING;
        }
        var lastCard = faceUpCards[faceUpCards.Count - 1];
        return lastCard.Rank == card.Rank + 1 && lastCard.Color != card.Color;
    }

    public Location GetNextCardLocation()
    {
        return new Location(PileType.TABLEAU, PileIndex, faceDownCards.Count + faceUpCards.Count, true);
    }

    public (List<Card> poppedCards, Card? flippedCard) PopAllAfter(int combinedIndex)
    {
        var index = combinedIndex - faceDownCards.Count;
        var cards = faceUpCards.GetRange(index, faceUpCards.Count - index);
        faceUpCards.RemoveRange(index, faceUpCards.Count - index);
        Card? flippedCard = null;
        if (faceUpCards.Count == 0 && faceDownCards.Count > 0)
        {
            faceUpCards.Add(faceDownCards.Pop());
        }
        return (cards, flippedCard);
    }

    public void PushAllOnto(List<Card> cards)
    {
        faceUpCards.AddRange(cards);
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
        return "" + PileType.ToString() + "_" + PileIndex + "[" + Order + "]";
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
        List<CardMovement> moves = new List<CardMovement>();
        if (source.PileType == PileType.STOCK)
        {
            moves.Add(new CardMovement(card, source, stockPile.GetNextCardLocation()));
        }
        else
        {
            foreach (var pile in tableau.piles)
            {
                if (pile.CanPushCardOntoPile(card))
                {
                    var move = new CardMovement(card, source, pile.GetNextCardLocation());
                    Debug.Log("Got Possible move: " + move.ToString());
                    moves.Add(move);
                }
            }
            foreach (var pile in foundations)
            {
                if (pile.CanPushCardOntoPile(card))
                {
                    var move = new CardMovement(card, source, pile.GetNextCardLocation());
                    moves.Add(move);
                }
            }
        }

        return moves;
    }

    public bool PerformMove(CardMovement move)
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
        }
        else if (move.Source.PileType == PileType.TABLEAU)
        {
            if (move.Destination.PileType == PileType.FOUNDATION)
            {
                tableau.piles[move.Source.PileIndex].PopFaceUp();
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
}

public class SolitaireGameBehaviour : MonoBehaviour
{
    public CardBehaviour cardPrefab;
    public GameObject cardTargetPrefab;
    Solitaire solitaire;

    public Vector3 TableauPosition;
    public Vector3 StockPilePosition;
    public Vector2 CardDimensions;
    public Vector2 CardSpacing;
    public Vector3 FoundationPilePosition;

    Dictionary<string, CardBehaviour> cards = new Dictionary<string, CardBehaviour>();

    List<GameObject> cardTargets = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        solitaire = new Solitaire();
        int i = 0;
        foreach (Card card in solitaire.stockPile.stock)
        {
            CardBehaviour cardGameObject = Instantiate<CardBehaviour>(cardPrefab);
            cardGameObject.cardLocation = new Location(PileType.STOCK, 0, i, false);
            cardGameObject.transform.position = GetPositionForCardLocation(cardGameObject.cardLocation.Value);
            cardGameObject.SetOrder(i);
            cardGameObject.card = card;
            cardGameObject.name = card.ToString();
            cardGameObject.faceUp = false;
            cardGameObject.solitaireGameBehaviour = this;
            cards[card.ToString()] = cardGameObject;
            i++;
        }
        DealCards();
    }

    Vector3 GetPositionForCardLocation(Location location)
    {
        Vector3 pos = Vector3.zero;
        pos.z = 0 - location.Order * .01f;
        if (location.PileType == PileType.TABLEAU)
        {
            pos.y = TableauPosition.y + location.Order * CardSpacing.y;
            pos.x = TableauPosition.x + location.PileIndex * (CardDimensions.x + CardSpacing.x);
        }
        else if (location.PileType == PileType.WASTE)
        {
            pos.y = StockPilePosition.y;
            pos.x = StockPilePosition.x - CardDimensions.x - CardSpacing.x;
        }
        else if (location.PileType == PileType.STOCK)
        {
            pos.y = StockPilePosition.y;
            pos.x = StockPilePosition.x;
        }
        else if (location.PileType == PileType.FOUNDATION)
        {
            pos.y = FoundationPilePosition.y;
            pos.x = FoundationPilePosition.x + location.PileIndex * (CardDimensions.x + CardSpacing.x);
        }
        return pos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                Gizmos.DrawWireCube(GetPositionForCardLocation(new Location(PileType.TABLEAU, i, j, false)), CardDimensions);
            }
        }
        Gizmos.DrawWireCube(GetPositionForCardLocation(new Location(PileType.STOCK, 0, 0, false)), CardDimensions);
        Gizmos.DrawWireCube(GetPositionForCardLocation(new Location(PileType.WASTE, 0, 0, false)), CardDimensions);
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawWireCube(GetPositionForCardLocation(new Location(PileType.FOUNDATION, i, 0, false)), CardDimensions);
        }
    }

    async void DealCards()
    {
        var moves = solitaire.Deal();
        foreach (var move in moves)
        {
            Debug.Log("Performing move: " + move);
            await AnimateMove(move);
        }
    }

    async Task AnimateMove(CardMovement move)
    {
        CardBehaviour cardToMove = cards[move.Card.ToString()];
        cardToMove.SetFaceUp(move.Destination.FaceUp);
        cardToMove.cardLocation = null;
        await cardToMove.GetComponent<MoveBehaviour>().MoveTo(GetPositionForCardLocation(move.Destination), .1f, move.Destination.Order);
        cardToMove.cardLocation = move.Destination;

        if (move.Destination.PileType == PileType.TABLEAU)
        {
            var pile = solitaire.tableau.piles[move.Destination.PileIndex];
            if (pile.faceUpCards.Count > 1)
            {
                var parentCard = cards[pile.faceUpCards[pile.faceUpCards.Count - 2].ToString()];
                cardToMove.transform.parent = parentCard.transform;
                Debug.Log("Setting parent of " + cardToMove.name + " to " + parentCard.name);
            }
        }
        if (move.Source.PileType == PileType.TABLEAU)
        {
            foreach (var card in solitaire.tableau.piles[move.Source.PileIndex].faceUpCards)
            {
                var otherCardToMove = cards[card.ToString()];
                var location = otherCardToMove.cardLocation.Value;
                location.FaceUp = true;
                otherCardToMove.cardLocation = location;
                otherCardToMove.SetFaceUp(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickCard(CardBehaviour cardBehaviour)
    {
        var location = cardBehaviour.cardLocation;
        var card = cardBehaviour.card;
        var moves = solitaire.GetPossibleMovesForCard(card, location.Value);
        if (moves.Count > 0)
        {
            if (solitaire.PerformMove(moves[0]))
            {
                AnimateMove(moves[0]);
            }
        }
    }

    public void OnMouseDownCard(CardBehaviour cardBehaviour)
    {
        var moves = solitaire.GetPossibleMovesForCard(cardBehaviour.card, cardBehaviour.cardLocation.Value);
        foreach (var move in moves)
        {
            var target = Instantiate(cardTargetPrefab);
            target.transform.position = GetPositionForCardLocation(move.Destination);
            cardTargets.Add(target);
        }
    }

    public void OnMouseUpCard(CardBehaviour cardBehaviour)
    {
        foreach (var target in cardTargets)
        {
            Destroy(target);
        }
    }
}
