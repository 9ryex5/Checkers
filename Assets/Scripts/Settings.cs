using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings S;

    public int boardSize;
    public bool mandatoryCapture;
    public bool flyingKing;

    private void Awake()
    {
        S = this;
    }
}
