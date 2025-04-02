using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTakeDamagedState : PlayerGroundState
{
    public PlayerTakeDamagedState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.TakeDamagedParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.TakeDamagedParameterHash);
    }
}
