using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        //stateMachine.PlayerController.speed = 0f; // 필요 없어보임
        base.Enter();
        StartAnimation(stateMachine.Player.animationData.IdleParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.animationData.IdleParameterHash);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.PlayerController.ReturnMoveInput() != Vector2.zero)
        {
            if (stateMachine.PlayerController.isSprint)
                stateMachine.ChangeState(stateMachine.RunState);
            else
                stateMachine.ChangeState(stateMachine.WalkState);
        }
    }
}
