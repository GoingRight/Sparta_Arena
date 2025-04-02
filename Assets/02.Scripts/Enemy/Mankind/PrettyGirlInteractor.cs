using System.Collections.Generic;
using Akasha;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrettyGirlInteractor : BaseInteractor<PrettyGirlEntity>
{
    [SerializeField] private Animator animator;
    public Text stateTxt;
    private MobStateMachine State => Entity?.stateMachine;

    private void Start()
    {
        stateTxt = GetComponentInChildren<Text>();
        if (State == null || animator == null) return;

        State.ActiveState.Bind(OnStateChanged, this, RxType.Functional);
    }

    private void OnStateChanged(MobState state)
    {
        switch (state)
        {
            case MobState.Idle:
                animator.Play("Idle"); break;
            case MobState.Walk:
                animator.Play("Walk"); break;
            case MobState.Run:
                animator.Play("Run"); break;
            case MobState.Hit:
                animator.Play("Hit"); break;
            case MobState.Dead:
                animator.Play("Dead"); break;

            // 공격 모션: 무기 기반으로 클립 다르게 매핑 가능
            case MobState.Act1:
                animator.Play("Attack_Sword"); break;
            case MobState.Act2:
                animator.Play("Attack_Spear"); break;
            case MobState.Act3:
                animator.Play("Attack_Rifle"); break;
        }

        if (stateTxt != null)
            stateTxt.text = $"STATE: {state}";
    }
}