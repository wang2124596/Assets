using UnityEngine;
using System.Collections;

public class GameSettings : MonoBehaviour {
    public static GameSettings Instance;
    public GameObject Player;
    public bool IsPlayerClient = false;
    public bool UnoptimizeMode = false;

    void Awake()
    {
        Instance = this;
    }
}
