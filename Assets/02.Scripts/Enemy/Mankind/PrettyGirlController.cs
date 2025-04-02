using UnityEngine;
using Akasha;

[RequireComponent(typeof(Rigidbody))]
public class PrettyGirlController : BaseController<PrettyGirlEntity>, IMobController
{
    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float RunThreshold = 0.5f;

    [Header("Weapon Ranges")]
    public float meleeRange = 2f;
    public float midRange = 4.5f;

    private Rigidbody _rb;



    public Vector3 MoveDirection { get; private set; }

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (Entity?.stateMachine == null) return;

        // 상태 업데이트
        Entity.stateMachine.Update(MoveDirection, 0.1f, RunThreshold);

        // 무기 자동 교체 판단
        UpdateWeaponByDistance();
    }

    public void Move(Vector3 direction)
    {
        if (Entity?.stateMachine == null) return;
        if (Entity.stateMachine.Is(MobState.Dead) || Entity.stateMachine.Is(MobState.Hit))
            return;

        float distance = direction.magnitude;

        float speedFactor = Mathf.Clamp01(distance / 0.5f);
        Vector3 move = direction.normalized * MoveSpeed * speedFactor;

        move.y = _rb.velocity.y;
        _rb.velocity = move;

        MoveDirection = direction;

        if (distance > 0.2f)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    private void UpdateWeaponByDistance()
    {
        if (Entity == null || MobManager.Instance == null) return;

        float distToPlayer = Vector3.Distance(transform.position, MobManager.Instance.PlayerPosition);

        // 거리 + 전략 기준으로 무기 선택
        if (distToPlayer < meleeRange)
            Entity.SetWeapon(WeaponType.Sword);
        else if (distToPlayer < midRange)
            Entity.SetWeapon(WeaponType.Spear);
        else
            Entity.SetWeapon(WeaponType.Rifle);
    }

    // 외부 호출용: 무기 타입에 따른 공격
    public void Attack()
    {
        switch (Entity.CurrentWeapon)
        {
            case WeaponType.Sword: Entity.stateMachine.Request(MobState.Act1); break;
            case WeaponType.Spear: Entity.stateMachine.Request(MobState.Act2); break;
            case WeaponType.Rifle: Entity.stateMachine.Request(MobState.Act3); break;
        }
    }

    public void TakeHit() => Entity.stateMachine.Request(MobState.Hit);
    public void Die() => Entity.stateMachine.Request(MobState.Dead);

    Transform IMobController.transform => this.transform;
}