using Akasha;
using UnityEngine;

public class PrettyGirlModel : RxModel, IMobHealthReadable
{
    // ✅ 저장되는 값
    public RxVar<float> CurrentHealth { get; private set; }
    public RxVar<float> MaxHealth { get; private set; }
    public RxVar<StrategyType> Strategy { get; private set; }

    // ✅ 외부 연결 가능한 계산 필드
    public RxExpr<float> HealthRatioExpr { get; private set; }

    // ✅ 조건 판단용 플래그
    public RxFlag IsAlive { get; private set; }

    public float HealthRatio => HealthRatioExpr.Value;

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        MaxHealth = new RxVar<float>(100f, this);
        CurrentHealth = new RxVar<float>(100f, this);
        Strategy = new RxVar<StrategyType>(StrategyType.Idle, this);

        HealthRatioExpr = new RxExpr<float>(
            () => Mathf.Clamp01(CurrentHealth.Value / Mathf.Max(MaxHealth.Value, 1f)),
            CurrentHealth, MaxHealth
        );

        IsAlive = new RxFlag(() => CurrentHealth.Value > 0f, this, CurrentHealth);
    }
}