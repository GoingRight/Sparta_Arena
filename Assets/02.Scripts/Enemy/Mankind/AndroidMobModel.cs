using Akasha;
using UnityEngine;

public class AndroidMobModel : RxModel, IMobHealthReadable
{
    // ✅ 게임 데이터 저장 필드 (RxVar)
    public RxVar<float> CurrentHealth { get; private set; }
    public RxVar<StrategyType> Strategy { get; private set; }
    public RxVar<DanceType> Dance { get; private set; }

    // ✅ 외부 연결 가능한 파생값 (RxExpr)
    public RxExpr<float> HealthRatioExpr { get; private set; }

    // ✅ 파생값을 Boolean 상태로 표시하는 Flag (RxFlag)
    public RxFlag IsAlive { get; private set; }

    public float HealthRatio => HealthRatioExpr.Value;

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        CurrentHealth = new RxVar<float>(100f, this);
        Strategy = new RxVar<StrategyType>(StrategyType.Idle, this);
        Dance = new RxVar<DanceType>(DanceType.Buff, this);

        HealthRatioExpr = new RxExpr<float>(
            () => Mathf.Clamp01(CurrentHealth.Value / 100f),
            CurrentHealth
        );

        IsAlive = new RxFlag(() => CurrentHealth.Value > 0f, this, CurrentHealth);
    }
}