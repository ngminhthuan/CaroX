using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int boardSize = 15;
    public GameObject tilePrefab;
    public Tile[,] tiles;
    public Side currentSide = Side.X; // 1 = X, 2 = O

    public static BoardManager Instance;

    [SerializeField] PlayerType X_PlayerType;
    [SerializeField] PlayerType O_PlayerType;

    Player X_Player;
    Player O_Player;
    Player playerToMove;

    [SerializeField] AI_MinMax AI_MinMax;
    [SerializeField] AI_Monte AI_Monte;
    public Player CurrentPlayer => playerToMove;
    bool is_X_Turn;
    Result gameResult;
    public TMP_Text Result_Text;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    void Start()
    {

    }
    #region Board
    public void CreateBoard(PlayerType xPlayerType , PlayerType oPlayerType )
    {
        if (tiles == null)
        {
            tiles = new Tile[boardSize, boardSize];
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                    tileGO.transform.parent = this.transform;
                    Tile tile = tileGO.GetComponent<Tile>();
                    tile.SetPosition(x, y);
                    tiles[x, y] = tile;
                }
            }

        }
        else
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    tiles[x, y].SetState(Side.None);
                }
            }
        }
        Result_Text.text = "";
        X_PlayerType = xPlayerType;
        O_PlayerType = oPlayerType;                                                             
        CreatePlayer(ref X_Player, X_PlayerType).SetSide(Side.X);
        CreatePlayer(ref O_Player, O_PlayerType).SetSide(Side.O);
        this.is_X_Turn = true;
        this.playerToMove = X_Player;
        this.gameResult = Result.Playing;
        NotifyPlayerToMove();
    }

    public void PlaceMark(int x, int y)
    {
        if (tiles[x, y].Side != Side.None) return;
        tiles[x, y].SetState(currentSide);
        string PlayerWinType = "";
        if (CheckWin(x, y, currentSide))
        {
            if (currentSide == Side.X)
            {
                this.gameResult = Result.XisWin;
                PlayerWinType = X_PlayerType.ToString();
            }
            else
            {
                this.gameResult = Result.OisWin;
                PlayerWinType = O_PlayerType.ToString();
            }
            Result_Text.text = PlayerWinType + " in " + currentSide + " side win!";
            return;
        }

        currentSide = (currentSide == Side.X) ? Side.O : Side.X;
    }

    public bool CheckWin(int x, int y, Side player)
    {
        return (CountInDirection(x, y, 1, 0, player) + CountInDirection(x, y, -1, 0, player) >= 4) || // Horizontal
               (CountInDirection(x, y, 0, 1, player) + CountInDirection(x, y, 0, -1, player) >= 4) || // Vertical
               (CountInDirection(x, y, 1, 1, player) + CountInDirection(x, y, -1, -1, player) >= 4) || // Diagonal \
               (CountInDirection(x, y, 1, -1, player) + CountInDirection(x, y, -1, 1, player) >= 4);   // Diagonal /
    }


    int CountInDirection(int x, int y, int dx, int dy, Side player)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (IsInBounds(nx, ny) && tiles[nx, ny].Side == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }

        return count;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < boardSize && y < boardSize;
    }
    #endregion

    #region Player
    Player CreatePlayer(ref Player player, PlayerType playerType)
    {
        if (player != null)
        {
            player.onMoveChosen -= OnMoveChosen;
        }

        if (playerType == PlayerType.Human)
        {
            player = new HumanPlayer();
            Debug.Log("Player");
        }

        else if (playerType == PlayerType.AI_MinMax)
        {
            player = this.AI_MinMax;
            Debug.Log("AI Minmax");

        }
        else if (playerType == PlayerType.AI_Monte)
        {
            player = this.AI_Monte;
            Debug.Log("AI Monte");
        }
        player.onMoveChosen += OnMoveChosen;
        return player;
    }

    void OnMoveChosen(Move move, Player player)
    {
        this.is_X_Turn = !this.is_X_Turn;
        NotifyPlayerToMove();
    }
    #endregion

    void NotifyPlayerToMove()
    {
        if (gameResult == Result.Playing)
        {
            playerToMove = (this.is_X_Turn) ? X_Player : O_Player;

            // Log board state
            int freeTiles = 0;
            foreach (var tile in BoardManager.Instance.tiles)
                if (tile.Side == Side.None) freeTiles++;

            if (freeTiles <= 0 && this.gameResult == Result.Playing) { 
                this.gameResult = Result.Stalement;
                this.Result_Text.text = "Stalement";
            }

            playerToMove.NotifyTurnToMove();
        }
        else
        {
            Debug.Log("Game Over");
        }
    }


    public bool isEndGame()
    {
        return gameResult != Result.Playing;
    }

}
public enum Result
{
    Playing,
    Stalement,
    XisWin,
    OisWin
}