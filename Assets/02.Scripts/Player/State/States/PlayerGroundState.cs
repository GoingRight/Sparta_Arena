using System;
using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.PlayerController.jumpTrigger += OnJumpStarted;
        stateMachine.Player.detectTakeDamage += OnTakeDamaged;
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
    }

    public override void Exit()
    {
        stateMachine.PlayerController.jumpTrigger -= OnJumpStarted;
        stateMachine.Player.detectTakeDamage -= OnTakeDamaged;
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.GroundParameterHash);
    }

    protected void OnJumpStarted()
    {
        stateMachine.ChangeState(stateMachine.JumpState);
    }

    protected void OnTakeDamaged()
    {
        stateMachine.ChangeState(stateMachine.TakeDamagedState);
    }

    public override void Update()
    {
        base.Update();

        if (stateMachine.isAttacking)
            OnAttack();
    }

    void OnAttack()
    {
        stateMachine.ChangeState(stateMachine.ComboAttackState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (!stateMachine.PlayerController.isGrounded && stateMachine.Player.RigidBody.velocity.y < Physics.gravity.y * Time.fixedDeltaTime)
        {
            stateMachine.ChangeState(stateMachine.FallState);
            return;
        }
    }
}
