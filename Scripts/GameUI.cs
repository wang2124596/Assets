using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    public static GameUI Instance;
    public Text TextPing;
    public Image BloodBar;
    public string Ping { get { return TextPing.text; } set { TextPing.text = value; } }

    NetManager net;
    void Awake()
    {
        Instance = this;
    }
	// Use this for initialization
	void Start () {
        net = NetManager.Instance;
	
	}
	
	// Update is called once per frame
	void Update () {
        if(net.Connected)
        {
            Ping = net.Ping.ToString();
        }
        else
        {
            Ping = "9999";
        }
    }
}
