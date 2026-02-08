using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    // Public Variables -------------------------------
    public TetrisManager tetrisManager;

    // This contains a reference to the piece prefab
    public Piece piecePrefab;
    public Tilemap tilemap;

    public TetronimoData[] tetronimos;


    public Vector2Int boardSize;
    public Vector2Int startPosition;

    public float dropInterval = 0.5f;

    public int turn = 0;

    float dropTime = 0.0f;


    // Private Variables ------------------------------
    Piece activePiece;

    int left
    {
        get { return -boardSize.x / 2; }
    }

    int right
    {
        get { return boardSize.x / 2; }
    }

    int top
    {
        get { return boardSize.y / 2; }
    }

    int bottom
    {
        get { return -boardSize.y / 2; }
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;

        dropTime += Time.deltaTime;

        if (dropTime > dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            bool moveresult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            // If the move fails, that means the piece is stuck / so that ends that placement turn
            if (!moveresult)
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(piecePrefab);

        if(turn == 0)
        {
            Tetronimo t = Tetronimo.U;
            activePiece.Initialize(this, t);
        }
        else if (turn == 1)
        {
            Tetronimo t = Tetronimo.O;
            activePiece.Initialize(this, t);
        }
        else if (turn == 2)
        {
            Tetronimo t = Tetronimo.U;
            activePiece.Initialize(this, t);
        }
        else if (turn == 3)
        {
            Tetronimo t = Tetronimo.L;
            activePiece.Initialize(this, t);
        }
        else if (turn == 4)
        {
            Tetronimo t = Tetronimo.O;
            activePiece.Initialize(this, t);
        }
        else if (turn == 5)
        {
            Tetronimo t = Tetronimo.U;
            activePiece.Initialize(this, t);
        }
        else
        {
            tetrisManager.SetGameOver(true);
        }

        CheckEndGame();

        Set(activePiece);
        turn++;
    }

    void CheckEndGame()
    {
        if(!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        // gameOver being false means we either started a new game or reset the game
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        // Clear the gameObjects
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;

        //tilemap.ClearAllTiles();

        SpawnPiece();
    }

    // Set will color in the tiles for a piece
    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // Bounds check
            if (cellPosition.x < left || cellPosition.x >= right || 
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            // This checks if this position is occupied in the tilemap
            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();
        for(int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        // We shift down here
        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);

            // At the end of every loop, we have shifted rows down by 1 more
            rowsShiftedDown++;
        }

        // Setting the score:
        int score = tetrisManager.CalculateScore(destroyedLines.Count);

        tetrisManager.ChangeScore(score);

    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition)) return false ;
        }
        return true;
    }

    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            tilemap.SetTile(cellPosition, null);
        }
    }
    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                // Store the tile temporarily
                TileBase currentTile = tilemap.GetTile(cellPosition);

                // Clear the position it is in
                tilemap.SetTile(cellPosition, null);

                // Move the tile down
                cellPosition.y -= 1;
                tilemap.SetTile(cellPosition, currentTile);                
            }
        }
    }

}
