using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodBarController : MonoBehaviour {

    public GameObject Player;
    public Image Blood;
    SoliderBehaviour sb;
    int previousLive;
	// Use this for initialization
	void Start () {
        if (Player == null)
            Player = GameObject.Find("Soldier");

        sb = Player.GetComponent<SoliderBehaviour>();
        previousLive = sb.Life;
        SetLive(sb.Life);
	}
	
	// Update is called once per frame
	void Update () {
        if(sb.Life != previousLive)
        {
            Debug.Log(sb.Life);
            SetLive(sb.Life);
            previousLive = sb.Life;
        }
	}

    void SetLive(int live)
    {
        if(live >= 100)
        {
            Blood.rectTransform.sizeDelta = new Vector2(100, 10);
        }
        else if(live >= 0)
        {
            Blood.rectTransform.sizeDelta = new Vector2(live, 10);
        }
    }
}
