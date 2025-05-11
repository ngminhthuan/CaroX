using System;
using System.Drawing;
using UnityEngine;

public class Player 
{
    public event System.Action<Move, Player> onMoveChosen;
    public Side currentSide = Side.O;
    public Side opponentSide = Side.X;

    public void SetSide(Side currentSide)
    {
        this.currentSide = currentSide;
        this.opponentSide = (currentSide == Side.X) ? Side.O : Side.X;
        Debug.Log("Current Side: " + currentSide);
    }
    public virtual void Update()
    {
    }

    public virtual void NotifyTurnToMove()
    {

    }

    protected virtual void ChoseMove(Move move, Player player)
    {
        onMoveChosen?.Invoke(move, this);
    }
}

public enum PlayerType { Human, AI_MinMax, AI_Monte }

public enum Side { None, X, O}