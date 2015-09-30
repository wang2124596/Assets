using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FutureCode.Game.Command;
using System;

[RequireComponent(typeof(CharacterController))]
public class SoliderBehaviour : MonoBehaviour {

    public int Life = 100;
    public bool  IsLive { get { return Life > 0; } }
    //public GameObject BulletPrefab;
    public CharacterController controller;
    Camera mainCamera;
    Vector3 lookPosition
    {
        get
        {
            var pos = transform.position;
            pos.y += 2;
            return pos;
        }
    }
    SoliderAnimationBehaviour anim;
    GameObject bulletPrefab;
    GameSettings settings;

    public bool Shotting = false;

    bool running = false;
    //float cameraHeight;
    //float cameraDistance;
    // Use this for initialization
    void Start () {
        settings = GameSettings.Instance;
        bulletPrefab = transform.FindChild("Bullet").gameObject;
        var animator = GetComponent<Animator>();
        anim = animator.GetBehaviour<SoliderAnimationBehaviour>();
        anim.solider_behaviour = this;
        mainCamera = Camera.main;
        mainCamera.transform.LookAt(lookPosition);
        controller = GetComponent<CharacterController>();

        shotLayerMask = ~((1 << LayerMask.NameToLayer("UnShot")) | 1 << LayerMask.NameToLayer("BirthPoint"));
	}
    KeyCode[] WASD = new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.S, KeyCode.A };
    KeyCode currentKey = KeyCode.None;
    Vector3 previousTargetDirection;
	// Update is called once per frame
	void Update () {
        if(settings.IsPlayerClient && Life > 0)
        {
            if (!Shotting && Input.GetMouseButtonDown(0))
            {
                Shotting = true;
                previousTargetDirection = Vector3.zero;

                ShotAim();

                var cmd = GetCurrentStateCmd(PlayerChangedStateType.Shot);
                NetManager.Instance.Send(cmd);

                Shot();
            }

            if(!Shotting)
            {
                Vector3 targetDirection = new Vector3();
                targetDirection.x = InputManager.instance.Horizontal;
                targetDirection.z = InputManager.instance.Vertical;
                if (targetDirection.x > 0)
                    targetDirection.x = 1;
                else if (targetDirection.x < 0)
                    targetDirection.x = -1;
                if (targetDirection.z > 0)
                    targetDirection.z = 1;
                else if (targetDirection.z < 0)
                    targetDirection.z = -1;

                if (targetDirection == Vector3.zero)
                {
                    if(running)
                        StopRun();
                }
                else
                {
                    if (targetDirection != previousTargetDirection)
                    {
                        transform.forward = targetDirection.normalized;
                        Run();
                    }
                }

                previousTargetDirection = targetDirection;
            }
        }
    }


    public void Turn(float angle)
    {
        var cameraTransform = mainCamera.transform;
        var forward = cameraTransform.forward;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

        transform.forward = forward;

        transform.Rotate(Vector3.up, angle);
    }
    public void Run()
    {
        anim.start_run = true;
        anim.end_run = false;
        running = true;
        if(settings.IsPlayerClient)
        {
            PlayerStateChangedCommand cmd = GetCurrentStateCmd(PlayerChangedStateType.StartMove);
            NetManager.Instance.Send(cmd);
        }
    }
    public void StopRun()
    {
        anim.start_run = false;
        anim.end_run = true;
        running = false;
        if (settings.IsPlayerClient)
        {
            PlayerStateChangedCommand cmd = GetCurrentStateCmd(PlayerChangedStateType.EndMove);
            NetManager.Instance.Send(cmd);
        }
    }

    public void RunSync(Vector3 position, Quaternion rotation, DateTime time)
    {
        if(settings.UnoptimizeMode)
        {
            transform.position = position;
            transform.rotation = rotation;
            Run();
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(position - transform.position);
            NetManager.Instance.SyncSmoothDrag(gameObject, position, NetManager.Instance.Ping * 0.001f * 0.5f,
                () =>
                {
                    transform.rotation = rotation;
                    Run();
                });
        }
    }
    public void StopRunSync(Vector3 position, Quaternion rotation, DateTime time)
    {
        if(settings.UnoptimizeMode)
        {
            transform.position = position;
            transform.rotation = rotation;
            StopRun();
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(position - transform.position);
            NetManager.Instance.SyncSmoothDrag(gameObject, position, NetManager.Instance.Ping * 0.001f * 0.5f,
                () =>
                {
                    transform.rotation = rotation;
                    StopRun();
                });
        }
    }

    public PlayerStateChangedCommand GetCurrentStateCmd(PlayerChangedStateType type)
    {
        PlayerStateChangedCommand cmd = new PlayerStateChangedCommand();
        cmd.PosX = transform.position.x;
        cmd.PosY = transform.position.y;
        cmd.PosZ = transform.position.z;
        cmd.RotationW = transform.rotation.w;
        cmd.RotationX = transform.rotation.x;
        cmd.RotationY = transform.rotation.y;
        cmd.RotationZ = transform.rotation.z;
        cmd.Time = NetManager.Instance.Clock.Now;
        cmd.ChangedStateType = type;
        return cmd;
    }
    public void PlayerStateChanged(PlayerStateChangedCommand cmd)
    {
        Vector3 position = new Vector3(cmd.PosX, cmd.PosY, cmd.PosZ);
        Quaternion rotation = new Quaternion(cmd.RotationX, cmd.RotationY,
            cmd.RotationZ, cmd.RotationW);

        PlayerChangedStateType type = cmd.ChangedStateType;
        DateTime cmdTime = cmd.Time;
        switch(type)
        {
            case PlayerChangedStateType.StartMove:
                RunSync(position, rotation, cmdTime);
                break;
            case PlayerChangedStateType.EndMove:
                StopRunSync(position, rotation, cmdTime);
                break;
            case PlayerChangedStateType.Shot:
                ShotAsync(position, rotation, cmdTime);
                break;
            case PlayerChangedStateType.Injured:
                this.Life -= 10;
                break;
            case PlayerChangedStateType.Dead:
                Life = 0;
                anim.death = true;
                break;
            case PlayerChangedStateType.Reborn:
                Life = 100;
                anim.reset = true;
                transform.position = position;
                transform.rotation = rotation;
                Debug.Log(Life);
                break;
        }
    }

    int shotLayerMask;
    void ShotAim()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, shotLayerMask, QueryTriggerInteraction.Ignore))
        {
            var pos = hit.point;
            var direction = pos - transform.position;
            transform.forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        }
    }
    void Shot()
    {
        anim.shot = true;

        var bullet = (GameObject)GameObject.Instantiate(bulletPrefab, bulletPrefab.transform.position, bulletPrefab.transform.rotation);
        bullet.GetComponent<CapsuleCollider>().enabled = true;
        bullet.GetComponent<MeshRenderer>().enabled = true;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity =  transform.forward * 10;
    }
    void ShotAsync(Vector3 position, Quaternion rotation, DateTime time)
    {
        if (settings.UnoptimizeMode)
        {
            Shotting = true;
            previousTargetDirection = Vector3.zero;
            transform.position = position;
            transform.rotation = rotation;
            Shot();
        }
        else
        {

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!settings.IsPlayerClient) return;

        if (Life <= 0)
            return;
        string triggerName = other.transform.name;
        if (triggerName != "Sword") return;

        Life -= 10;

        if (Life <= 0)
        {
            Death(other.transform);
        }
        else if (GameSettings.Instance.IsPlayerClient)
        {
            var cmd = GetCurrentStateCmd(PlayerChangedStateType.Injured);
            NetManager.Instance.Send(cmd);
        }
    }
    void Death(Transform attacker)
    {
        anim.death = true;
        attacker = attacker.parent;

        var direction = attacker.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        transform.forward = direction.normalized;

        if (GameSettings.Instance.IsPlayerClient)
        {
            var cmd = GetCurrentStateCmd(PlayerChangedStateType.Dead);
            NetManager.Instance.Send(cmd);
        }

        StartCoroutine(Reborn());
    }

    IEnumerator Reborn()
    {
        yield return new WaitForSeconds(3);
        while (BirthPoint.UnUsedPoint.Count == 0)
            yield return new WaitForSeconds(1);
        var pos = BirthPoint.GetEmptyPoint();
        anim.reset = true;
        transform.position = pos.Value;
        Life = 100;
        transform.LookAt(new Vector3(30, 0, 30));

        var cmd = GetCurrentStateCmd(PlayerChangedStateType.Reborn);
        NetManager.Instance.Send(cmd);
    }

}
