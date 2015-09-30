using UnityEngine;
using System.Collections;
using System;
using FutureCode.Game.Command;

public class GameController : MonoBehaviour {
    public bool Started { get; private set; }
    public GameObject Monsters;
    NetManager net;
    GameSettings settings;
	// Use this for initialization
	void Start () {
        net = NetManager.Instance;
        settings = GameSettings.Instance;
	}
	
	// Update is called once per frame
	void Update () {
        if(!Started && net.Connected)
        {
            Started = true;
            StartGame();

            //if(Input.GetMouseButtonDown(0))
            //{
            //    Test();
            //}
            //if(Input.GetMouseButtonDown(1))
            //{

            //}
        }
	}

    private void StartGame()
    {
        settings.Player.SetActive(true);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraController>().Target = settings.Player.transform;
        GameUI.Instance.BloodBar.gameObject.SetActive(true);
        Monsters.SetActive(true);
    }
    private void Test()
    {

    }
    private void Test1()
    {

    }
}
