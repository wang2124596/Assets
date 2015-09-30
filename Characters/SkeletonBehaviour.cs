using UnityEngine;
using System.Collections;
using DateTime = System.DateTime;
using FutureCode.Game.Command;

[RequireComponent(typeof(CharacterController))]
public class SkeletonBehaviour : MonoBehaviour {

    public GameObject Player;
    public CharacterController Controller;
    public int ID;
    public int Life = 100;
    public float MaxWanderDistance = 5;
    public float SightDistance = 15;
    public float StrategyMakeInterval = 3;
    SkeletonAnimationBehaviour anim;
    public bool Death = false;
    public bool Warn { set { anim.warn = value; } }
    bool breakAttack = false;
    SoliderBehaviour playerBehaviour;

    Strategy currentStrategy = Strategy.Idle;
    int uncastLayerMask;

	void Awake () {
        uncastLayerMask = ~(1 << LayerMask.NameToLayer("BirthPoint"));
        var animator = GetComponent<Animator>();
        anim = animator.GetBehaviour<SkeletonAnimationBehaviour>();
        anim.skeleton_behaviour = this;
        Controller = GetComponent<CharacterController>();
        attackTrigger = transform.FindChild("Sword").GetComponent<BoxCollider>();

        if (Player == null)
            Player = GameObject.FindWithTag("Player");
        playerBehaviour = Player.GetComponent<SoliderBehaviour>();
	}

