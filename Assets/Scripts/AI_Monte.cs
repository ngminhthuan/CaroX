using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Monte : Player
{
    public int simulationsPerMove = 30;
    public override void NotifyTurnToMove()
    {
        base.NotifyTurnToMove();
        StartCoroutine(this.MakeMove());

    }
    IEnumerator MakeMove()
    {
        yield return new WaitForSeconds(2f);
        Move bestMove = GetBestMove();
        if (bestMove == null)
        {
            Debug.LogWarning("Monte Carlo AI did not find a move.");
            yield break;
        }

        BoardManager.Instance.PlaceMark(bestMove.x, bestMove.y);
        ChoseMove(bestMove, this);
    }

    public Move GetBestMove()
    {
        List<Move> legalMoves = GetAllLegalMoves();

        if (legalMoves == null || legalMoves.Count == 0)
        {
            Debug.LogWarning("No legal moves available for Monte Carlo AI.");
            return null;
        }

        foreach (var move in legalMoves)
        {
            var boardCopy = CloneBoard();
            boardCopy[move.x, move.y].Side = currentSide;

            if (CheckWinSim(move.x, move.y, currentSide, boardCopy))
            {
                Debug.Log("AI wins immediately with move: " + move);
                return move;
            }
        }

        foreach (var move in legalMoves)
        {
            var boardCopy = CloneBoard();
            boardCopy[move.x, move.y].Side = opponentSide;

            if (CheckWinSim(move.x, move.y, opponentSide, boardCopy))
            {
                Debug.Log("AI blocks opponent's win with move: " + move);
                return move;
            }
        }

        Move bestMove = legalMoves[0];
        float bestWinRate = -1f;

        foreach (var move in legalMoves)
        {
            int wins = 0;
            int i = 0;

            for (; i < simulationsPerMove; i++)
            {
                if (SimulateGame(move))
                    wins++;

                if (wins == i + 1 && wins >= 15)
                    break;
            }

            float winRate = (float)wins / (i + 1);

            if (winRate > bestWinRate)
            {
                bestWinRate = winRate;
                bestMove = move;

                if (winRate == 1f)
                    break;
            }
        }

        return bestMove;
    }


    List<Move> GetAllLegalMoves()
    {
        return GetAllLegalMoves(BoardManager.Instance.tiles);
    }

    List<Move> GetAllLegalMoves(Tile[,] tiles)
    {
        List<Move> moves = new();
        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardSize; y++)
            {
                if (tiles[x, y].Side == Side.None)
                    moves.Add(new Move(x, y));
            }
        }
        return moves;
    }

    bool SimulateGame(Move firstMove)
    {
        var boardCopy = CloneBoard(); // use local variable

        boardCopy[firstMove.x, firstMove.y].Side = currentSide;

        if (CheckWinSim(firstMove.x, firstMove.y, currentSide, boardCopy))
            return true;

        List<Move> moves = GetAllLegalMoves(boardCopy);

        for (int i = moves.Count - 1; i >= 0; i--)
        {
            if (moves[i].x == firstMove.x && moves[i].y == firstMove.y)
                moves.RemoveAt(i);
        }

        Side currentPlayer = opponentSide;
        int maxSteps = boardCopy.Length;
        int steps = 0;

        while (moves.Count > 0 && steps < maxSteps)
        {
            steps++;
            Move randomMove = moves[Random.Range(0, moves.Count)];
            boardCopy[randomMove.x, randomMove.y].Side = currentPlayer;
            moves.Remove(randomMove);

            if (CheckWinSim(randomMove.x, randomMove.y, currentPlayer, boardCopy))
                return currentPlayer == currentSide;

            currentPlayer = (currentPlayer == currentSide) ? opponentSide : currentSide;
        }

        return false;
    }

    SimTile[,] CloneBoard()
    {
        int size = BoardManager.Instance.boardSize;
        SimTile[,] copy = new SimTile[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                copy[x, y] = new SimTile();
                copy[x, y].Side = BoardManager.Instance.tiles[x, y].Side;
            }
        }

        return copy;
    }


    List<Move> GetAllLegalMoves(SimTile[,] board)
    {
        List<Move> moves = new();
        int size = BoardManager.Instance.boardSize;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (board[x, y].Side == Side.None)
                    moves.Add(new Move(x, y));
            }
        }
        return moves;
    }

    bool CheckWinSim(int x, int y, Side player, SimTile[,] board)
    {
        return CountInDirectionSim(x, y, 1, 0, player, board) + CountInDirectionSim(x, y, -1, 0, player, board) >= 4 ||
               CountInDirectionSim(x, y, 0, 1, player, board) + CountInDirectionSim(x, y, 0, -1, player, board) >= 4 ||
               CountInDirectionSim(x, y, 1, 1, player, board) + CountInDirectionSim(x, y, -1, -1, player, board) >= 4 ||
               CountInDirectionSim(x, y, 1, -1, player, board) + CountInDirectionSim(x, y, -1, 1, player, board) >= 4;
    }

    int CountInDirectionSim(int x, int y, int dx, int dy, Side player, SimTile[,] board)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (nx >= 0 && ny >= 0 && nx < board.GetLength(0) && ny < board.GetLength(1)
               && board[nx, ny].Side == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }

}