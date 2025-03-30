using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Akasha;


public class PrettyGirl : Mankind
{
    [SerializeField] private Animator animator;

    private float checkInterval = 0.2f;
    private float checkTimer = 0f;

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckProximityAndDecide();
        }

        // 이동: 추후 FormationController에서 targetPosition 제공받아 이동 가능
        transform.position = Vector3.Lerp(transform.position, transform.position, Time.deltaTime * 5f);
    }

    private void CheckProximityAndDecide()
    {
        float dist = Vector3.Distance(transform.position, Manager.PlayerPosition);

        if (dist < 3f && strategyState == StrategyType.Idle)
        {
            strategyState = DecideAttackOrDefend();
            TriggerAction();
        }
        else if (dist >= 3f && strategyState == StrategyType.Attack)
        {
            strategyState = StrategyType.Idle;
        }
    }

    private StrategyType DecideAttackOrDefend()
    {
        return Random.Range(0f,1f) > 0.5 ? StrategyType.Attack : StrategyType.Defend;
    }

    private void TriggerAction()
    {
        if (strategyState == StrategyType.Attack)
        {
            PlayAttackAnimation();
        }
        else if (strategyState == StrategyType.Defend)
        {
            PlayDefendAnimation();
        }
    }

    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("[PrettyGirl] 공격 애니메이션 실행");
        }
    }

    private void PlayDefendAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Defend");
            Debug.Log("[PrettyGirl] 방어 애니메이션 실행");
        }
    }

    public override void TickUpdate() { }
    public override void EvaluateStrategy() { }

    public override FormationRole Role => FormationRole.Fighter;
}