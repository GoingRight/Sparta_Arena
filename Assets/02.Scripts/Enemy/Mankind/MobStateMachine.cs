using Akasha;
using UnityEngine;
using System;
using System.Collections.Generic;

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
    private Vector3 selfPosition => _positionProvider?.Invoke() ?? Vector3.zero;
    private Func<Vector3>? _positionProvider;

    private MobGroupStatusModel group;

    public MobStateMachine(object owner, Func<Vector3> positionProvider) : base(owner, MobState.Idle)
    {
        _positionProvider = positionProvider;

        Register(MobState.Idle, IsAlways);
        Register(MobState.Walk, IsAlways);
        Register(MobState.Run, IsAlways);
        Register(MobState.Act1, IsAlways);
        Register(MobState.Act2, IsAlways);
        Register(MobState.Act3, IsAlways);
        Register(MobState.Hit, IsAlways);
        Register(MobState.Dead, IsAlways);

        group = MobManager.Instance.GroupStatus;

        Register(MobState.Retreat, () => IsPlayerNear() && IsWeakGroup());
        Register(MobState.Buff, () => IsGroupDense() && IsSafe());
        Register(MobState.Debuff, () => IsPlayerNear() && IsGroupAggressive());

        RxBinder.Bind(group.AvgHealth, _ => TickAutoStates(), this);
        RxBinder.Bind(group.UnderAttack, _ => TickAutoStates(), this);
    }

    private bool IsAlways() => true;

    public void TickAutoStates()
    {
        TryRequest(MobState.Retreat);
        TryRequest(MobState.Buff);
        TryRequest(MobState.Debuff);
    }

    private void TryRequest(MobState state)
    {
        Request(state); // RxStateMachine이 조건 검사 내부 처리
    }

    private bool IsPlayerNear()
    {
        var playerPos = MobManager.Instance.PlayerPosition;
        return (playerPos - selfPosition).sqrMagnitude < 4f;
    }

    private bool IsWeakGroup() => group.AvgHealth.Value < 0.5f;
    private bool IsGroupDense() => group.AllyCount.Value >= 6;
    private bool IsSafe() => !group.UnderAttack.Value;
    private bool IsGroupAggressive() => group.UnderAttack.Value;

    public void Update(Vector3 moveDir, float walkThreshold, float runThreshold)
    {
        TickAutoStates();

        if (ActiveState.Value == MobState.Dead || ActiveState.Value == MobState.Retreat)
            return;

        float speed = new Vector3(moveDir.x, 0f, moveDir.z).magnitude;

        if (speed > runThreshold)
            Request(MobState.Run);
        else if (speed > walkThreshold)
            Request(MobState.Walk);
        else
            Request(MobState.Idle);
    }

    public bool Is(MobState state) => ActiveState.Value == state;
    public bool IsActing => Is(MobState.Act1) || Is(MobState.Act2) || Is(MobState.Act3);
}