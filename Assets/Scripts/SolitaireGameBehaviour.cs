using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public enum GameState
{
    Init,
    Dealing,
    Playing,
    Resetting,
    Finished,
}
public class SolitaireGameBehaviour : MonoBehaviour
{
    public CardBehaviour cardPrefab;
    public CardTarget cardTargetPrefab;
    public GameObject lineRendererPrefab;
    public StockPileEmpty stockPileEmptyPrefab;
    public Canvas IntroScreenCanvas;
    public Canvas GameCanvas;
    public Solitaire solitaire { get; private set; }
    public float cardAnimationSpeed = 0.1f;

    public Text winText;

    public Vector3 TableauPosition;
    public Vector3 StockPilePosition;
    public Vector2 CardDimensions;
    public Vector2 CardSpacing;
    public Vector3 FoundationPilePosition;
    public DrawType drawType = DrawType.SingleCardDraw;

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

    private GameObject CardSpace;
    private Transform CardTopLevelTransform
    {
        get
        {
            return CardSpace.transform;
        }
    }

    void Awake()
    {
        SolitaireGameBehaviour.Instance = this;
        CardSpace = new GameObject("CardSpace");
    }

    // Start is called before the first frame update
    void Start()
    {
        IntroScreenCanvas.gameObject.SetActive(false);
        GameCanvas.gameObject.SetActive(true);
        Application.targetFrameRate = 60;
        random = new System.Random(RandomSeedToUse);
        solitaire = new Solitaire(RandomSeedToUse, drawType);
        GameDuration = 0;
        StockPileEmpty stockPileEmpty = Instantiate<StockPileEmpty>(stockPileEmptyPrefab);
        stockPileEmpty.name = "StockPileEmpty";
        stockPileEmpty.transform.position = StockPilePosition + Vector3.forward;
        foreach (var locatedCard in solitaire.stockPile.stock.LocatedCards())
        {
            CardBehaviour cardGameObject = Instantiate<CardBehaviour>(cardPrefab, CardTopLevelTransform);
            cardGameObject.locatedCard = locatedCard;
            cardGameObject.transform.position = GetPositionForCardLocation(locatedCard.Location);
            cardGameObject.name = locatedCard.Card.ToString();
            cardsById[locatedCard.Card.Id] = cardGameObject;
        }
        Validate();
    }

    public void OnDrawTypeValueChanged(Dropdown change)
    {
        drawType = (DrawType)change.value;
        solitaire.drawType = drawType;
    }

    public void NewGame()
    {
        random = new System.Random(RandomSeedToUse);
        solitaire = new Solitaire(RandomSeedToUse, drawType);
        GameDuration = 0;
        MoveAllCardsToCurrentLocation();
        Validate();
        state = GameState.Resetting;
    }

    public void NewThreeCardDrawGame()
    {
        drawType = DrawType.ThreeCardDraw;
        enabled = true;
    }

    public void NewSingleCardDrawGame()
    {
        drawType = DrawType.SingleCardDraw;
        enabled = true;
    }

    private void MoveAllCardsToCurrentLocation()
    {
        foreach (var locatedCard in solitaire.AllCards())
        {
            Debug.Log($"move card {locatedCard.Card.Id}: {locatedCard.Card}");
            var cardGameObject = cardsById[locatedCard.Card.Id];
            if (!cardGameObject.locatedCard.Equals(locatedCard))
            {
                cardGameObject.transform.parent = CardTopLevelTransform;
                cardGameObject.locatedCard = locatedCard;
                cardGameObject.Move.MoveTo(GetPositionForCardLocation(locatedCard.Location));
            }
        }
    }

    public void UndoMove()
    {
        var undone = new Solitaire(solitaire.RandomSeed, drawType);
        undone.DealAll();
        for (int i = 0; i < solitaire.moveHistory.Count - 1; i++)
        {
            undone.PerformMove(solitaire.moveHistory[i]);
        }
        solitaire = undone;
        MoveAllCardsToCurrentLocation();
        Validate();
    }

    public void SaveGame()
    {
        var bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, solitaire);
        file.Close();
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            solitaire = (Solitaire)bf.Deserialize(file);
            file.Close();

