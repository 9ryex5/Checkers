using System.Collections.Generic;
using UnityEngine;

public class MGameplay : MonoBehaviour
{

    public Camera myCamera;

    public GameObject prefabCellWhite, prefabCellBlack;
    public GameObject prefabPieceWhite, prefabPieceBlack;
    public GameObject prefabPreviewMove;

    public Transform parentBoard;
    public Transform parentPieces;
    public Transform parentPreviews;

    private Cell[,] board;
    private Vector2Int selectedPiecePos;
    private List<Vector2Int> previewCellsPos;

    private struct Cell
    {
        public Transform pieceTransform;
        public bool isPieceBlack;
        public bool isPieceKing;
        public bool isPreview;
    }

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
            Vector2Int target = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

            if (IsCellBoard(target))
            {
                if (board[target.x, target.y].isPreview)
                {
                    Move(target.x, target.y);
                    DeselectPiece();
                }

                else if ((board[target.x, target.y].pieceTransform != null && board[target.x, target.y].isPieceBlack == blackTurn))
                {
                    DeselectPiece();
                    SelectPiece(target.x, target.y);
                }
            }
        }
    }

    private void DrawBoard()
    {
        board = new Cell[Settings.S.boardSize, Settings.S.boardSize];
        previewCellsPos = new List<Vector2Int>();

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
                        Transform t = Instantiate(prefabPieceWhite, new Vector2(x, y), Quaternion.identity, parentPieces).transform;
                        board[x, y].pieceTransform = t;
                        board[x, y].isPieceBlack = false;
                    }

                    if (y > Settings.S.boardSize / 2)
                    {
                        Transform t = Instantiate(prefabPieceBlack, new Vector2(x, y), Quaternion.identity, parentPieces).transform;
                        board[x, y].pieceTransform = t;
                        board[x, y].isPieceBlack = true;
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
        Vector2Int target = Vector2Int.zero;

        if (blackTurn)
        {
            target = new Vector2Int(_x - 1, _y - 1);
            if (IsCellBoard(target))
            {
                if (board[target.x, target.y].pieceTransform == null)
                {
                    PreviewCell(target);
                }
                else if (board[target.x, target.y].isPieceBlack != blackTurn)
                {
                    target = new Vector2Int(_x - 2, _y - 2);
                    if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
                        PreviewCell(target);
                }
            }

            target = new Vector2Int(_x + 1, _y - 1);
            if (IsCellBoard(target))
            {
                if (board[target.x, target.y].pieceTransform == null)
                {
                    PreviewCell(target);
                }
                else if (board[target.x, target.y].isPieceBlack != blackTurn)
                {
                    target = new Vector2Int(_x + 2, _y - 2);
                    if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
                        PreviewCell(target);
                }
            }
        }
        else
        {
            target = new Vector2Int(_x - 1, _y + 1);
            if (IsCellBoard(target))
            {
                if (board[target.x, target.y].pieceTransform == null)
                {
                    PreviewCell(target);
                }
                else if (board[target.x, target.y].isPieceBlack != blackTurn)
                {
                    target = new Vector2Int(_x - 2, _y + 2);
                    if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
                        PreviewCell(target);
                }
            }

            target = new Vector2Int(_x + 1, _y + 1);
            if (IsCellBoard(target))
            {
                if (board[target.x, target.y].pieceTransform == null)
                {
                    PreviewCell(target);
                }
                else if (board[target.x, target.y].isPieceBlack != blackTurn)
                {
                    target = new Vector2Int(_x + 2, _y + 2);
                    if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
                        PreviewCell(target);
                }
            }
        }

        selectedPiecePos = new Vector2Int(_x, _y);
    }

    private void PreviewCell(Vector2Int _target)
    {
        Instantiate(prefabPreviewMove, (Vector2)_target, Quaternion.identity, parentPreviews);
        board[_target.x, _target.y].isPreview = true;
        previewCellsPos.Add(_target);
    }

    private void DeselectPiece()
    {
        for (int i = 0; i < previewCellsPos.Count; i++)
            board[previewCellsPos[i].x, previewCellsPos[i].y].isPreview = false;

        previewCellsPos = new List<Vector2Int>();

        for (int i = 0; i < parentPreviews.childCount; i++)
            Destroy(parentPreviews.GetChild(i).gameObject);
    }

    private void Move(int _toX, int _toY)
    {
        if (Mathf.Abs(_toY - selectedPiecePos.y) > 1)
            Destroy(board[(_toX + selectedPiecePos.x) / 2, (_toY + selectedPiecePos.y) / 2].pieceTransform.gameObject);

        board[_toX, _toY].pieceTransform = board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform;
        board[_toX, _toY].isPieceBlack = board[selectedPiecePos.x, selectedPiecePos.y].isPieceBlack;
        board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform.position = new Vector2(_toX, _toY);
        board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform = null;

        blackTurn = !blackTurn;
    }

    private bool IsCellBoard(Vector2Int _cell)
    {
        return _cell.x >= 0 && _cell.x < Settings.S.boardSize && _cell.y >= 0 && _cell.y < Settings.S.boardSize;
    }
}
