using Akasha;
using UnityEngine;

public class AndroidMobEntity : BaseEntity
{
    private AndroidMobModel model;
    public MobStateMachine stateMachine;

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
        stateMachine = new MobStateMachine();
        stateMachine.Setup(this, () => transform.position);

        BindStrategyToState();

        var group = MobManager.Instance.GroupStatus;
        RxBinder.Bind(group.AvgHealth, _ => EvaluateStrategy(group), this);
        RxBinder.Bind(group.UnderAttack, _ => EvaluateStrategy(group), this);
    }

    private void EvaluateStrategy(MobGroupStatusModel status)
    {
        if (status.AvgHealth.Value < 0.4f)
            Dance = DanceType.Heal;
        else if (status.UnderAttack.Value)
            Dance = DanceType.Debuff;
        else
            Dance = DanceType.Buff;

        Strategy = StrategyType.Defend;
        RequestDanceState();
    }

    private void RequestDanceState()
    {
        switch (Dance)
        {
            case DanceType.Heal:
            case DanceType.Buff:
                stateMachine.RequestState(MobState.Buff);
                break;
            case DanceType.Debuff:
                stateMachine.RequestState(MobState.Debuff);
                break;
        }
    }

    private void BindStrategyToState()
    {
        // 선택적으로 애니메이션이나 추가 로직 바인딩 가능
    }
}