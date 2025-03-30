using UnityEngine;

public enum DanceType
{
    Buff,
    Heal,
    Debuff
}

public class AndroidMob : Mankind
{
    private MobGroupStatus lastStatus;
    private DanceType selectedDance = DanceType.Buff;

    public override FormationRole Role => FormationRole.Supporter;

    private void Update()
    {
        // 근접 시 후퇴 로직 (예시)
        float dist = Vector3.Distance(transform.position, Manager.PlayerPosition);
        if (dist < 2f)
        {
            Vector3 dir = (transform.position - Manager.PlayerPosition).normalized;
            transform.position += dir * Time.deltaTime * 1.5f;
            strategyState = StrategyType.Defend;
        }
    }

    public void ReceiveGroupStatus(MobGroupStatus status)
    {
        lastStatus = status;
        EvaluateStrategy();
    }

    public override void EvaluateStrategy()
    {
        selectedDance = DecideDance(lastStatus);
        PerformDance();
    }

    private DanceType DecideDance(MobGroupStatus status)
    {
        if (status.AvgHealth < 0.4f) return DanceType.Heal;
        if (status.UnderAttack) return DanceType.Debuff;
        return DanceType.Buff;
    }

    private void PerformDance()
    {
        Debug.Log($"[AndroidMob] 전략 댄스 실행: {selectedDance}");
        // 애니메이션, 효과, 버프 처리 등 여기에
    }

    public override void TickUpdate() { }
}