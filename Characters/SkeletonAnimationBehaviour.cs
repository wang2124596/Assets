using UnityEngine;
using System.Collections;

public class SkeletonAnimationBehaviour : StateMachineBehaviour {

    public SkeletonBehaviour skeleton_behaviour;
    
    public float base_speed = 1;
    public bool death;
    public bool attacking;
    public bool damage;
    public bool attack;
    public bool start_locomotion;
    public bool end_locomotion;
    public bool warn;
    public float speed;


    CharacterController controller { get { return skeleton_behaviour.Controller; } }

    int f_speed = Animator.StringToHash("speed");
    int t_damage = Animator.StringToHash("damage");
    int t_death = Animator.StringToHash("death");
    int t_attack = Animator.StringToHash("attack");
    int b_locomotion = Animator.StringToHash("locomotion");
    int b_warn = Animator.StringToHash("warn");

    BoxCollider attackTrigger;
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.shortNameHash == s_attack)
        {
            attacking = true;
            //if(attackTrigger == null)
            //{
            //    var sword = skeleton_behaviour.transform.FindChild("Sword");
            //    attackTrigger = sword.GetComponent<BoxCollider>();
            //}
            //skeleton_behaviour.Invoke()
            //attackTrigger.enabled = true;
            skeleton_behaviour.Attack();
        }
    }

    int s_death = Animator.StringToHash("Death");
    int s_locomotion = Animator.StringToHash("Locomotion");
    //int s_damage = Animator.StringToHash("Damage");
    int s_attack = Animator.StringToHash("Attack");
    int s_warn = Animator.StringToHash("Warn");
    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.shortNameHash == s_death)
            return;

        if(warn)
            animator.SetBool(b_warn, true);
        else if(stateInfo.shortNameHash == s_warn)
            animator.SetBool(b_warn, false);

        if (start_locomotion)
        {
            animator.SetBool(b_locomotion, true);
        }
        if (end_locomotion)
        {
            animator.SetBool(b_locomotion, false);
        }
        start_locomotion = end_locomotion = false;
        animator.SetFloat(f_speed, speed);

        if(attack)
        {
            animator.SetBool(b_locomotion, false);
            animator.SetTrigger(t_attack);
            attack = !attack;
        }
        if (damage && !death)
        {
            damage = false;
            animator.SetTrigger(t_damage);
        }
        if (death)
        {
            animator.SetTrigger(t_death);
            return;
        }

        var transform = skeleton_behaviour.transform;
        if (stateInfo.shortNameHash == s_locomotion)
        {
            controller.SimpleMove(transform.forward.normalized * (speed * 2 + 1));
        }
            //transform.Translate(transform.forward * (speed * 2 + 1) * Time.deltaTime);
    }


    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == s_attack)
        {
            attacking = false;
        }
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //}

    // OnStateMachineExit is called when exiting a statemachine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //}
}
