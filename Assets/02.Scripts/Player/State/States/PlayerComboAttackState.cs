using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboAttackState : PlayerMeleeState
{
    private bool alreadyAppliedCombo;
    private bool alreadyApplyForce;

    AttackInfoData attackInfoData;

    public PlayerComboAttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Combo Attack State");
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);

        alreadyAppliedCombo = false;
        alreadyApplyForce = false;

        int comboIndex = stateMachine.ComboIndex;
        attackInfoData = stateMachine.Player.Data.AttackData.AttackInfoData(comboIndex);
        stateMachine.Player.Animator.SetInteger("Combo", comboIndex);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.MeleeAttackParameterHash);

        if (!alreadyAppliedCombo)
        {
            stateMachine.ComboIndex = 0;
        }
    }

    public override void Update()
    {
        base.Update();

        float normalizedTime = GetNormalizeTime(stateMachine.Player.Animator, "Attack");
        if(normalizedTime < 1f)
        {
            if (normalizedTime >= attackInfoData.ComboTransitionTime)
            {
                TryComboAttack();
            }

            if (normalizedTime >= attackInfoData.ForceTranstionTime)
            {
                // 댐핑 처리
            }
        }
        else
        {
            if(alreadyAppliedCombo)
            {
                stateMachine.ComboIndex = attackInfoData.ComboStateIndex;
                stateMachine.ChangeState(stateMachine.ComboAttackState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }

    void TryComboAttack()
    {
        if (alreadyAppliedCombo) return;

        if (attackInfoData.ComboStateIndex == -1) return;

        if (!stateMachine.isAttacking) return;

        alreadyAppliedCombo = true;
    }
}
