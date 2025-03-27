using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseState : IState
{
    protected PlayerStateMachine stateMachine;
    protected readonly PlayerGroundData groundData;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        groundData = stateMachine.player.Data.GroundData;
    }

    public void Enter()
    {

    }

    public void Exit()
    {

    }

    public void HandleInput()
    {

    }

    public void PhysicsUpdate()
    {

    }

    public void Update()
    {

    }

    protected void StartAnimation(int animatorHash)
    {
        stateMachine.player.animator.SetBool(animatorHash, true);
    }

    protected void StopAnimation(int animatorHash)
    {
        stateMachine.player.animator.SetBool(animatorHash, false);
    }
}
