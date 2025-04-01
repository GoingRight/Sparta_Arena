using UnityEngine;
using Akasha;

[RequireComponent(typeof(Rigidbody))]
public class AndroidMobController : BaseController<AndroidMobEntity>
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float RunThreshold = 3f;
    public Vector3 MoveDirection;

    private Rigidbody _rb;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        var interactor = GetComponent<AndroidMobInteractor>();

    }

    private void Update()
    {
        if (Entity?.stateMachine == null) return;
        Entity.stateMachine.Update(MoveDirection, 0.1f, RunThreshold);
    }

    public void Move(Vector3 direction)
    {
        if (Entity?.stateMachine == null) return;
        if (Entity.stateMachine.IsActive(MobState.Dead) || Entity.stateMachine.IsActive(MobState.Hit))
            return;

        MoveDirection = direction;

        Vector3 move = direction.normalized * MoveSpeed;
        move.y = _rb.velocity.y;
        _rb.velocity = move;

        // 이동 방향으로 회전
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    public void PlayAct1() => Entity.stateMachine.RequestState(MobState.Act1);
    public void PlayAct2() => Entity.stateMachine.RequestState(MobState.Act2);
    public void PlayAct3() => Entity.stateMachine.RequestState(MobState.Act3);
    public void TakeHit() => Entity.stateMachine.RequestState(MobState.Hit);
    public void Die() => Entity.stateMachine.RequestState(MobState.Dead);
}