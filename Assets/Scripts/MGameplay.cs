using UnityEngine;

public class MGameplay : MonoBehaviour
{

    public GameObject prefabCellWhite, prefabCellBlack;
    public GameObject prefabPieceWhite, prefabPieceBlack;

    public Transform parentBoard;
    public Transform parentPieces;

    private void Start()
    {
        bool evenRow = false;
        bool evenLine = false;

        for (int i = 0; i < Settings.S.boardSize; i++)
        {
            for (int j = 0; j < Settings.S.boardSize; j++)
            {
                if ((evenLine && evenRow || !evenLine && !evenRow))
                {
                    Instantiate(prefabCellBlack, new Vector2(j, i), Quaternion.identity, parentBoard);

                    if (i < Settings.S.boardSize / 2 - 1)
                        Instantiate(prefabPieceWhite, new Vector2(j, i), Quaternion.identity, parentPieces);

                    if (i > Settings.S.boardSize / 2)
                        Instantiate(prefabPieceBlack, new Vector2(j, i), Quaternion.identity, parentPieces);
                }
                else
                {
                    Instantiate(prefabCellWhite, new Vector2(j, i), Quaternion.identity, parentBoard);
                }

                evenRow = !evenRow;
            }
            evenRow = false;
            evenLine = !evenLine;
        }
    }
}
