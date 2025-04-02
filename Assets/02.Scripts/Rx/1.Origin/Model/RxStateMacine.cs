using System;
using System.Collections.Generic;

namespace Akasha
{
    public class RxStateMachine<T> : IRxStateMachine, IFunctionalSubscriber, IFiniteTriggerSubscriber, IFiniteLocalEventSubscriber where T : Enum
    {
        public readonly RxVar<T> ActiveState;

        private readonly Dictionary<T, Func<bool>?> _conditions = new();
        private readonly object _owner;

        public RxStateMachine(object owner, T initialState)
        {
            _owner = owner;
            ActiveState = new RxVar<T>(initialState, owner);
        }

        public void Register(T state, Func<bool>? condition = null)
        {
            _conditions[state] = condition;
        }

        public void Request(T state)
        {
            if (_conditions.TryGetValue(state, out var condition))
            {
                if (condition != null && !condition())
                    return;
            }

            ActiveState.SetValue(state, _owner);
        }

        public bool IsActive(T state)
        {
            return EqualityComparer<T>.Default.Equals(ActiveState.Value, state);
        }
    }
}