using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletController : MonoBehaviour {

    bool destory = false;
	// Use this for initialization
	void Start () { } 
	// Update is called once per frame
	void Update () {
        if (destory)
            GameObject.DestroyImmediate(gameObject);
	
	}
    void OnTriggerEnter(Collider other)
    {
        if (other.name != "point")
        {
            destory = true;
        }
    }
}


