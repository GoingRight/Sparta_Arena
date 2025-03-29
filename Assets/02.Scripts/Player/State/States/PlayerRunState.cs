using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerGroundState
{
    public PlayerRunState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.animationData.RunParameterHash);
        Debug.Log("Run");
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.animationData.RunParameterHash);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.PlayerController.ReturnMoveInput() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
        if (!stateMachine.PlayerController.isSprint)
        {
            stateMachine.ChangeState(stateMachine.WalkState);
            return;
        }
    }
}
