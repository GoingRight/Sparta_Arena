using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBaseState : IState
{
    protected PlayerStateMachine stateMachine;
    protected readonly PlayerGroundData groundData;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        groundData = stateMachine.Player.Data.GroundData;
    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {

    }

    public virtual void HandleInput()
    {
        stateMachine.PlayerController.ReturnMoveInput();
    }

    public virtual void PhysicsUpdate()
    {

    }

    public virtual void Update()
    {

    }

    protected void StartAnimation(int animatorHash)
    {
        stateMachine.Player.Animator.SetBool(animatorHash, true);
    }

    protected void StopAnimation(int animatorHash)
    {
        stateMachine.Player.Animator.SetBool(animatorHash, false);
    }

    protected void ForceMove()
    {
        stateMachine.PlayerController._rigidbody.velocity = stateMachine.Player.transform.forward * stateMachine.PlayerController.speed;
    }

    protected float GetNormalizeTime(Animator anim, string tag)
    {
        AnimatorStateInfo currentInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = anim.GetNextAnimatorStateInfo(0);

        if (anim.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }

        else if (!anim.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }

        else
        {
            return 0;
        }
    }
}
