using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Akasha
{
    public class RxFlag : IRxComputed, IRxObservable<bool>, IRxDynamicSubscribable, IRxModel
    {
        private readonly RxVar<bool> _state;
        private RxExpr<bool>? _expression;
        private readonly Action<object> _recalc;
        private readonly Dictionary<Action<object>, Action<bool>> _wrappedSubs = new();

        public bool Value => _state.Value;
        public object ReactiveOwner { get; private set; }

        public RxFlag(Func<bool> compute, object owner, params IRxObservable[] dependencies)
        {
            ValidateOwner(owner);
            ReactiveOwner = owner;
            _state = new RxVar<bool>(false, owner);
            _recalc = _ => Recalculate();

            if (dependencies.Length > 0)
            {
                _expression = new RxExpr<bool>(compute, dependencies);
                foreach (var dep in dependencies)
                    (dep as IRxDynamicSubscribable)?.SubscribeByObject(_recalc, this, RxType.Functional);
            }
            else
            {
                _expression = new RxExpr<bool>(compute);
            }

            Recalculate();
        }

        public RxFlag(RxExpr<bool> expr, object owner)
        {
            ValidateOwner(owner);
            ReactiveOwner = owner;
            _expression = expr;
            _state = new RxVar<bool>(false, owner);
            _recalc = _ => Recalculate();

            expr.SubscribeLaw(v => _recalc(v), this, RxType.Functional);
            Recalculate();
        }
        public void RecalculatePublic() => Recalculate();
        private void Recalculate()
        {
            if (_expression != null)
            {
                var newVal = this.WithContext(() => _expression.Value);
                Debug.Log($"[RxFlag] Recalculate: newVal = {newVal}, 현재값 = {_state.Value}");

                _state.SetValue(newVal, this);
            }
        }

        public void SetManual(bool value)
        {
            if (_expression != null)
                throw new InvalidOperationException("[RxFlag] Expression이 있는 상태에서는 수동 제어가 불가능합니다.");
            _state.SetValue(value, this);
        }

        public void SubscribeLaw(Action<bool> subscriber, object context, RxType type)
            => _state.SubscribeLaw(subscriber, context, type);

        public void UnsubscribeLaw(Action<bool> subscriber)
            => _state.UnsubscribeLaw(subscriber);

        public IDisposable Bind(Action<bool> subscriber, object context, RxType type)
            => _state.Bind(subscriber, context, type);

        public void SubscribeByObject(Action<object> callback, object context, RxType type)
        {
            void Wrapped(bool val) => callback(val);
            _wrappedSubs[callback] = Wrapped;
            SubscribeLaw(Wrapped, context, type);
        }

        public void UnsubscribeByObject(Action<object> callback)
        {
            if (_wrappedSubs.TryGetValue(callback, out var wrapped))
            {
                UnsubscribeLaw(wrapped);
                _wrappedSubs.Remove(callback);
            }
        }

        public void Teardown()
        {
            if (_expression != null)
            {
                _expression.UnsubscribeByObject(_recalc);
                _expression = null;
            }
            _wrappedSubs.Clear();
        }

        private static void ValidateOwner(object owner)
        {
            if (owner is not IRxStateMachine and not IScreen and not IRxUnsafe and not IManager and not IRxModel)
            {
                throw new InvalidOperationException($"[RxFlag] {owner?.GetType().Name}는 RxFlag의 유효한 소유자가 아닙니다.");
            }
        }
    }
}
