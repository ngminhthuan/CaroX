using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public void PlayerMakeMove(Move move, Player player)
    {
        BoardManager.Instance.PlaceMark(move.x, move.y);
        ChoseMove(move, this);
    }
}
