using Akasha;
using UnityEngine;

public class PrettyGirlEntity : BaseEntity
{
    private PrettyGirlModel model;

    protected override void SetupModels()
    {
        model = new PrettyGirlModel();
        model.Setup(this);

        MobManager.Instance.Register(model);
    }

    // --- 모델 래핑 프로퍼티 ---
    private StrategyType Strategy
    {
        get => model.Strategy.Value;
        set => model.Strategy.SetValue(value, this);
    }

    protected override void SetupParts()
    {
        RxTimer.Every(0.2f, this, CheckProximityAndDecide);
    }



    private float DistanceToPlayer
        => Vector3.Distance(transform.position, MobManager.Instance.PlayerPosition);

    // --- 전략 판단 로직 ---

    private void CheckProximityAndDecide()
    {
        if (DistanceToPlayer < 3f && Strategy == StrategyType.Idle)
        {
            Strategy = Random.value > 0.5f ? StrategyType.Attack : StrategyType.Defend;
        }
        else if (DistanceToPlayer >= 3f && Strategy == StrategyType.Attack)
        {
            Strategy = StrategyType.Idle;
        }
    }
}