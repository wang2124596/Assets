using UnityEngine;
using System.Collections;

public class SoliderAnimationBehaviour : StateMachineBehaviour {

    public SoliderBehaviour solider_behaviour;
    public bool start_run = false;
    public bool end_run = false;
    public bool shot = false;
    public bool death = false;
    public float speed = 1;
    public bool reset = false;

    bool is_run = false;

    //parameterType_parameterName
    int b_run = Animator.StringToHash("isRun");
    int t_reset = Animator.StringToHash("reset");
    int t_death = Animator.StringToHash("death");
    int t_shot = Animator.StringToHash("shot");

    //state_animationName
    int s_run = Animator.StringToHash("Run");
    int s_shot = Animator.StringToHash("Shot");
    int s_death = Animator.StringToHash("Death");

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(reset)
        {
            reset = false;
            death = false;
            animator.SetTrigger(t_reset);
        }
        if (stateInfo.shortNameHash == s_death)
            return;

        if(death)
        {
            animator.SetTrigger(t_death);
            return;
        }
        if (start_run)
        {
            is_run = true;
            animator.SetBool(b_run, true);
        }
        else if (end_run)
        {
            is_run = false;
            animator.SetBool(b_run, false);
        }
        start_run = end_run = false;

        if (shot)
        {
            if (stateInfo.shortNameHash == s_run)
                animator.SetBool(b_run, false);
            animator.SetTrigger(t_shot);
            shot = false;
        }
        else
        {
            if(is_run)
                animator.SetBool(b_run, true);
        }

        var transform = solider_behaviour.transform;
        if (stateInfo.shortNameHash == s_run)
        {
            //transform.Translate(transform.forward * -speed * Time.deltaTime);
            solider_behaviour.controller.SimpleMove(transform.forward.normalized * 5);
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash == s_shot)
            solider_behaviour.Shotting = false;
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
    //
    //}

    // OnStateMachineExit is called when exiting a statemachine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
    //
    //}
}
