using UnityEngine;

public class MGameplay : MonoBehaviour
{

    public Camera myCamera;

    public GameObject prefabCellWhite, prefabCellBlack;
    public GameObject prefabPieceWhite, prefabPieceBlack;
    public GameObject prefabPossibleMove;

    public Transform parentBoard;
    public Transform parentPieces;

    private Cell[,] board;
    private enum Cell { EMPTY, PW, PB }

    private bool blackTurn;

    private void Start()
    {
        myCamera.transform.position = new Vector3(Settings.S.boardSize / 2 - 0.5f, Settings.S.boardSize / 2 - 0.5f, -1);
        myCamera.orthographicSize = Settings.S.boardSize / 2;

        DrawBoard();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float x = myCamera.ScreenToWorldPoint(Input.mousePosition).x;
            float y = myCamera.ScreenToWorldPoint(Input.mousePosition).y;
            Vector2Int currentCell = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

            if (IsCellBoard(currentCell) && (board[currentCell.x, currentCell.y] == Cell.PW && !blackTurn || board[currentCell.x, currentCell.y] == Cell.PB && blackTurn))
                SelectPiece(currentCell.x, currentCell.y);
        }
    }

    private void DrawBoard()
    {
        board = new Cell[Settings.S.boardSize, Settings.S.boardSize];

        bool evenRow = false;
        bool evenLine = false;

        for (int y = 0; y < Settings.S.boardSize; y++)
        {
            for (int x = 0; x < Settings.S.boardSize; x++)
            {
                if ((evenLine && evenRow || !evenLine && !evenRow))
                {
                    Instantiate(prefabCellBlack, new Vector2(x, y), Quaternion.identity, parentBoard);

                    if (y < Settings.S.boardSize / 2 - 1)
                    {
                        Instantiate(prefabPieceWhite, new Vector2(x, y), Quaternion.identity, parentPieces);
                        board[x, y] = Cell.PW;
                    }

                    if (y > Settings.S.boardSize / 2)
                    {
                        Instantiate(prefabPieceBlack, new Vector2(x, y), Quaternion.identity, parentPieces);
                        board[x, y] = Cell.PB;
                    }
                }
                else
                {
                    Instantiate(prefabCellWhite, new Vector2(x, y), Quaternion.identity, parentBoard);
                }

                evenRow = !evenRow;
            }
            evenRow = false;
            evenLine = !evenLine;
        }
    }

    private void SelectPiece(int _x, int _y)
    {
        if (blackTurn)
        {
            if (IsCellBoard(new Vector2Int(_x - 1, _y - 1)))
                Instantiate(prefabPossibleMove, new Vector2(_x - 1, _y - 1), Quaternion.identity);

            if (IsCellBoard(new Vector2Int(_x + 1, _y - 1)))
                Instantiate(prefabPossibleMove, new Vector2(_x + 1, _y - 1), Quaternion.identity);
        }
        else
        {
            if (IsCellBoard(new Vector2Int(_x - 1, _y + 1)))
                Instantiate(prefabPossibleMove, new Vector2(_x - 1, _y + 1), Quaternion.identity);

            if (IsCellBoard(new Vector2Int(_x + 1, _y + 1)))
                Instantiate(prefabPossibleMove, new Vector2(_x + 1, _y + 1), Quaternion.identity);
        }
    }

    private bool IsCellBoard(Vector2Int _cell)
    {
        return _cell.x >= 0 && _cell.x < Settings.S.boardSize && _cell.y >= 0 && _cell.y < Settings.S.boardSize;
    }
}
