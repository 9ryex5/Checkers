using System.Collections.Generic;
using UnityEngine;

public class ManagerGameplay : MonoBehaviour
{
    public static ManagerGameplay MG;

    public Camera myCamera;

    public GameObject prefabCellWhite, prefabCellBlack;
    public GameObject prefabPieceWhite, prefabPieceBlack;
    public GameObject prefabPreviewMove;
    public GameObject prefabKing;

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
    private bool canCapture;
    private bool isPlaying;

    private void Awake()
    {
        MG = this;
    }

    private void Start()
    {
        myCamera.transform.position = new Vector3(Settings.S.boardSize / 2 - 0.5f, Settings.S.boardSize / 2 - 0.5f, -1);
        myCamera.orthographicSize = Settings.S.boardSize / 2;

        RestartMatch();
    }

    private void Update()
    {
        if (!isPlaying) return;

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
                }

                else if ((board[target.x, target.y].pieceTransform != null && board[target.x, target.y].isPieceBlack == blackTurn))
                {
                    DeselectPiece();
                    SelectPiece(target.x, target.y);
                }
            }
        }
    }

    public void RestartMatch()
    {
        ClearBoard();
        DrawBoard();
        isPlaying = true;
        blackTurn = false;
        canCapture = false;
        ManagerUI.MUI.UpdateTurn(false);
    }

    private void ClearBoard()
    {
        for (int i = 0; i < parentPieces.childCount; i++)
            Destroy(parentPieces.GetChild(i).gameObject);
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
        List<Vector2Int> moves = PossibleMoves(_x, _y);

        if (Settings.S.mandatoryCapture && canCapture)
        {
            for (int i = 0; i < moves.Count; i++)
                if (IsMoveCapture(_x, _y, moves[i].x, moves[i].y)) PreviewCell(moves[i]);
        }
        else
        {
            for (int i = 0; i < moves.Count; i++)
                PreviewCell(moves[i]);
        }

        selectedPiecePos = new Vector2Int(_x, _y);
    }

    private List<Vector2Int> PossibleMoves(int _x, int _y)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int target = Vector2Int.zero;

        if (Settings.S.flyingKing && board[_x, _y].isPieceKing)
        {
            for (int i = 1; i < Settings.S.boardSize - 1; i++)
            {
                //Left
                if (IsPossibleSimple(_x - i, _y + (blackTurn ? -i : i)))
                    moves.Add(new Vector2Int(_x - i, _y + (blackTurn ? -i : i)));
                else if (IsPossibleCapture(_x - i, _y + (blackTurn ? -i : i), _x - i - 1, _y + (blackTurn ? -i - 1 : i + 1)))
                    moves.Add(new Vector2Int(_x - i - 1, _y + (blackTurn ? -i - 1 : i + 1)));
                else
                    break;
            }

            for (int i = 1; i < Settings.S.boardSize - 1; i++)
            {
                //Right
                if (IsPossibleSimple(_x + i, _y + (blackTurn ? -i : i)))
                    moves.Add(new Vector2Int(_x + i, _y + (blackTurn ? -i : i)));
                else if (IsPossibleCapture(_x + i, _y + (blackTurn ? -i : i), _x - i - 1, _y + (blackTurn ? -i - 1 : i + 1)))
                    moves.Add(new Vector2Int(_x + i + 1, _y + (blackTurn ? -i - 1 : i + 1)));
                else
                    break;
            }

            for (int i = 1; i < Settings.S.boardSize - 1; i++)
            {
                //Left
                if (IsPossibleSimple(_x - i, _y + (!blackTurn ? -i : i)))
                    moves.Add(new Vector2Int(_x - i, _y + (!blackTurn ? -i : i)));
                else if (IsPossibleCapture(_x - i, _y + (!blackTurn ? -i : i), _x - i - 1, _y + (!blackTurn ? -i - 1 : i + 1)))
                    moves.Add(new Vector2Int(_x - i - 1, _y + (!blackTurn ? -i - 1 : i + 1)));
                else
                    break;
            }

            for (int i = 1; i < Settings.S.boardSize - 1; i++)
            {
                //Right
                if (IsPossibleSimple(_x + i, _y + (!blackTurn ? -i : i)))
                    moves.Add(new Vector2Int(_x + i, _y + (!blackTurn ? -i : i)));
                else if (IsPossibleCapture(_x + i, _y + (!blackTurn ? -i : i), _x - i - 1, _y + (!blackTurn ? -i - 1 : i + 1)))
                    moves.Add(new Vector2Int(_x + i + 1, _y + (!blackTurn ? -i - 1 : i + 1)));
                else
                    break;
            }
            foreach (Vector2Int v in moves)
            {
                Debug.Log("x" + v.x + "y" + v.y);
            }
            return moves;
        }

        //Left
        if (IsPossibleSimple(_x - 1, _y + (blackTurn ? -1 : 1))) moves.Add(new Vector2Int(_x - 1, _y + (blackTurn ? -1 : 1)));
        if (IsPossibleCapture(_x - 1, _y + (blackTurn ? -1 : 1), _x - 2, _y + (blackTurn ? -2 : 2))) moves.Add(new Vector2Int(_x - 2, _y + (blackTurn ? -2 : 2)));
        //Right
        if (IsPossibleSimple(_x + 1, _y + (blackTurn ? -1 : 1))) moves.Add(new Vector2Int(_x + 1, _y + (blackTurn ? -1 : 1)));
        if (IsPossibleCapture(_x + 1, _y + (blackTurn ? -1 : 1), _x + 2, _y + (blackTurn ? -2 : 2))) moves.Add(new Vector2Int(_x + 2, _y + (blackTurn ? -2 : 2)));

        if (board[_x, _y].isPieceKing)
        {
            //Left
            if (IsPossibleSimple(_x - 1, _y + (!blackTurn ? -1 : 1))) moves.Add(new Vector2Int(_x - 1, _y + (!blackTurn ? -1 : 1)));
            if (IsPossibleCapture(_x - 1, _y + (!blackTurn ? -1 : 1), _x - 2, _y + (!blackTurn ? -2 : 2))) moves.Add(new Vector2Int(_x - 2, _y + (!blackTurn ? -2 : 2)));
            //Right
            if (IsPossibleSimple(_x + 1, _y + (!blackTurn ? -1 : 1))) moves.Add(new Vector2Int(_x + 1, _y + (!blackTurn ? -1 : 1)));
            if (IsPossibleCapture(_x + 1, _y + (!blackTurn ? -1 : 1), _x + 2, _y + (!blackTurn ? -2 : 2))) moves.Add(new Vector2Int(_x + 2, _y + (!blackTurn ? -2 : 2)));
        }

        return moves;
    }

    private bool IsPossibleSimple(int _x, int _y)
    {
        Vector2Int target = new Vector2Int(_x, _y);
        if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
            return true;

        return false;
    }

    private bool IsPossibleCapture(int _stepX, int _stepY, int _landX, int _landY)
    {
        Vector2Int target = new Vector2Int(_stepX, _stepY);
        if (IsCellBoard(target) && board[target.x, target.y].pieceTransform != null && board[target.x, target.y].isPieceBlack != blackTurn)
        {
            target = new Vector2Int(_landX, _landY);
            if (IsCellBoard(target) && board[target.x, target.y].pieceTransform == null)
                return true;
        }

        return false;
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

    private bool IsMoveCapture(int _fromX, int _fromY, int _toX, int _toY)
    {
        for (int i = 1; i < Mathf.Abs(_toY - _fromY); i++)
            if (board[_fromX + (_fromX < _toX ? i : -i), _fromY + (_fromY < _toY ? i : -i)].isPieceBlack != blackTurn) return true;

        return false;
    }

    private void Move(int _toX, int _toY)
    {
        bool playAgain = false;

        board[_toX, _toY].pieceTransform = board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform;
        board[_toX, _toY].isPieceBlack = board[selectedPiecePos.x, selectedPiecePos.y].isPieceBlack;
        board[_toX, _toY].isPieceKing = board[selectedPiecePos.x, selectedPiecePos.y].isPieceKing;
        board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform.position = new Vector2(_toX, _toY);
        board[selectedPiecePos.x, selectedPiecePos.y].pieceTransform = null;

        if (IsMoveCapture(selectedPiecePos.x, selectedPiecePos.y, _toX, _toY))
        {
            Destroy(board[(_toX + selectedPiecePos.x) / 2, (_toY + selectedPiecePos.y) / 2].pieceTransform.gameObject);
            board[(_toX + selectedPiecePos.x) / 2, (_toY + selectedPiecePos.y) / 2].pieceTransform = null;

            List<Vector2Int> movesAfterCapture = PossibleMoves(_toX, _toY);

            for (int i = 0; i < movesAfterCapture.Count; i++)
                if (IsMoveCapture(_toX, _toY, movesAfterCapture[i].x, movesAfterCapture[i].y)) playAgain = true;
        }

        if (_toY == (blackTurn ? 0 : Settings.S.boardSize - 1)) Promote(_toX, _toY);

        DeselectPiece();

        if (playAgain) SelectPiece(_toX, _toY);
        else ChangeTurn();
    }

    private void Promote(int _x, int _y)
    {
        Instantiate(prefabKing, new Vector2(_x, _y), Quaternion.identity, board[_x, _y].pieceTransform);
        board[_x, _y].isPieceKing = true;
    }

    private void ChangeTurn()
    {
        blackTurn = !blackTurn;

        if (CheckWin()) return;

        canCapture = false;
        if (Settings.S.mandatoryCapture) CheckCanCapture();
        ManagerUI.MUI.UpdateTurn(blackTurn);
    }

    private bool CheckWin()
    {
        for (int y = 0; y < Settings.S.boardSize; y++)
            for (int x = 0; x < Settings.S.boardSize; x++)
                if (board[x, y].pieceTransform != null && board[x, y].isPieceBlack == blackTurn)
                    return false;

        isPlaying = false;
        ManagerUI.MUI.Win(!blackTurn);

        return true;
    }

    private void CheckCanCapture()
    {
        for (int y = 0; y < Settings.S.boardSize; y++)
            for (int x = 0; x < Settings.S.boardSize; x++)
                if (board[x, y].pieceTransform != null && board[x, y].isPieceBlack == blackTurn)
                {
                    List<Vector2Int> moves = PossibleMoves(x, y);
                    foreach (Vector2Int v in moves)
                    {
                        if (IsMoveCapture(x, y, v.x, v.y))
                        {
                            canCapture = true;
                            return;
                        }
                    }
                }
    }

    private bool IsCellBoard(Vector2Int _cell)
    {
        return _cell.x >= 0 && _cell.x < Settings.S.boardSize && _cell.y >= 0 && _cell.y < Settings.S.boardSize;
    }
}
