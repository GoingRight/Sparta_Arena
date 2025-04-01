using UnityEngine;
using Akasha;

[RequireComponent(typeof(Rigidbody))]
public class AndroidMobController : BaseController<AndroidMobEntity>
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float RunThreshold = 3f;
    public Vector3 MoveDirection;

    private Rigidbody _rb;
    private MobStateModel _stateModel;

    protected override void OnControllerEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _stateModel = new MobStateModel();
        _stateModel.Setup(this);

        _stateModel.State.Bind(OnStateChanged, this, RxType.Logical);
    }

    private void Update()
    {
        EvaluateState();
    }

    private void EvaluateState()
    {
        if (_stateModel.IsDead.Value) return;

        float speed = new Vector3(MoveDirection.x, 0f, MoveDirection.z).magnitude;

        if (speed > RunThreshold)
            _stateModel.State.SetValue(MobState.Run, this);
        else if (speed > 0.1f)
            _stateModel.State.SetValue(MobState.Walk, this);
        else
            _stateModel.State.SetValue(MobState.Idle, this);
    }

    private void OnStateChanged(MobState state)
    {
        switch (state)
        {
            case MobState.Idle: animator.Play("Idle"); break;
            case MobState.Walk: animator.Play("Walk"); break;
            case MobState.Run: animator.Play("Run"); break;
            case MobState.Act1: animator.Play("Skill1"); break;
            case MobState.Act2: animator.Play("Skill2"); break;
            case MobState.Act3: animator.Play("Skill3"); break;
            case MobState.Hit: animator.Play("Hit"); break;
            case MobState.Dead: animator.Play("Dead"); break;
        }
    }

    public void Move(Vector3 direction)
    {
        MoveDirection = direction;

        Vector3 move = direction.normalized * MoveSpeed;
        move.y = _rb.velocity.y;

        _rb.velocity = move;
    }

    // 외부에서 상태 전이 요청
    public void PlayAct1() => _stateModel.State.SetValue(MobState.Act1, this);
    public void PlayAct2() => _stateModel.State.SetValue(MobState.Act2, this);
    public void PlayAct3() => _stateModel.State.SetValue(MobState.Act3, this);
    public void TakeHit() => _stateModel.State.SetValue(MobState.Hit, this);
    public void Die() => _stateModel.State.SetValue(MobState.Dead, this);
}

public class MobStateModel : RxModel
{
    public RxVar<MobState> State = new(MobState.Idle, owner: null);

    public RxFlag IsIdle;
    public RxFlag IsWalk;
    public RxFlag IsRun;
    public RxFlag IsActing;
    public RxFlag IsHit;
    public RxFlag IsDead;

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        IsIdle = new RxFlag(() => State.Value == MobState.Idle, this, State );
        IsWalk = new RxFlag(() => State.Value == MobState.Walk, this, State);
        IsRun = new RxFlag(() => State.Value == MobState.Run, this, State);
        IsActing = new RxFlag(() =>
            State.Value == MobState.Act1 ||
            State.Value == MobState.Act2 ||
            State.Value == MobState.Act3, this,  State);

        IsHit = new RxFlag(() => State.Value == MobState.Hit, this, State);
        IsDead = new RxFlag(() => State.Value == MobState.Dead, this, State);
    }
}