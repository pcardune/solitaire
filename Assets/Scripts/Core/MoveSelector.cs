using System.Collections;
using System.Collections.Generic;

public interface IMoveSelector
{
    CardMovement GetMove(Solitaire solitaire);
}

public class MoveSelector : IMoveSelector
{
    private System.Random random;

    public MoveSelector(System.Random random)
    {
        this.random = random;
    }

    public MoveSelector(int randomSeed)
    {
        this.random = new System.Random(randomSeed);
    }

    public static int GetScoreForMove(Solitaire solitaire, CardMovement move)
    {
        // higher score means better move

        // it's always a good idea to move aces onto the foundation
        if (move.card.Rank == Rank.ACE && move.destination.pileType == PileType.FOUNDATION)
        {
            return 10;
        }
        // it's always good to uncover cards in the tableau
        if (move.source.pileType == PileType.TABLEAU && move.source.order > 0 && move.source.order == solitaire.tableauPiles[move.source.pileIndex].FaceDownCount)
        {
            return 10;
        }

        // it's always good to move a card onto the tableau from the waste if it allows a new card to be uncovered
        if (move.destination.pileType == PileType.TABLEAU && move.source.pileType == PileType.WASTE)
        {
            foreach (var pile in solitaire.tableauPiles)
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
        foreach (var pile in solitaire.tableauPiles)
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

    public static IEnumerable<ScoredMove> GetScoredMoves(Solitaire solitaire)
    {
        var moves = solitaire.GetAllPossibleMoves();
        foreach (var move in moves)
        {
            yield return new ScoredMove(move, GetScoreForMove(solitaire, move));
        }
    }

    public CardMovement GetMove(Solitaire solitaire)
    {
        List<ScoredMove> movesToConsider = new List<ScoredMove>();
        var moves = GetScoredMoves(solitaire);
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
        return movesToConsider[random.Next(0, movesToConsider.Count)].Move;
    }
}
