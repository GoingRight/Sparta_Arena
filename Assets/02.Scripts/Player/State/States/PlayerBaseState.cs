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
        stateMachine.Player.animator.SetBool(animatorHash, true);
    }

    protected void StopAnimation(int animatorHash)
    {
        stateMachine.Player.animator.SetBool(animatorHash, false);
    }

    protected virtual void OnMoveCancled(InputAction.CallbackContext context) { }

    protected virtual void OnRunStarted(InputAction.CallbackContext context) { }
}
