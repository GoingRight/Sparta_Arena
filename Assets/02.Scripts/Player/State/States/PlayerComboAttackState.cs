using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboAttackState : PlayerMeleeState
{
    public PlayerComboAttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.PlayerController.speed = 0;
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);
    }

    public override void Exit()
    {
        stateMachine.PlayerController.speed = stateMachine.Player.stat.Speed;
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);
    }
}