            MoveAllCardsToCurrentLocation();
            state = GameState.Playing;
        }
    }

    public Vector3 GetPositionForCardLocation(Location location)
    {
        Vector3 pos = Vector3.zero;
        pos.z = 0 - location.order * .01f;
        if (location.pileType == PileType.TABLEAU)
        {
            pos.y = TableauPosition.y + location.order * CardSpacing.y;
            pos.x = TableauPosition.x + location.pileIndex * (CardDimensions.x + CardSpacing.x);
        }
        else if (location.pileType == PileType.WASTE)
        {
            pos.y = StockPilePosition.y;
            pos.x = StockPilePosition.x - CardDimensions.x - CardSpacing.x;
            if (drawType == DrawType.ThreeCardDraw)
            {
                var topIndex = solitaire == null ? 0 : solitaire.stockPile.waste.Count - 1;
                var rightMostPos = pos.x;
                if (location.order > topIndex - 3)
                {
                    pos.x = rightMostPos - CardSpacing.x * 2 * (topIndex - location.order);
                }
                else
                {
                    pos.x = rightMostPos - CardSpacing.x * 2 * 2;
                }
            }
        }
        else if (location.pileType == PileType.STOCK)
        {
            pos.y = StockPilePosition.y;
            pos.x = StockPilePosition.x;
        }
        else if (location.pileType == PileType.FOUNDATION)
        {
            pos.y = FoundationPilePosition.y;
            pos.x = FoundationPilePosition.x + location.pileIndex * (CardDimensions.x + CardSpacing.x);
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
        if (move.type == MoveType.StockPileReset)
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
        else if (move.type == MoveType.SingleCard)
        {
            cardBeingMoved = cardsById[move.card.Id];
            cardBeingMoved.transform.parent = CardTopLevelTransform;
            cardBeingMoved.Move.MoveTo(GetPositionForCardLocation(move.destination));
            cardBeingMoved.locatedCard = new LocatedCard(move.card, move.destination);

            if (move.source.pileType == PileType.STOCK)
            {
                if (solitaire.drawType == DrawType.ThreeCardDraw)
                {
                    // update the locations of the other cards that were drawn
                    foreach (var locatedCard in solitaire.stockPile.waste.LocatedCards())
                    {
                        cardBeingMoved = cardsById[locatedCard.Card.Id];
                        cardBeingMoved.Move.MoveTo(GetPositionForCardLocation(locatedCard.Location));
                        cardBeingMoved.locatedCard = locatedCard;
                    }
                }
            }

            // Update parents of all the cards in the destination pile
            // so that they get dragged appropriately
            if (move.destination.pileType == PileType.TABLEAU)
            {
                var pile = solitaire.tableauPiles[move.destination.pileIndex];
                CardBehaviour parent = null;
                foreach (var locatedCard in pile.LocatedCards())
                {
                    var cardBehaviour = cardsById[locatedCard.Card.Id];
                    cardBehaviour.locatedCard = locatedCard;
                    if (parent != null && parent.cardLocation.faceUp == true)
                    {
                        cardBehaviour.transform.parent = parent.transform;
                    }
                    parent = cardBehaviour;
                }
            }

            if (move.source.pileType == PileType.TABLEAU)
            {
                var pile = solitaire.tableauPiles[move.source.pileIndex];
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
        if (solitaire != null)
        {
            solitaire.drawType = drawType;
        }

        CardBehaviour justFinishedMovingCard = null;
        if (cardBeingMoved != null && cardBeingMoved.Move && !cardBeingMoved.Move.IsMoving)
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
                    AutoPlay = false;
                    state = GameState.Finished;
                }
            }
            if (cardBeingMoved == null)
            {
                if (moveQueue.Count > 0)
                {
                    AnimateMove(moveQueue.Dequeue());
                }
                else if (AutoPlay)
                {
                    if (solitaire.moveHistory.Count < MaxAutoPlayMoves)
                    {
                        MakeAutoPlayMove();
                    }
                    else
                    {
                        AutoPlay = false;
                    }
                }
            }
        }
    }

    public void OnClickCard(CardBehaviour cardBehaviour)
    {
        var locatedCard = cardBehaviour.locatedCard;
        CardMovement move = null;
        if (SelectedCardTarget == null)
        {
            var moves = solitaire.GetPossibleMovesForCard(locatedCard);
            if (moves.Count > 0 && cardBehaviour.Drag.DragDuration < 0.5f)
            {
                move = moves[0];
                var maxScore = MoveSelector.GetScoreForMove(solitaire, move);
                for (int i = 0; i < moves.Count; i++)
                {
                    var score = MoveSelector.GetScoreForMove(solitaire, move);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        move = moves[i];
                    }
                }
            }

        }
        else
        {
            move = new CardMovement(locatedCard, SelectedCardTarget.cardLocation);
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
            var moves = solitaire.GetPossibleMovesForCard(cardBehaviour.locatedCard);
            foreach (var move in moves)
            {
                var cardTarget = Instantiate(
                    cardTargetPrefab,
                    GetPositionForCardLocation(move.destination),
                    Quaternion.identity
                );
                cardTarget.cardLocation = move.destination;
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

    public void MakeAutoPlayMove()
    {
        // bool success;
        // var moves = solitaire.PerformSmartMoves(random, MaxAutoPlayMoves, out success);
        // MoveAllCardsToCurrentLocation();

        var move = new MoveSelector(random).GetMove(solitaire);
        Debug.Log("Performing smart move: " + move);
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
            // var moves = solitaire.GetAllPossibleMoves();
            var randomMove = new MoveSelector(random).GetMove(solitaire);

            var lineIndex = 0;
            foreach (var scoredMove in MoveSelector.GetScoredMoves(solitaire))
            {
                MoveLineBehaviour line;
                if (lineIndex < possibleMoveLines.Count - 1)
                {
                    line = possibleMoveLines[lineIndex];
                }
                else
                {
                    line = Instantiate(lineRendererPrefab).GetComponent<MoveLineBehaviour>();
                    possibleMoveLines.Add(line);
                }
                line.gameObject.SetActive(true);
                line.SetScoredMove(scoredMove);
                line.Highlight = scoredMove.Move == randomMove;
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
                AssertIsTrue(location.pileType == PileType.FOUNDATION, $"{card}: Wrong Pile: {location.pileType}");
                AssertIsTrue(location.pileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.faceUp == true, $"{card}: Cards in foundation should all be face up.");
                AssertIsTrue(location.order == order, $"{card}: Cards should be in the correct order.");
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
        for (int pileIndex = 0; pileIndex < solitaire.tableauPiles.Count; pileIndex++)
        {
            var pile = solitaire.tableauPiles[pileIndex];
            int order = 0;
            int i = 0;
            for (; i < pile.FaceDownCount; i++)
            {
                var card = pile[i];
                var location = cardsById[card.Id].cardLocation;
                AssertIsTrue(location.pileType == PileType.TABLEAU, $"{card}: Wrong Pile. Expected: {PileType.TABLEAU} Got: {location.pileType}");
                AssertIsTrue(location.pileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.faceUp == false, $"{card}: Cards in tableau facedown pile should all be face down.");
                AssertIsTrue(location.order == order, $"{card}: should be in the correct order: {order} (was {location.order}).");
                order++;
            }
            Card? lastCard = null;
            for (; i < pile.Count; i++)
            {
                var card = pile[i];
                var location = cardsById[card.Id].cardLocation;
                AssertIsTrue(location.pileType == PileType.TABLEAU, $"{card}: Wrong Pile");
                AssertIsTrue(location.pileIndex == pileIndex, $"{card}: Wrong pile index");
                AssertIsTrue(location.faceUp == true, $"{card}: Cards in tableau faceup pile should all be face up.");
                AssertIsTrue(location.order == order, $"{card}: should be in the correct order: {order} (was {location.order}).");
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
                AssertIsTrue(location.pileType == PileType.WASTE, $"{card}: Wrong Pile {location.pileType}. Expected {PileType.WASTE}");
                AssertIsTrue(location.pileIndex == 0, $"{card}: Wrong pile index");
                AssertIsTrue(location.faceUp == true, $"{card}: Cards in waste pile should all be face up.");
                AssertIsTrue(location.order == order, $"{card}: should be in the correct order: {order} (was {location.order}).");
                order++;
            }
        }
        // validate the stock pile
        {
            int order = 0;
            foreach (var card in solitaire.stockPile.stock)
            {
                var location = cardsById[card.Id].cardLocation;
                AssertIsTrue(location.pileType == PileType.STOCK, $"{card}: Wrong Pile");
                AssertIsTrue(location.pileIndex == 0, $"{card}: Wrong pile index");
                AssertIsTrue(location.faceUp == false, $"{card}: Cards in stock pile should all be face down.");
                AssertIsTrue(location.order == order, $"{card}: should be in the correct order: {order} (was {location.order}).");
                order++;
            }
        }
    }
}
