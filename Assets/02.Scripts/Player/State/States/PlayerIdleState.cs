using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.PlayerController.speed = 0f; // 필요 없어보임
        base.Enter();
        StartAnimation(stateMachine.Player.animationData.idleParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.animationData.idleParameterHash);
    }

    public override void Update()
    {
        base.Update();
    }
}
