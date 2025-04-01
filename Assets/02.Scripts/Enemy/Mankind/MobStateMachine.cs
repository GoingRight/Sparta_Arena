using Akasha;
using UnityEngine;

public enum MobState
{
    Idle,
    Walk,
    Run,
    Act1,
    Act2,
    Act3,
    Hit,
    Dead,
    Retreat,
    Buff,
    Debuff
}

public static class MobStatePriority
{
    public const int Idle = 10;
    public const int Walk = 20;
    public const int Run = 30;
    public const int Act = 80;
    public const int Hit = 90;
    public const int Dead = 1000;
    public const int Retreat = 95;
    public const int Buff = 70;
    public const int Debuff = 75;
}

public class MobStateMachine : RxStateMachine<MobState>
{
    public RxFlag IsIdle, IsWalk, IsRun, IsActing, IsHit, IsDead, IsRetreating, IsBuffing, IsDebuffing;

    private Vector3 selfPosition => _positionProvider?.Invoke() ?? Vector3.zero;
    private System.Func<Vector3>? _positionProvider;

    private MobGroupStatusModel group;

    public void Setup(object owner, System.Func<Vector3> positionProvider)
    {
        base.Setup(owner);
        _positionProvider = positionProvider;

        Register(MobState.Idle, MobStatePriority.Idle);
        Register(MobState.Walk, MobStatePriority.Walk);
        Register(MobState.Run, MobStatePriority.Run);
        Register(MobState.Act1, MobStatePriority.Act);
        Register(MobState.Act2, MobStatePriority.Act);
        Register(MobState.Act3, MobStatePriority.Act);
        Register(MobState.Hit, MobStatePriority.Hit);
        Register(MobState.Dead, MobStatePriority.Dead);
        Register(MobState.Retreat, MobStatePriority.Retreat);
        Register(MobState.Buff, MobStatePriority.Buff);
        Register(MobState.Debuff, MobStatePriority.Debuff);

        group = MobManager.Instance.GroupStatus;
        RxBinder.Bind(group.AvgHealth, _ => TickAutoStates(), this);
        RxBinder.Bind(group.UnderAttack, _ => TickAutoStates(), this);

        AddCondition(MobState.Retreat, () => IsPlayerNear() && IsWeakGroup());
        AddCondition(MobState.Buff, () => IsGroupDense() && IsSafe());
        AddCondition(MobState.Debuff, () => IsPlayerNear() && IsGroupAggressive());

        IsIdle = CreateFlag(MobState.Idle);
        IsWalk = CreateFlag(MobState.Walk);
        IsRun = CreateFlag(MobState.Run);
        IsActing = CreateFlag(MobState.Act1, MobState.Act2, MobState.Act3);
        IsHit = CreateFlag(MobState.Hit);
        IsDead = CreateFlag(MobState.Dead);
        IsRetreating = CreateFlag(MobState.Retreat);
        IsBuffing = CreateFlag(MobState.Buff);
        IsDebuffing = CreateFlag(MobState.Debuff);
    }

    public void TickAutoStates()
    {
        TryRequest(MobState.Retreat);
        TryRequest(MobState.Buff);
        TryRequest(MobState.Debuff);
    }

    private void TryRequest(MobState state)
    {
        if (_states.TryGetValue(state, out var info))
        {
            if (info.Condition?.Invoke() == true && !IsActive(state))
                RequestState(state);
        }
    }

    private bool IsPlayerNear()
    {
        var playerPos = MobManager.Instance.PlayerPosition;
        return (playerPos - selfPosition).sqrMagnitude < 4f;
    }

    private bool IsWeakGroup()
    {
        return group.AvgHealth.Value < 0.5f;
    }

    private bool IsGroupDense()
    {
        return group.AllyCount.Value >= 6;
    }

    private bool IsSafe()
    {
        return !group.UnderAttack.Value;
    }

    private bool IsGroupAggressive()
    {
        return group.UnderAttack.Value;
    }

    public void Update(Vector3 moveDir, float walkThreshold, float runThreshold)
    {
        TickAutoStates();

        if (IsActive(MobState.Dead) || IsActive(MobState.Retreat)) return;

        float speed = new Vector3(moveDir.x, 0f, moveDir.z).magnitude;

        if (speed > runThreshold)
            RequestState(MobState.Run);
        else if (speed > walkThreshold)
            RequestState(MobState.Walk);
        else
            RequestState(MobState.Idle);
    }
}