using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

    public static InputManager instance;
    public KeyStateHold KeyStateHold;
    public float Horizontal { get { return Input.GetAxis("Horizontal"); } }
    public float Vertical { get { return Input.GetAxis("Vertical"); } }
    public float MouseX { get { return Input.GetAxis("Mouse X"); } }
    public float MouseY { get { return Input.GetAxis("Mouse Y"); } }
	// Use this for initialization
	void Start () {
        instance = this;
        KeyStateHold = new KeyStateHold();
	}

    // Update is called once per frame
    List<KeyCode> codes = new List<KeyCode>() { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space };
    void Update () {
        foreach (var code in codes)
        {
            if (Input.GetKeyDown(code))
                KeyStateHold[code] = KeyState.Down;
            else if (Input.GetKeyUp(code))
                KeyStateHold[code] = KeyState.Up;
            else if (KeyStateHold[code] == KeyState.Down)
                KeyStateHold[code] = KeyState.Stay;
            else if (KeyStateHold[code] == KeyState.Up)
                KeyStateHold[code] = KeyState.Idle;
        }
    }
}

public class KeyStateHold
{
    Dictionary<KeyCode, KeyState> key_dict;
    public KeyState this[KeyCode code]
    {
        get
        {
            if (key_dict.ContainsKey(code))
                return key_dict[code];
            return KeyState.Idle;
        }
        set
        {
            if (key_dict.ContainsKey(code))
                key_dict[code] = value;
            else
                key_dict.Add(code, value);
        }
    }
    public KeyStateHold()
    {
        key_dict = new Dictionary<KeyCode, KeyState>();
    }

    //List<KeyCode> wasd = new List<KeyCode>() { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    //public bool HasWASDStay(out KeyCode staiedKey)
    //{
    //    bool res = false;
    //    staiedKey = KeyCode.None;
    //    foreach (var key in wasd)
    //    {
    //        if (this[key] == KeyState.Down || this[key] == KeyState.Stay)
    //        {
    //            res = true;
    //            staiedKey = key;
    //            break;
    //        }
    //    }
    //    return res;
    //}

}

public enum KeyState
{
    Down = 0,
    Stay = 1,
    Up = 2,
    Idle = -1
}
