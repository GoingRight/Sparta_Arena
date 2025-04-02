using Akasha;
using UnityEngine;

public class PrettyGirlModel : RxModel, IMobHealthReadable
{
    // ✅ 게임 데이터 저장 필드
    public RxVar<float> CurrentHealth { get; private set; }
    public RxVar<WeaponType> Weapon { get; private set; }

    // ✅ 외부 연결 파생값
    public RxExpr<float> HealthRatioExpr { get; private set; }

    // ✅ 조건 플래그
    public RxFlag IsAlive { get; private set; }

    public float HealthRatio => HealthRatioExpr.Value;

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        CurrentHealth = new RxVar<float>(100f, this);
        Weapon = new RxVar<WeaponType>(WeaponType.Sword, this); // 기본 무기는 검

        HealthRatioExpr = new RxExpr<float>(
            () => Mathf.Clamp01(CurrentHealth.Value / 100f),
            CurrentHealth
        );

        IsAlive = new RxFlag(() => CurrentHealth.Value > 0f, this, CurrentHealth);
    }
}