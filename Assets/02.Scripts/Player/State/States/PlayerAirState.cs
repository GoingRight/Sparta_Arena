using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    public PlayerAirState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.airParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.airParameterHash);
    }
}
