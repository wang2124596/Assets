using UnityEngine;
using System.Collections;
using Stopwatch = System.Diagnostics.Stopwatch;
using System;

public class LerpTest : MonoBehaviour {

    public Vector3 TargetPosition;
    public float TimeDuration = 1;
    public float Factor = 2f;
    public float Velocity = 1f;
    Stopwatch watch;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
        if(Input.GetKey(KeyCode.Alpha1))
        {
            float step = 1 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
        }
        if(Input.GetKey(KeyCode.Alpha2))
        {
            Vector3 targetDir = TargetPosition - transform.position;
            float step = 1 * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
        if(Input.GetKey(KeyCode.Alpha3))
        {
            if (transform.position == TargetPosition)
                return;
            transform.position = Vector3.Slerp(transform.position, TargetPosition, Time.deltaTime);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            watch = Stopwatch.StartNew();
            StartCoroutine(Move(gameObject, TargetPosition, TimeDuration,
                () => { Debug.Log(watch.ElapsedMilliseconds);
            }));
        }
	
	}

    public IEnumerator Move(GameObject obj, Vector3 targetPos, float timeDuration, Action callBack = null)
    {
        float startTime = Time.time;
        float endTime = Time.time + timeDuration;
        float usedTime = 0f;
        while (Time.time < endTime)
        {
            float percentage = usedTime / TimeDuration;
            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, percentage);
            yield return new WaitForEndOfFrame();
            usedTime = Time.time - startTime;
        }
        if (callBack != null)
            callBack.Invoke();
    }
}
