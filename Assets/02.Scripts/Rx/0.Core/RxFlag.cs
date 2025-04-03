using System;
using System.Collections.Generic;

namespace Akasha
{
    public class RxFlag : IRxFlag, IRxComputed, IRxObservable<bool>, IRxDynamicSubscribable, IRxExprOwner, IFiniteFieldSubscriber
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

            _expression = new RxExpr<bool>(compute, dependencies);
            foreach (var dep in dependencies)
                (dep as IRxDynamicSubscribable)?.SubscribeByObject(_recalc, this, RxType.Functional);

            // 핵심: _state의 수동 변경도 반응하도록
            _state.SubscribeLaw(_ => Recalculate(), this, RxType.Functional);

            Recalculate();
        }

        public RxFlag(RxExpr<bool> expr, object owner)
        {
            ValidateOwner(owner);
            ReactiveOwner = owner;
            _expression = expr;
            _state = new RxVar<bool>(false, owner);
            _recalc = _ => Recalculate();

            expr.SubscribeLaw(_ => _recalc(_), this, RxType.Functional);
            _state.SubscribeLaw(_ => Recalculate(), this, RxType.Functional); // 핵심

            Recalculate();
        }

        public void RecalculatePublic() => Recalculate();

        public void SetManual(bool value)
        {
            if (_expression != null)
                throw new InvalidOperationException("[RxFlag] Expression이 있는 상태에서는 수동 제어가 불가능합니다.");

            _state.SetValue(value, this); // 이때도 위에서 구독 중이므로 Recalculate 유발됨
        }

        private void Recalculate()
        {
            if (_expression == null) return;

            var newVal = this.WithContext(() => _expression.Value);

            // 기존 값과 다를 때만 SetValue → 무한루프 방지
            if (!EqualityComparer<bool>.Default.Equals(_state.Value, newVal))
            {
                _state.SetValue(newVal, this);
            }
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
            _expression?.UnsubscribeByObject(_recalc);
            _expression = null;
            _wrappedSubs.Clear();
        }

        private static void ValidateOwner(object owner)
        {
            if (owner is not IRxFieldOwner && owner is not IRxExprOwner && owner is not IRxUnsafe)
                throw new InvalidOperationException($"[RxFlag] {owner?.GetType().Name}는 RxFlag의 유효한 소유자가 아닙니다.");
        }
    }
}