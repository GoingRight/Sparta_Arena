using Akasha;
using UnityEngine;

public class AndroidMobEntity : BaseEntity
{
    private AndroidMobModel model;
    // --- 모델 래핑 프로퍼티 ---
    private StrategyType Strategy
    {
        get => model.Strategy.Value;
        set => model.Strategy.SetValue(value, this);
    }

    private DanceType Dance
    {
        get => model.Dance.Value;
        set => model.Dance.SetValue(value, this);
    }

    protected override void SetupModels()
    {
        model = new AndroidMobModel();
        model.Setup(this);

        MobManager.Instance.Register(model);
    }

    protected override void SetupParts()
    {
        var group = MobManager.Instance.GroupStatus;

        RxBinder.Bind(group.AvgHealth, _ => EvaluateStrategy(group), this);
        RxBinder.Bind(group.UnderAttack, _ => EvaluateStrategy(group), this);
    }

    // --- 전략 판단 로직 ---

    private void EvaluateStrategy(MobGroupStatusModel status)
    {
        if (status.AvgHealth.Value < 0.4f)
            Dance = DanceType.Heal;
        else if (status.UnderAttack.Value)
            Dance = DanceType.Debuff;
        else
            Dance = DanceType.Buff;

        Strategy = StrategyType.Defend;

        PerformDance();
    }

    private void PerformDance()
    {
        Debug.Log($"[AndroidMobEntity] 전략 댄스 실행: {Dance}");
        // TODO: 애니메이션, 이펙트 처리
    }
}