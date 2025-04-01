using System;
using System.Collections.Generic;
using System.Linq;
using Akasha;


public class RxStateMachine<T> : IRxStateMachine, IFunctionalSubscriber, IFiniteTriggerSubscriber, IFiniteLocalEventSubscriber where T : Enum
{
    protected readonly Dictionary<T, StateInfo> _states = new();
    private readonly HashSet<T> _activeStates = new();
    private object? _owner;

    protected class StateInfo
    {
        public int Priority;
        public Func<bool>? Condition;
        public RxFlag? Flag;
    }

    public void Setup(object owner)
    {
        _owner = owner;
    }

    public void Register(T state, int priority)
    {
        _states[state] = new StateInfo { Priority = priority };
    }

    public void AddCondition(T state, Func<bool> condition)
    {
        if (_states.TryGetValue(state, out var info))
            info.Condition = condition;
        else
            _states[state] = new StateInfo { Priority = 0, Condition = condition };
    }

    public void RequestState(T state)
    {
        if (!_states.TryGetValue(state, out var info)) return;

        // 조건 우선 적용
        if (info.Condition != null && !info.Condition()) return;

        // 단일 상태만 유지
        var current = _activeStates.FirstOrDefault();
        if (!_activeStates.Contains(state))
        {
            if (current != null && _states[current].Priority > info.Priority)
                return;

            _activeStates.Clear();
            _activeStates.Add(state);
            NotifyFlags();
        }
    }

    public bool IsActive(T state) => _activeStates.Contains(state);

    public RxFlag CreateFlag(params T[] targetStates)
    {
        return new RxFlag(() => targetStates.Any(IsActive), _owner!);
    }

    private void NotifyFlags()
    {
        foreach (var kvp in _states)
        {
            kvp.Value.Flag?.GetType().GetMethod("Recalculate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(kvp.Value.Flag, null);
        }
    }
}
