using Akasha;
using UnityEngine;

public class AndroidMobEntity : BaseEntity
{
    private AndroidMobModel model;
    public MobStateMachine stateMachine;

    private StrategyType Strategy
    {
        get => model.Strategy.Value;
        set => model.Strategy.SetValue(value, model);
    }

    private DanceType Dance
    {
        get => model.Dance.Value;
        set => model.Dance.SetValue(value, model);
    }

    protected override void SetupModels()
    {
        model = new AndroidMobModel();
        model.Setup(this);
        MobManager.Instance.Register(model);
    }

    protected override void SetupParts()
    {
        stateMachine = new MobStateMachine(this, () => transform.position); // ✅ 새 구조: 생성자에서 상태 등록

        BindStrategyToState();

        var group = MobManager.Instance.GroupStatus;
        RxBinder.Bind(group.AvgHealth, _ => EvaluateStrategy(group), this);
        RxBinder.Bind(group.UnderAttack, _ => EvaluateStrategy(group), this);
    }

    private void EvaluateStrategy(MobGroupStatusModel status)
    {
        if (status.AvgHealth.Value < 0.3f)
            Dance = DanceType.HealAll;
        else if (status.UnderAttack.Value)
            Dance = DanceType.DebuffPlayer;
        else
            Dance = DanceType.BuffAll;

        Strategy = StrategyType.Defend;
        RequestDanceState();
    }

    private void RequestDanceState()
    {
        switch (Dance)
        {
            case DanceType.HealAll:
                stateMachine.Request(MobState.Heal);
                break;
            case DanceType.BuffAll:
                stateMachine.Request(MobState.Buff);
                break;
            case DanceType.DebuffPlayer:
                stateMachine.Request(MobState.Debuff);
                break;
        }
    }
    private void BindStrategyToState()
    {
        // 필요시 상태 변화 바인딩 예:
        // stateMachine.ActiveState.Bind(state => { ... }, this, RxType.Functional);
    }
}