using UnityEngine;
using UnityEngine.UI;

public class ManagerUI : MonoBehaviour
{
    public static ManagerUI MUI;

    public Text textTurn;
    public Text textWin;
    public GameObject buttonRestart;

    private void Awake()
    {
        MUI = this;
    }

    private void Start()
    {
        UpdateTurn(false);
    }

    public void UpdateTurn(bool _black)
    {
        textTurn.text = _black ? "Black" : "White";
    }

    public void Win(bool _black)
    {
        textWin.text = _black ? "Black Wins!" : "White Wins!";
        buttonRestart.gameObject.SetActive(true);
    }

    public void ButtonRestartMatch()
    {
        ManagerGameplay.MG.RestartMatch();

        textWin.gameObject.SetActive(false);
        buttonRestart.SetActive(false);
    }
}
