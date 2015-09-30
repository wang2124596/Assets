using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class BirthPoint : MonoBehaviour {

    public static LinkedList<BirthPoint> UnUsedPoint = new LinkedList<BirthPoint>();
    public static LinkedList<BirthPoint> UsedPoint = new LinkedList<BirthPoint>();
    public static Vector3? GetEmptyPoint()
    {
        if (UnUsedPoint.Count == 0)
            return null;
        return UnUsedPoint.First.Value.transform.position;
    }
	// Use this for initialization
	void Start () {
        UnUsedPoint.AddLast(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        UnUsedPoint.Remove(this);
        UsedPoint.AddLast(this);
    }
    void OnTriggerExit(Collider other)
    {
        UsedPoint.Remove(this);
        UnUsedPoint.AddLast(this);
    }
}
