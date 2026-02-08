using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    public bool freeze = false;

    public void Initialize(Board board, Tetronimo tetronimo)
    {
        // set a reference to the board object
        this.board = board;

        // search for the tetronimo data and assign
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // create a copy of the tetronimo cell locations
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++) cells[i] = data.cells[i];

        // Set the start position of the piece
        position = board.startPosition;
    }

    private void Update()
    {
        if (board.tetrisManager.gameOver) return;

        if (freeze) return;

        board.Clear(this);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            // if we hard drop -- do NOT process any other piece motion
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }

            // Rotation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Rotate(1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Rotate(-1);
            }
        }

        board.Set(this);

        // We only check the board and spawn the new piece AFTER the final piece has been moved
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }
    }

    void Rotate(int direction)
    {
        Vector2Int[] temporaryCells = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++) temporaryCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks())
            {
                RevertRotation(temporaryCells);
            }
            else
            {
                // Wall kicks succeeded
            }
        }
        else
        {
            // Valid Rotation
        }
    }

    bool TryWallKicks()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>()
        {
            Vector2Int.left,
            Vector2Int.down,
            Vector2Int.right,
            new Vector2Int(-1, -1), // Diagonal down-left
            new Vector2Int(1, -1), // Diagonal down-right
        };

        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);
        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    void RevertRotation(Vector2Int[] temporaryCells)
    {
        for (int i = 0; i < cells.Length; i++) cells[i] = temporaryCells[i];
    }

    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90.0f * direction);

        bool isSpecial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;

        for (int i = 0; i < data.cells.Length; i++)
        {
            // Convert cell location a vector3 to work with quaternions
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            if (isSpecial)
            {
                cellPosition.x -= 0.5f;
                cellPosition.y -= 0.5f;
            }

            // Get the result
            Vector3 result = rotation * cellPosition;

            // Put it back in the cells data
            if (isSpecial)
            {
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }
        }
    }

    void HardDrop()
    {
        // Keep moving down until the translation is invalid
        while (Move(Vector2Int.down))
        {
            // do nothing
        }

        freeze = true;
    }

    // Move will return whether or not the translation is valid
    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool isValid = board.IsPositionValid(this, newPosition);
        if (isValid) position += translation;

        return isValid;
    }

}
