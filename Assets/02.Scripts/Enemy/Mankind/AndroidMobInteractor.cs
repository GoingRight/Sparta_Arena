using Akasha;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class AndroidMobInteractor : BaseInteractor<AndroidMobEntity>
{
    [SerializeField] private Animator animator;

    private MobStateMachine State => (Entity)?.stateMachine;

    private void OnEnable()
    {
        if (State == null || animator == null) return;

        State.IsIdle.Bind(v => { if (v) animator.Play("Idle"); }, State, RxType.Logical);
        State.IsWalk.Bind(v => { if (v) animator.Play("Walk"); }, State, RxType.Logical);
        State.IsRun.Bind(v => { if (v) animator.Play("Run"); }, State, RxType.Logical);
        State.IsRetreating.Bind(v => { if (v) animator.Play("Retreat"); }, State, RxType.Logical);
        State.IsBuffing.Bind(v => { if (v) animator.Play("Buff"); }, State, RxType.Logical);
        State.IsDebuffing.Bind(v => { if (v) animator.Play("Debuff"); }, State, RxType.Logical);
        State.IsHit.Bind(v => { if (v) animator.Play("Hit"); }, State, RxType.Logical);
        State.IsDead.Bind(v => { if (v) animator.Play("Dead"); }, State, RxType.Logical);

        State.IsActing.Bind(v => {
            if (!v) return;
            if (State.IsActive(MobState.Act1)) animator.Play("Skill1");
            else if (State.IsActive(MobState.Act2)) animator.Play("Skill2");
            else if (State.IsActive(MobState.Act3)) animator.Play("Skill3");
        }, State, RxType.Logical);
    }
}
