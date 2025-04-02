using System.Collections.Generic;
using Akasha;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AndroidMobInteractor : BaseInteractor<AndroidMobEntity>
{
    [SerializeField] private Animator animator;
    public Text stateTxt;
    private MobStateMachine State => Entity?.stateMachine;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) PlayTestAnim(MobState.Heal);
        if (Input.GetKeyDown(KeyCode.B)) PlayTestAnim(MobState.Buff);
        if (Input.GetKeyDown(KeyCode.D)) PlayTestAnim(MobState.Debuff);
    }

    private void PlayTestAnim(MobState state)
    {
        animator.Play(state.ToString()); // 단, 상태 이름과 클립 이름이 일치해야 함
        Debug.Log($"[Test] 재생 요청: {state}");
    }
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
            case MobState.Retreat:
                animator.Play("Retreat"); break;
            case MobState.Buff:
                animator.Play("Buff"); break;
            case MobState.Debuff:
                animator.Play("Debuff"); break;
            case MobState.Hit:
                animator.Play("Hit"); break;
            case MobState.Dead:
                animator.Play("Dead"); break;
            case MobState.Act1:
                animator.Play("Skill1"); break;
            case MobState.Act2:
                animator.Play("Skill2"); break;
            case MobState.Act3:
                animator.Play("Skill3"); break;
            case MobState.Heal:
                animator.Play("Heal"); break;
        }
        if (stateTxt != null)
            stateTxt.text = $"STATE: {state}";
    }
}