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
    Resetting,
}
public class SolitaireGameBehaviour : MonoBehaviour
{
    public CardBehaviour cardPrefab;
    public CardTarget cardTargetPrefab;
    public GameObject lineRendererPrefab;
    public Solitaire solitaire { get; private set; }

    public Text winText;

    public Vector3 TableauPosition;
    public Vector3 StockPilePosition;
    public Vector2 CardDimensions;
    public Vector2 CardSpacing;
    public Vector3 FoundationPilePosition;

    public float GameDuration { get; private set; }


    Dictionary<string, CardBehaviour> cardsById = new Dictionary<string, CardBehaviour>();

    List<CardTarget> cardTargets = new List<CardTarget>();
    public CardTarget SelectedCardTarget
    {
        get
        {
            foreach (var target in cardTargets)
            {
                if (target.IsSelected)
                {
                    return target;
                }
            }
            return null;
        }
    }
    public bool AutoPlay = false;
    public int MaxAutoPlayMoves = 1000;
    CardBehaviour cardBeingMoved;
    System.Random random;

    public bool useRandomSeed = true;
    public int randomSeed = 1;
    public int RandomSeedToUse
    {
        get
        {
            if (useRandomSeed)
            {
                return randomSeed;
            }
            return new System.Random().Next();
        }
    }
    public bool debugPossibleMoves = false;
    List<MoveLineBehaviour> possibleMoveLines = new List<MoveLineBehaviour>();

    Queue<CardMovement> moveQueue = new Queue<CardMovement>();
    public GameState state = GameState.Init;

    public static SolitaireGameBehaviour Instance { get; private set; }

    void Awake()
    {
        SolitaireGameBehaviour.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random(RandomSeedToUse);
        solitaire = new Solitaire(RandomSeedToUse);
        GameDuration = 0;
        int i = 0;
        foreach (var locatedCard in solitaire.stockPile.stock.LocatedCards())
        {
            CardBehaviour cardGameObject = Instantiate<CardBehaviour>(cardPrefab);
            cardGameObject.transform.position = GetPositionForCardLocation(cardGameObject.cardLocation);
            cardGameObject.locatedCard = locatedCard;
            cardGameObject.name = locatedCard.Card.ToString();
            cardGameObject.solitaireGameBehaviour = this;
            cardsById[locatedCard.Card.Id] = cardGameObject;
            i++;
        }
        Validate();
    }


