using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings S;

    public int boardSize;

    private void Awake()
    {
        S = this;
    }
}
