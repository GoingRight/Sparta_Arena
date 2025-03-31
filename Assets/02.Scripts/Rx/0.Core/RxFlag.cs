using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Akasha
{
    public class RxFlag : IRxComputed, IRxObservable<bool>, IRxDynamicSubscribable
    {
        private bool _value;
        private readonly RxSubscription<bool> _subscription = new();

        private readonly RxExpr<bool> _expression;
        private readonly Action<object> _recalc;

        private readonly Dictionary<Action<object>, Action<bool>> _wrappedSubs = new();

        public bool Value => _value;
        public object? ReactiveOwner { get; private set; }

        public RxFlag(IRxObservable expression, object owner)
        {
            if (expression is not IRxObservable<bool> boolExpr)
                throw new InvalidOperationException("[RxFlag] expression은 IRxObservable<bool>이어야 합니다.");

            if (!IsValidOwner(owner))
                throw new InvalidOperationException($"[RxFlag.ctor] {owner?.GetType().Name}는 RxFlag의 유효한 소유자가 아닙니다.");

            ReactiveOwner = owner;

            _expression = boolExpr as RxExpr<bool>
                ?? throw new InvalidOperationException("[RxFlag] RxExpr<bool>만 허용됩니다.");

            _recalc = _ => Recalculate();

            _value = this.WithContext(() => _expression.Value);
            _expression.SubscribeLaw(v => _recalc(v), this, RxType.Functional);
        }

        public RxFlag(Func<bool> compute, object owner, params IRxObservable[] dependencies)
            : this(new RxExpr<bool>(compute, dependencies), owner)
        {
        }

        private void Recalculate()
        {
            var newValue = this.WithContext(() => _expression.Value);
            if (_value != newValue)
            {
                _value = newValue;
                _subscription.NotifyAll(_value);
            }
        }

        public void SubscribeLaw(Action<bool> subscriber, object context, RxType type)
        {
            if (type != RxType.Functional && type != RxType.Logical)
                throw new InvalidOperationException("[RxFlag.Subscribe] Functional 또는 Logical 구독만 허용됩니다.");

            RxValidator.ValidateFieldSubscriber(context, this);
            _subscription.Add(subscriber, context, type);
        }

        public IDisposable Bind(Action<bool> subscriber, object context, RxType type)
        {
            SubscribeLaw(subscriber, context, type);
            return new SubscriptionDisposable<bool>(this, subscriber);
        }

        public void UnsubscribeLaw(Action<bool> subscriber)
        {
            _subscription.Remove(subscriber);
        }

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
            _expression.UnsubscribeByObject(_recalc);
        }

        private static bool IsValidOwner(object owner)
        {
            return owner is IRxFlagger or IScreen or IRxUnsafe or IManager or IRxModelOwner;
        }
    }
}