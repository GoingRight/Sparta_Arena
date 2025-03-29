using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerGroundState
{
    public PlayerWalkState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.animationData.WalkParameterHash);
        Debug.Log("Walk");
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.animationData.WalkParameterHash);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.PlayerController.ReturnMoveInput() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        } else if (stateMachine.PlayerController.isSprint)
        {
            stateMachine.ChangeState(stateMachine.RunState);
        }
    }

}
