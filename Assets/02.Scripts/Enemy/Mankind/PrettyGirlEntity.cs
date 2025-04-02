using Akasha;
using UnityEngine;

public class PrettyGirlEntity : BaseEntity
{
    private PrettyGirlModel model;
    public MobStateMachine stateMachine;

    public WeaponType CurrentWeapon => model.Weapon.Value;

    protected override void SetupModels()
    {
        model = new PrettyGirlModel();
        model.Setup(this);
        MobManager.Instance.Register(model);
    }

    protected override void SetupParts()
    {
        stateMachine = new MobStateMachine(this, () => transform.position);
        // 추가 상태 바인딩 필요 시 여기에
    }

    public void SetWeapon(WeaponType newWeapon)
    {
        if (model.Weapon.Value == newWeapon) return;

        model.Weapon.SetValue(newWeapon, model);

        Debug.Log($"[PrettyGirlEntity] 무기 변경됨: {newWeapon}");
    }

    public void Attack()
    {
        switch (CurrentWeapon)
        {
            case WeaponType.Sword:
                stateMachine.Request(MobState.Act1);
                break;
            case WeaponType.Spear:
                stateMachine.Request(MobState.Act2);
                break;
            case WeaponType.Rifle:
                stateMachine.Request(MobState.Act3);
                break;
        }
    }
}