using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    public PlayerAirState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.Player.FeetParticle.gameObject.SetActive(false);
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.AirParameterHash);
    }

    public override void Exit()
    {
        stateMachine.Player.FeetParticle.gameObject.SetActive(true);
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.AirParameterHash);
    }
}
