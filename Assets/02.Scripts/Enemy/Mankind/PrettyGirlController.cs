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

        Entity.stateMachine.Update(MoveDirection, 0.1f, RunThreshold);
        UpdateWeaponByDistance();
    }

    public void Move(Vector3 moveDir, Vector3 lookDir)
    {
        if (Entity?.stateMachine == null) return;
        if (Entity.stateMachine.Is(MobState.Dead) || Entity.stateMachine.Is(MobState.Hit))
            return;

        float speedFactor = Mathf.Clamp01(moveDir.magnitude / 0.5f);
        Vector3 move = moveDir.normalized * MoveSpeed * speedFactor;
        move.y = _rb.velocity.y;
        _rb.velocity = move;

        MoveDirection = moveDir;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(lookDir.x, 0, lookDir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    private void UpdateWeaponByDistance()
    {
        if (Entity == null || MobManager.Instance == null) return;

        float distToPlayer = Vector3.Distance(transform.position, MobManager.Instance.PlayerPosition);

        if (distToPlayer < meleeRange)
            Entity.SetWeapon(WeaponType.Sword);
        else if (distToPlayer < midRange)
            Entity.SetWeapon(WeaponType.Spear);
        else
            Entity.SetWeapon(WeaponType.Rifle);
    }

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

    Transform IMobController.transform => transform;
}
