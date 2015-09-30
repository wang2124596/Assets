using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public Transform Target;
    public float Speed = 3;
    public float BaseRotateAngle = 10;
    public Vector3 RelativePosition = new Vector3(0f, 3f, -3f);
    //bool isHit = false;
	// Use this for initialization
	void Start () {
        if (Target == null)
            Target = GameObject.FindWithTag("Player").transform;
        //key_state = InputManager.instance.KeyStateHold;
	}
	
	// Update is called once per frame
	void Update () {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        Vector3 targetPos = Target.position + RelativePosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, Speed * Time.deltaTime);
        //transform.position = targetPos;
        var lookPos = Target.position;
        lookPos.y += 2;
        var direction = lookPos - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
    }

    void MouseLookAt()
    {
        Quaternion rotation = new Quaternion();
        var sinAngle = Mathf.Sin(BaseRotateAngle * InputManager.instance.MouseX / 2);
        rotation.x = Target.up.x * sinAngle;
        rotation.y = Target.up.y * sinAngle;
        rotation.z = Target.up.z * sinAngle;
        rotation.w = Mathf.Cos(BaseRotateAngle * InputManager.instance.MouseX / 2);
        RelativePosition = rotation * RelativePosition;
    }
}
