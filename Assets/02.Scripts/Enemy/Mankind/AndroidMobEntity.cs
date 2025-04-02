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
                stateMachine.Request(MobState.Buff); // ✅ 이름 변경
                break;
            case DanceType.Debuff:
                stateMachine.Request(MobState.Debuff); // ✅ 이름 변경
                break;
        }
    }

    private void BindStrategyToState()
    {
        // 필요시 상태 변화 바인딩 예:
        // stateMachine.ActiveState.Bind(state => { ... }, this, RxType.Functional);
    }
}