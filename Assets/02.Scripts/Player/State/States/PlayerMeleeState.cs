using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeState : PlayerBaseState
{
    public PlayerMeleeState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);
    }
}
