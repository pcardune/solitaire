using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public enum GameState
{
    Init,
    Dealing,
    Playing,
}
public class SolitaireGameBehaviour : MonoBehaviour
{
    public CardBehaviour cardPrefab;
    public GameObject cardTargetPrefab;
    public GameObject lineRendererPrefab;
    Solitaire solitaire;

    public Text GameStatsText;

    public Vector3 TableauPosition;
    public Vector3 StockPilePosition;
    public Vector2 CardDimensions;
    public Vector2 CardSpacing;
    public Vector3 FoundationPilePosition;

    Dictionary<string, CardBehaviour> cards = new Dictionary<string, CardBehaviour>();

    List<GameObject> cardTargets = new List<GameObject>();
    public bool AutoPlay = false;
    public int MaxAutoPlayMoves = 1000;
    CardBehaviour cardBeingMoved;
    System.Random random;

    public int randomSeed = 1;
    public bool debugPossibleMoves = false;
    List<MoveLineBehaviour> possibleMoveLines = new List<MoveLineBehaviour>();

    Queue<CardMovement> moveQueue = new Queue<CardMovement>();
    public GameState state = GameState.Init;

    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random(randomSeed);
        solitaire = new Solitaire();
        int i = 0;
        foreach (Card card in solitaire.stockPile.stock)
        {
            CardBehaviour cardGameObject = Instantiate<CardBehaviour>(cardPrefab);
            cardGameObject.cardLocation = new Location(PileType.STOCK, 0, i, false);
            cardGameObject.transform.position = GetPositionForCardLocation(cardGameObject.cardLocation);
            cardGameObject.card = card;
            cardGameObject.name = card.ToString();
            cardGameObject.faceUp = false;
            cardGameObject.solitaireGameBehaviour = this;
            cards[card.Id] = cardGameObject;
            i++;
        }
        Validate();
    }

    public Vector3 GetPositionForCardLocation(Location location)
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

    public void DealCards()
    {
        state = GameState.Dealing;
        foreach (var move in solitaire.Deal())
        {
            moveQueue.Enqueue(move);
        }
    }

    void AnimateMove(CardMovement move)
    {
        cardBeingMoved = cards[move.Card.Id];
        cardBeingMoved.SetFaceUp(move.Destination.FaceUp);
        cardBeingMoved.GetComponent<MoveBehaviour>().MoveTo(GetPositionForCardLocation(move.Destination), .1f, move.Destination.Order);
        cardBeingMoved.cardLocation = move.Destination;

        if (move.Destination.PileType == PileType.TABLEAU)
        {
            var pile = solitaire.tableau.piles[move.Destination.PileIndex];
            int order = pile.faceDownCards.Count + 1;
            for (int i = 1; i < pile.faceUpCards.Count; i++)
            {
                var cardBehaviour = cards[pile.faceUpCards[i].Id];
                var location = move.Destination;
                location.Order = order;
                cardBehaviour.cardLocation = location;
                order++;
                var parentCard = cards[pile.faceUpCards[i - 1].Id];
                cardBehaviour.transform.parent = parentCard.transform;
            }
        }
        if (move.Source.PileType == PileType.TABLEAU)
        {
            foreach (var card in solitaire.tableau.piles[move.Source.PileIndex].faceUpCards)
            {
                var otherCardToMove = cards[card.Id];
                otherCardToMove.cardLocation.FaceUp = true;
                otherCardToMove.SetFaceUp(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CardBehaviour justFinishedMovingCard = null;
        if (cardBeingMoved != null && !cardBeingMoved.GetComponent<MoveBehaviour>().IsMoving)
        {
            justFinishedMovingCard = cardBeingMoved;
            cardBeingMoved = null;
        }

        if (state == GameState.Init)
        {
            DealCards();
        }
        if (state == GameState.Dealing)
        {
            if (moveQueue.Count > 0)
            {
                if (cardBeingMoved == null)
                {
                    AnimateMove(moveQueue.Dequeue());
                }
            }
            else
            {
                Validate();
                state = GameState.Playing;
            }
        }
        if (state == GameState.Playing)
        {
            if (justFinishedMovingCard != null)
            {
                Validate();
            }
            if (cardBeingMoved == null)
            {
                if (moveQueue.Count > 0)
                {
                    var nextMove = moveQueue.Dequeue();
                    AnimateMove(nextMove);
                }
                else if (AutoPlay && solitaire.moveHistory.Count < MaxAutoPlayMoves)
                {
                    MakeRandomMove();
                }
            }
        }
    }

    public void OnClickCard(CardBehaviour cardBehaviour)
    {
        var location = cardBehaviour.cardLocation;
        var card = cardBehaviour.card;
        var moves = solitaire.GetPossibleMovesForCard(card, location);
        if (moves.Count > 0)
        {
            PerformAndAnimateMove(moves[0]);
        }
    }

    bool isDraggingCard = false;

    public void OnMouseDownCard(CardBehaviour cardBehaviour)
    {

    }

    public void OnMouseDragCard(CardBehaviour cardBehaviour)
    {
        if (!isDraggingCard)
        {
            var moves = solitaire.GetPossibleMovesForCard(cardBehaviour.card, cardBehaviour.cardLocation);
            foreach (var move in moves)
            {
                var target = Instantiate(cardTargetPrefab);
                target.transform.position = GetPositionForCardLocation(move.Destination);
                cardTargets.Add(target);
            }
            isDraggingCard = true;
        }
    }

    public void OnMouseUpCard(CardBehaviour cardBehaviour)
    {
        foreach (var target in cardTargets)
        {
            Destroy(target);
        }
        isDraggingCard = false;
    }

    public void ToggleAutoPlay()
    {
        AutoPlay = !AutoPlay;
    }

    public void MakeRandomMove()
    {
        MakeRandomMoveAsync();
    }

    public void MakeRandomMoveAsync()
    {
        var moves = solitaire.GetAllPossibleMoves();

        var move = moves[random.Next(0, moves.Count)];
        Debug.Log("Performing random move: " + move);
        PerformAndAnimateMove(move);
    }

    public void PerformAndAnimateMove(CardMovement move)
    {
        if (solitaire.PerformMove(move))
        {
            moveQueue.Enqueue(move);
            UpdatePossibleMoveLines();
            UpdateGameStats();
        }
    }

    void UpdateGameStats()
    {
        GameStatsText.text = "Moves: " + solitaire.moveHistory.Count;
    }

    void UpdatePossibleMoveLines()
    {
        if (debugPossibleMoves)
        {
            var moves = solitaire.GetAllPossibleMoves();
            var s = "Found the following possible moves:\n";
            for (int i = 0; i < moves.Count; i++)
            {
                s += "  " + i + ". " + moves[i].ToString() + "\n";
            }
            Debug.Log(s);

            var lineIndex = 0;
            foreach (var move in moves)
            {
                MoveLineBehaviour line;
                if (lineIndex < possibleMoveLines.Count - 1)
                {
                    line = possibleMoveLines[lineIndex];
                }
                else
                {
                    line = Instantiate(lineRendererPrefab).GetComponent<MoveLineBehaviour>();
                    line.solitaireGameBehaviour = this;
                    possibleMoveLines.Add(line);
                }
                line.gameObject.SetActive(true);
                line.SetMove(move);
                lineIndex++;
            }
            for (; lineIndex < possibleMoveLines.Count; lineIndex++)
            {
                possibleMoveLines[lineIndex].gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var line in possibleMoveLines)
            {
                line.gameObject.SetActive(false);
            }
            return;
        }

    }

    public void DebugGameState()
    {
        debugPossibleMoves = !debugPossibleMoves;
        UpdatePossibleMoveLines();
    }

    public void DumpGameState()
    {
        Debug.Log("GAME STATE DUMP:\n" + JsonUtility.ToJson(solitaire.ToJSON(), true));
    }

    void AssertIsTrue(bool cond, string msg)
    {
        if (!cond)
        {
            Debug.Break();
        }
        Debug.Assert(cond, msg);
        Assert.IsTrue(cond, msg);
    }

    public void Validate()
    {
        // validate the foundations
        for (int pileIndex = 0; pileIndex < solitaire.foundations.Count; pileIndex++)
        {
            var pile = solitaire.foundations[pileIndex];
            int order = 0;
            Card? lastCard = null;
            foreach (var card in pile.Cards)
            {
                var location = cards[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.FOUNDATION, $"{card}: Wrong Pile: {location.PileType}");
                AssertIsTrue(location.PileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == true, $"{card}: Cards in foundation should all be face up.");
                AssertIsTrue(location.Order == order, $"{card}: Cards should be in the correct order.");
                if (order == 0)
                {
                    AssertIsTrue(card.Rank == Rank.ACE, $"{card}: First card in foundation pile must be an ace.");
                }
                if (lastCard.HasValue)
                {
                    AssertIsTrue(card.Suit == lastCard.Value.Suit, $"{card}: Cards in foundation pile should all have the same suit.");
                    AssertIsTrue(card.Rank == lastCard.Value.Rank + 1, $"{card}: Cards in foundation pile should go up in rank by 1 each time.");
                }
                lastCard = card;
                order++;
            }
        }

        // validate the tableau
        for (int pileIndex = 0; pileIndex < solitaire.tableau.piles.Count; pileIndex++)
        {
            var pile = solitaire.tableau.piles[pileIndex];
            int order = 0;
            foreach (var card in pile.faceDownCards)
            {
                var location = cards[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.TABLEAU, $"{card}: Wrong Pile. Expected: {PileType.TABLEAU} Got: {location.PileType}");
                AssertIsTrue(location.PileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == false, $"{card}: Cards in tableau facedown pile should all be face down.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                order++;
            }
            Card? lastCard = null;
            foreach (var card in pile.faceUpCards)
            {
                var location = cards[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.TABLEAU, $"{card}: Wrong Pile");
                AssertIsTrue(location.PileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == true, $"{card}: Cards in tableau faceup pile should all be face up.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                if (lastCard.HasValue)
                {
                    AssertIsTrue(card.Color != lastCard.Value.Color, $"{card}: Cards in tableau should alternate colors");
                    AssertIsTrue(card.Rank == lastCard.Value.Rank - 1, $"{card}: Cards in tableau pile should go down in rank by 1 each time.");
                }
                lastCard = card;
                order++;
            }
        }

        // validate the waste pile
        {
            int order = 0;
            foreach (var card in solitaire.stockPile.waste)
            {
                var location = cards[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.WASTE, $"{card}: Wrong Pile");
                AssertIsTrue(location.PileIndex == 0, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == true, $"{card}: Cards in waste pile should all be face up.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                order++;
            }
        }
        // validate the stock pile
        {
            int order = 0;
            foreach (var card in solitaire.stockPile.stock)
            {
                var location = cards[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.STOCK, $"{card}: Wrong Pile");
                AssertIsTrue(location.PileIndex == 0, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == false, $"{card}: Cards in stock pile should all be face down.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                order++;
            }
        }
    }
}