    public void NewGame()
    {
        random = new System.Random(RandomSeedToUse);
        solitaire = new Solitaire(RandomSeedToUse);
        GameDuration = 0;
        int i = 0;
        foreach (var locatedCard in solitaire.stockPile.stock.LocatedCards())
        {
            var cardGameObject = cardsById[locatedCard.Card.Id];
            cardGameObject.transform.parent = null;
            cardGameObject.locatedCard = locatedCard;
            cardGameObject.Move.MoveTo(GetPositionForCardLocation(locatedCard.Location));
            i++;
        }
        Validate();
        state = GameState.Resetting;
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
        if (move.Type == MoveType.StockPileReset)
        {
            // when doing a stock pile reset, we have to move all the cards from the waste
            // pile to the stock pile 
            foreach (var locatedCard in solitaire.stockPile.stock.LocatedCards())
            {
                cardBeingMoved = cardsById[locatedCard.Card.Id];
                cardBeingMoved.locatedCard = locatedCard;
                cardBeingMoved.Move.MoveTo(GetPositionForCardLocation(cardBeingMoved.cardLocation));
            }
        }
        else if (move.Type == MoveType.SingleCard)
        {
            cardBeingMoved = cardsById[move.Card.Id];
            cardBeingMoved.transform.parent = null;
            cardBeingMoved.Move.MoveTo(GetPositionForCardLocation(move.Destination));
            cardBeingMoved.locatedCard = new LocatedCard(move.Card, move.Destination);

            // Update parents of all the cards in the destination pile
            // so that they get dragged appropriately
            if (move.Destination.PileType == PileType.TABLEAU)
            {
                var pile = solitaire.tableau.piles[move.Destination.PileIndex];
                CardBehaviour parent = null;
                foreach (var locatedCard in pile.LocatedCards())
                {
                    var cardBehaviour = cardsById[locatedCard.Card.Id];
                    cardBehaviour.locatedCard = locatedCard;
                    if (parent != null && parent.cardLocation.FaceUp == true)
                    {
                        cardBehaviour.transform.parent = parent.transform;
                    }
                    parent = cardBehaviour;
                }
            }

            if (move.Source.PileType == PileType.TABLEAU)
            {
                var pile = solitaire.tableau.piles[move.Source.PileIndex];
                foreach (var locatedCard in pile.LocatedCards())
                {
                    var otherCardToMove = cardsById[locatedCard.Card.Id];
                    otherCardToMove.locatedCard = locatedCard;
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        CardBehaviour justFinishedMovingCard = null;
        if (cardBeingMoved != null && !cardBeingMoved.Move.IsMoving)
        {
            justFinishedMovingCard = cardBeingMoved;
            cardBeingMoved = null;
        }

        if (state == GameState.Resetting)
        {
            var stillMoving = false;
            foreach (var card in cardsById.Values)
            {
                if (card.Move.IsMoving)
                {
                    stillMoving = true;
                    break;
                }
            }
            if (!stillMoving)
            {
                state = GameState.Init;
            }
        }

        if (state == GameState.Init)
        {
            winText.gameObject.SetActive(false);
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
            GameDuration += Time.deltaTime;
            if (justFinishedMovingCard != null)
            {
                Validate();
                if (solitaire.IsGameOver())
                {
                    winText.gameObject.SetActive(true);
                }
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
        CardMovement move = null;
        if (SelectedCardTarget == null)
        {
            var moves = solitaire.GetPossibleMovesForCard(card, location);
            if (moves.Count > 0 && cardBehaviour.Drag.DragDuration < 0.5f)
            {
                move = moves[0];
            }

        }
        else
        {
            move = new CardMovement(card, location, SelectedCardTarget.cardLocation);
        }
        if (move != null)
        {
            PerformAndAnimateMove(move);
        }
        else
        {
            cardBehaviour.Drag.Reset();
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
                var cardTarget = Instantiate(
                    cardTargetPrefab,
                    GetPositionForCardLocation(move.Destination),
                    Quaternion.identity
                );
                cardTarget.cardLocation = move.Destination;
                cardTargets.Add(cardTarget);
            }
            isDraggingCard = true;
        }
    }

    public void OnMouseUpCard(CardBehaviour cardBehaviour)
    {
        foreach (var target in cardTargets)
        {
            Destroy(target.gameObject);
        }
        cardTargets.Clear();
        isDraggingCard = false;
    }

    public void ToggleAutoPlay()
    {
        AutoPlay = !AutoPlay;
    }

    public void MakeRandomMove()
    {
        var move = solitaire.GetSmartMove(random);
        Debug.Log("Performing random move: " + move);
        PerformAndAnimateMove(move);
    }

    public void PerformAndAnimateMove(CardMovement move)
    {
        if (solitaire.PerformMove(move))
        {
            moveQueue.Enqueue(move);
            UpdatePossibleMoveLines();
        }
    }

    public void ResetStockPile()
    {
        if (solitaire.stockPile.CanReset())
        {
            var move = solitaire.stockPile.GetResetMove();
            PerformAndAnimateMove(move);
        }
    }

    void UpdatePossibleMoveLines()
    {
        if (debugPossibleMoves)
        {
            var moves = solitaire.GetAllPossibleMoves();
            var randomMove = solitaire.GetSmartMove(random);
            // var s = "Found the following possible moves:\n";
            // for (int i = 0; i < moves.Count; i++)
            // {
            //     s += "  " + i + ". " + moves[i].ToString() + "\n";
            // }
            // Debug.Log(s);

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
                line.Highlight = move == randomMove;
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
            foreach (var card in pile)
            {
                var location = cardsById[card.Id].cardLocation;
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
            int i = 0;
            for (; i < pile.FaceDownCount; i++)
            {
                var card = pile[i];
                var location = cardsById[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.TABLEAU, $"{card}: Wrong Pile. Expected: {PileType.TABLEAU} Got: {location.PileType}");
                AssertIsTrue(location.PileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == false, $"{card}: Cards in tableau facedown pile should all be face down.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                order++;
            }
            Card? lastCard = null;
            for (; i < pile.Count; i++)
            {
                var card = pile[i];
                var location = cardsById[card.Id].cardLocation;
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
                var location = cardsById[card.Id].cardLocation;
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
                var location = cardsById[card.Id].cardLocation;
                AssertIsTrue(location.PileType == PileType.STOCK, $"{card}: Wrong Pile");
                AssertIsTrue(location.PileIndex == 0, $"{card}: Wrong pile index");
                AssertIsTrue(location.FaceUp == false, $"{card}: Cards in stock pile should all be face down.");
                AssertIsTrue(location.Order == order, $"{card}: should be in the correct order: {order} (was {location.Order}).");
                order++;
            }
        }
    }
}
