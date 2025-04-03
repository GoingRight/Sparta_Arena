using UnityEngine;
using Akasha;
public interface IMobController
{
    Vector3 MoveDirection { get; }
    void Move(Vector3 moveDir, Vector3 lookDir);
    Transform transform { get; }
}


[RequireComponent(typeof(Rigidbody))]
public class AndroidMobController : BaseController<AndroidMobEntity>, IMobController
{
    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float RunThreshold = 0.5f;

    public Vector3 MoveDirection { get; private set; }
    private Rigidbody _rb;

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

    public void PlayAct1() => Entity.stateMachine.Request(MobState.Act1);
    public void PlayAct2() => Entity.stateMachine.Request(MobState.Act2);
    public void PlayAct3() => Entity.stateMachine.Request(MobState.Act3);
    public void TakeHit() => Entity.stateMachine.Request(MobState.Hit);
    public void Die() => Entity.stateMachine.Request(MobState.Dead);

    Transform IMobController.transform => transform;
}
