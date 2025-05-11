using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    public Side Side = Side.None; // 0 = empty, 1 = Player X, 2 = Player O
    [SerializeField] GameObject X_Sprite;
    [SerializeField] GameObject O_Sprite;

    public void OnEnable()
    {
        UpdateSprite();
    }
    void OnMouseDown()
    {
        // Check if it's the human player's turn and the tile is empty
        if (Side == Side.None && BoardManager.Instance.CurrentPlayer is HumanPlayer humanPlayer && !BoardManager.Instance.isEndGame())
        {
            // Create move and notify human player
            Move move = new Move(x, y);
            Debug.Log("PLayer move");
            humanPlayer.PlayerMakeMove(move, humanPlayer);
        }
    }


    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetState(Side player)
    {
        Side = player;
        UpdateSprite();
        // Visual: show X or O
        // Example: this.GetComponent<SpriteRenderer>().sprite = (player == 1) ? Xsprite : Osprite;
    }

    public void UpdateSprite()
    {
        this.X_Sprite.SetActive(Side == Side.X);
        this.O_Sprite.SetActive(Side == Side.O);
    }
}


public class SimTile
{
    public Side Side = Side.None;
}