	// Update is called once per frame
	void Update () {
        if (!GameSettings.Instance.IsPlayerClient)
            return;

        if (Death)
        {
            return;
        }

        if(!playerBehaviour.IsLive)
        {
            Idle();
            return;
        }

        if (Time.frameCount % 30 == 0)
        {
            if (FindPlayer())
            {
                anim.warn = true;

                if (currentStrategy != Strategy.Pursuit)
                {
                    if (CloseTo(Player.transform.position))
                    {
                        var targetForward = Player.transform.position - transform.position;
                        transform.forward = targetForward;
                        anim.attack = true;
                    }
                    else
                    {
                        StartCoroutine(PursuitPlayer());
                    }
                }
            }
            else if (currentStrategy == Strategy.Wander)
            {
            }
            else
            {
                Idle();
            }

            if(currentStrategy == Strategy.Idle)
            {
                StartCoroutine(Wander());
            }

        }
    }
    public void Idle()
    {
        if (currentStrategy == Strategy.Idle)
            return;

        if (GameSettings.Instance.IsPlayerClient)
        {
            var cmd = GetCurrentStateCmd(MonsterChangedStateType.Idle);
            NetManager.Instance.Send(cmd);
        }

        anim.speed = 0;
        anim.warn = false;
        anim.attack = false;
        anim.end_locomotion = true;
        currentStrategy = Strategy.Idle;
    }
    public void IdleSync(Vector3 position, Quaternion rotation, DateTime time)
    {
        if(GameSettings.Instance.UnoptimizeMode)
        {
            transform.position = position;
            transform.rotation = rotation;
            Idle();
        }
        else
        {

        }

    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            anim.damage = true;
            Life -= 20;
            if (Life <= 0)
            {
                if (GameSettings.Instance.IsPlayerClient)
                {
                    var cmd = GetCurrentStateCmd(MonsterChangedStateType.Dead);
                    NetManager.Instance.Send(cmd);
                }

                anim.death = true;
                Death = true;
                MonsterManager.Instance.MonsterDestroied(ID);
                var colliders = transform.GetComponents<Collider>();
                foreach (var c in colliders)
                    c.enabled = false;
            }
            else
            {
                if (GameSettings.Instance.IsPlayerClient)
                {
                    var cmd = GetCurrentStateCmd(MonsterChangedStateType.Injured);
                    NetManager.Instance.Send(cmd);
                }

                transform.LookAt(Player.transform);
            }
            breakAttack = true;
        }
    }


    bool FindPlayer()
    {
        var forward = transform.forward;
        var playerPosition = Player.transform.position;
        playerPosition.y += 1;
        var directionToPlayer = playerPosition - transform.position;
        float angle = Vector3.Angle(forward, directionToPlayer);
        if(angle < 90)
        {
            var distance = Vector3.Distance(transform.position, Player.transform.position);
            if (distance > SightDistance)
                return false;
            var start = transform.position;
            start.y += 1;
            Ray ray = new Ray(start, directionToPlayer);
            Debug.DrawRay(transform.position, directionToPlayer);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, SightDistance, uncastLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.tag == "Player")
                    return true;
            }
        }
        return false;
    }

    IEnumerator Wander()
    {
        currentStrategy = Strategy.Wander;
        Ray[] rays = new Ray[4];
        rays[0] = new Ray(transform.position, transform.forward);
        rays[1] = new Ray(transform.position, -transform.forward);
        rays[2] = new Ray(transform.position, -transform.right);
        rays[3] = new Ray(transform.position, transform.right);
        RaycastHit hit;
        int randomIndex = Random.Range(0, rays.Length - 1);
        Physics.Raycast(rays[randomIndex], out hit, 100f);
        var pos = hit.point;
        float distance = Vector3.Distance(pos, transform.position) -.5f;
        if (distance > MaxWanderDistance)
        {
            distance = MaxWanderDistance;
        }
        transform.forward = rays[randomIndex].direction.normalized;

        var cmd = GetCurrentStateCmd(MonsterChangedStateType.StartWander);
        NetManager.Instance.Send(cmd);

        if (distance > 0.5f)
            distance = Random.Range(0.5f, distance);
        var targetPos = transform.position + transform.forward * distance;
        AnimStartLocomotion();
        while (!CloseTo(targetPos, 1f))
        {
            if (currentStrategy != Strategy.Wander)
                break;
            yield return new WaitForFixedUpdate();
        }

        cmd = GetCurrentStateCmd(MonsterChangedStateType.EndWander);
        NetManager.Instance.Send(cmd);

        if (currentStrategy == Strategy.Wander)
        {
            AnimEndLocomotion();
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            if (currentStrategy == Strategy.Wander)
            {
                Idle();
            }
        }
    }
    IEnumerator PursuitPlayer()
    {
        currentStrategy = Strategy.Pursuit;
        var targetPosition = Player.transform.position;
        transform.LookAt(targetPosition);

        var cmd = GetCurrentStateCmd(MonsterChangedStateType.StartPursuit);
        NetManager.Instance.Send(cmd);

        AnimStartLocomotion();
        anim.speed = 1;
        while (!CloseTo(targetPosition) && !CloseTo(Player.transform.position))
        {
            yield return new WaitForFixedUpdate();
        }

        cmd = GetCurrentStateCmd(MonsterChangedStateType.Warn);
        NetManager.Instance.Send(cmd);

        AnimEndLocomotion();
        currentStrategy = Strategy.Warn;
        anim.speed = 0;
    }

    IEnumerator AimPlayer()
    {
        currentStrategy = Strategy.Aim;
        var playerPosition = Player.transform.position;
        var direction = playerPosition - transform.position;
        var current = transform.forward;
        while (direction != transform.forward)
        {
            current = Vector3.Lerp(direction, current, 0.1f);
            transform.forward = current;
            yield return new WaitForFixedUpdate();
        }
        currentStrategy = Strategy.Warn;
    }

    bool CloseTo(Vector3 target, float distanceIsClose = 3)
    {
        distanceIsClose += 0.05f;
        return Vector3.Distance(transform.position, target) <= distanceIsClose;
    }

    BoxCollider attackTrigger;
    IEnumerator attack()
    {
        breakAttack = false;
        yield return new WaitForSeconds(1);
        if (!breakAttack)
        {
            attackTrigger.enabled = true;
            yield return new WaitForSeconds(0.5f);
            attackTrigger.enabled = false;
        }
    }

    public void Attack()
    {
        if (GameSettings.Instance.IsPlayerClient)
        {
            var cmd = GetCurrentStateCmd(MonsterChangedStateType.Attack);
            NetManager.Instance.Send(cmd);
        }

        StartCoroutine(attack());
    }
    public void Dead()
    {
        Life = 0;
        Death = true;
        anim.death = true;
    }
    public void StartLocomotionSync(Vector3 startPosition, Quaternion rotation, DateTime time, int speed)
    {
        if(GameSettings.Instance.UnoptimizeMode)
        {
            transform.position = startPosition;
            transform.rotation = rotation;
            anim.speed = speed;
            AnimStartLocomotion();
        }
        else
        {

        }
    }
    public void EndLocomotionSync(Vector3 endPosition, Quaternion rotation, DateTime time)
    {
        if (GameSettings.Instance.UnoptimizeMode)
        {
            transform.position = endPosition;
            transform.rotation = rotation;
            anim.speed = 0;
            AnimEndLocomotion();
        }
        else
        {

        }
    }

    public void AttackSync(Vector3 attackPosition, Quaternion rotation, DateTime time)
    {
        if (GameSettings.Instance.UnoptimizeMode)
        {
            AnimEndLocomotion();
            transform.position = attackPosition;
            transform.rotation = rotation;
            anim.attack = true;
        }
        else
        {

        }
    }
    //public void StartPursuitSync(Vector3 startPosition, Quaternion rotation, DateTime time)
    //{
    //    if (GameSettings.Instance.UnoptimizeMode)
    //    {
    //        transform.position = startPosition;
    //        transform.rotation = rotation;
    //        anim.speed = 1;
    //        AnimStartLocomotion();
    //    }
    //    else
    //    {

    //    }
    //}
    //public void EndPursuitSync(Vector3 endPosition, Quaternion rotation, DateTime time)
    //{
    //    if (GameSettings.Instance.UnoptimizeMode)
    //    {
    //        transform.position = endPosition;
    //        transform.rotation = rotation;
    //        anim.speed = 1;
    //        AnimEndLocomotion();
    //    }
    //    else
    //    {

    //    }
    //}

    public MonsterStateChangedCommand GetCurrentStateCmd(MonsterChangedStateType type)
    {
        MonsterStateChangedCommand cmd = new MonsterStateChangedCommand();
        cmd.MonsterID = this.ID;
        cmd.Life = this.Life;
        cmd.PosX = transform.position.x;
        cmd.PosY = transform.position.y;
        cmd.PosZ = transform.position.z;
        cmd.RotationX = transform.rotation.x;
        cmd.RotationY = transform.rotation.y;
        cmd.RotationZ = transform.rotation.z;
        cmd.RotationW = transform.rotation.w;
        cmd.Time = NetManager.Instance.Clock.Now;
        cmd.ChangedStateType = type;
        return cmd;
    }

    void AnimStartLocomotion()
    {
        anim.start_locomotion = true;
        anim.end_locomotion = false;
    }
    void AnimEndLocomotion()
    {
        anim.start_locomotion = false;
        anim.end_locomotion = true;
    }


    enum Strategy
    {
        Idle,
        Wander,
        Warn,
        Aim,
        Pursuit,
        Attack,

    }
}

