using System.Collections.Generic;
using UnityEngine;

public class AI_MinMax : Player
{
    public int searchDepth = 2;

    public override void NotifyTurnToMove()
    {
        base.NotifyTurnToMove();

        Move bestMove = GetBestMove();
        Debug.Log("????? " + bestMove);
        BoardManager.Instance.PlaceMark(bestMove.x, bestMove.y);
        ChoseMove(bestMove, this);
    }

    public Move GetBestMove()
    {
        // 1. Try to win

        Move? criticalBlock = null;
        int highestThreat = 0;
        foreach (var move in GetAllLegalMoves())
        {
            BoardManager.Instance.tiles[move.x, move.y].Side = opponentSide;
            int threat = EvaluateThreats(opponentSide);
            BoardManager.Instance.tiles[move.x, move.y].Side = Side.None;

            if (threat > highestThreat)
            {
                highestThreat = threat;
                criticalBlock = move;
            }
        }
        if (criticalBlock != null && highestThreat > 1000) // tune threshold if needed
            return criticalBlock;

        foreach (var move in GetAllLegalMoves())
        {
            BoardManager.Instance.tiles[move.x, move.y].Side = currentSide;
            if (BoardManager.Instance.CheckWin(move.x, move.y, currentSide))
            {
                BoardManager.Instance.tiles[move.x, move.y].Side = Side.None;
                return move;
            }
            BoardManager.Instance.tiles[move.x, move.y].Side = Side.None;
        }

        // 2. Block opponent's win
        foreach (var move in GetAllLegalMoves())
        {
            BoardManager.Instance.tiles[move.x, move.y].Side = opponentSide;
            if (BoardManager.Instance.CheckWin(move.x, move.y, opponentSide))
            {
                BoardManager.Instance.tiles[move.x, move.y].Side = Side.None;
                return move;
            }
            BoardManager.Instance.tiles[move.x, move.y].Side = Side.None;
        }

        // 3. Block strong threats (like open 3s or 4s)


        // 4. Otherwise use minimax
        return GetBestMoveByMinimax();
    }

    public Move GetBestMoveByMinimax()
    {
        int bestScore = int.MinValue;
        List<Move> bestMoves = new();

        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (BoardManager.Instance.tiles[x, y].Side == Side.None)
                {
                    BoardManager.Instance.tiles[x, y].Side = currentSide;
                    int score = Minimax(searchDepth - 1, false);
                    BoardManager.Instance.tiles[x, y].Side = Side.None;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear();
                        bestMoves.Add(new Move(x, y));
                    }
                    else if (score == bestScore)
                    {
                        bestMoves.Add(new Move(x, y));
                    }
                }
            }
        }

        // Pick one randomly from the best moves
        if (bestMoves.Count > 0)
            return bestMoves[Random.Range(0, bestMoves.Count)];

        return new Move(0, 0); // fallback
    }



    List<Move> GetAllLegalMoves()
    {
        List<Move> moves = new();
        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (BoardManager.Instance.tiles[x, y].Side == Side.None)
                    moves.Add(new Move(x, y));
            }
        }
        return moves;
    }


    int Minimax(int depth, bool isMaximizing)
    {
        Side winner = EvaluateWinner();
        if (depth == 0 || winner != Side.None)
        {
            return EvaluateBoard();
        }

        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (BoardManager.Instance.tiles[x, y].Side == Side.None)
                {
                    BoardManager.Instance.tiles[x, y].Side = isMaximizing ? currentSide : opponentSide;
                    int score = Minimax(depth - 1, !isMaximizing);
                    BoardManager.Instance.tiles[x, y].Side = Side.None;

                    if (isMaximizing)
                        bestScore = Mathf.Max(score, bestScore);
                    else
                        bestScore = Mathf.Min(score, bestScore);
                }
            }
        }

        return bestScore;
    }

    int EvaluateBoard()
    {
        int score = 0;
        score += EvaluateThreats(currentSide);
        score -= EvaluateThreats(opponentSide) * 2; // Weight opponent threats more to defend
        return score;
    }


    int EvaluatePlayer(Side player)
    {
        int score = 0;
        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (BoardManager.Instance.tiles[x, y].Side == player)
                {
                    score += CountDirection(x, y, 1, 0, player); // Horizontal
                    score += CountDirection(x, y, 0, 1, player); // Vertical
                    score += CountDirection(x, y, 1, 1, player); // Diagonal \
                    score += CountDirection(x, y, 1, -1, player); // Diagonal /
                }
            }
        }
        return score;
    }

    int CountDirection(int x, int y, int dx, int dy, Side player)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (BoardManager.Instance.IsInBounds(nx, ny) &&
               BoardManager.Instance.tiles[nx, ny].Side == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }

    Side EvaluateWinner()
    {
        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                Side side = BoardManager.Instance.tiles[x, y].Side;
                if (side == Side.None) continue;

                if (BoardManager.Instance.CheckWin(x, y, side))
                    return side;
            }
        }
        return Side.None;
    }

    int EvaluateThreats(Side player)
    {
        int score = 0;
        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (BoardManager.Instance.tiles[x, y].Side == player)
                {
                    score += ScoreLine(x, y, 1, 0, player); // horizontal
                    score += ScoreLine(x, y, 0, 1, player); // vertical
                    score += ScoreLine(x, y, 1, 1, player); // diagonal \
                    score += ScoreLine(x, y, 1, -1, player); // diagonal /
                }
            }
        }
        return score;
    }

    int ScoreLine(int x, int y, int dx, int dy, Side player)
    {
        int length = 1;
        int openEnds = 0;

        // Forward
        int nx = x + dx;
        int ny = y + dy;
        while (BoardManager.Instance.IsInBounds(nx, ny) &&
               BoardManager.Instance.tiles[nx, ny].Side == player)
        {
            length++;
            nx += dx;
            ny += dy;
        }
        if (BoardManager.Instance.IsInBounds(nx, ny) &&
            BoardManager.Instance.tiles[nx, ny].Side == Side.None)
            openEnds++;

        // Backward
        nx = x - dx;
        ny = y - dy;
        while (BoardManager.Instance.IsInBounds(nx, ny) &&
               BoardManager.Instance.tiles[nx, ny].Side == player)
        {
            length++;
            nx -= dx;
            ny -= dy;
        }
        if (BoardManager.Instance.IsInBounds(nx, ny) &&
            BoardManager.Instance.tiles[nx, ny].Side == Side.None)
            openEnds++;

        if (length >= 5) return 10000; // Win

        if (length == 4 && openEnds > 0) return 5000;
        if (length == 3 && openEnds > 0) return 1000;
        if (length == 2 && openEnds == 2) return 500;

        return length * 10;
    }

}