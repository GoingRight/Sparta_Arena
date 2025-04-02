using UnityEngine;
using Akasha;

[RequireComponent(typeof(Rigidbody))]
public class AndroidMobController : BaseController<AndroidMobEntity>
{
    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float RunThreshold = 0.5f;
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

    public void PlayAct1() => Entity.stateMachine.Request(MobState.Act1);
    public void PlayAct2() => Entity.stateMachine.Request(MobState.Act2);
    public void PlayAct3() => Entity.stateMachine.Request(MobState.Act3);
    public void TakeHit() => Entity.stateMachine.Request(MobState.Hit);
    public void Die() => Entity.stateMachine.Request(MobState.Dead);
}