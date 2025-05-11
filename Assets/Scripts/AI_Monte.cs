using System.Collections.Generic;
using UnityEngine;

public class AI_Monte : Player
{
    public int simulationsPerMove = 100;
    SimTile[,] boardCopy;
    public override void NotifyTurnToMove()
    {
        base.NotifyTurnToMove();
        Debug.Log("Monte Carlo selected move: ");
        Move bestMove = GetBestMove();
        BoardManager.Instance.PlaceMark(bestMove.x, bestMove.y);
        ChoseMove(bestMove, this);
    }

    public Move GetBestMove()
    {
        List<Move> legalMoves = GetAllLegalMoves();
        Move bestMove = legalMoves[0];
        float bestWinRate = -1f;

        foreach (var move in legalMoves)
        {
            int wins = 0;

            for (int i = 0; i < simulationsPerMove; i++)
            {
                if (SimulateGame(move))
                    wins++;
            }

            float winRate = (float)wins / simulationsPerMove;
            if (winRate > bestWinRate)
            {
                bestWinRate = winRate;
                bestMove = move;
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
        CloneBoard();

        this.boardCopy[firstMove.x, firstMove.y].Side = currentSide;

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

    void CloneBoard()
    {
        int size = BoardManager.Instance.boardSize;
        if (this.boardCopy == null)
        {
            this.boardCopy = new SimTile[size, size];
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                this.boardCopy[x, y] = new SimTile();
                this.boardCopy[x, y].Side = BoardManager.Instance.tiles[x, y].Side;
            }
        }
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