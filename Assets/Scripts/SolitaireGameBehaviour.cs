using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    System.Random random;

    public int randomSeed = 1;
    public bool debugPossibleMoves = false;
    List<MoveLineBehaviour> possibleMoveLines = new List<MoveLineBehaviour>();

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
            cards[card.ToString()] = cardGameObject;
            i++;
        }
        DealCards();
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
                otherCardToMove.cardLocation.FaceUp = true;
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

    public void MakeRandomMove()
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
            AnimateMove(move);
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
}
