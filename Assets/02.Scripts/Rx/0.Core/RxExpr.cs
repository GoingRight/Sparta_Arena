using System;
using System.Collections.Generic;

namespace Akasha
{
    public class RxExpr<T> : IRxComputed, IRxObservable<T>, IRxDynamicSubscribable
    {
        private T _value;
        private readonly Func<T> _compute;
        private readonly RxSubscription<T> _subscription = new();

        private readonly List<IRxObservable> _dependencies = new();
        private readonly Dictionary<Action<object>, Action<T>> _wrappedSubs = new();
        private readonly Action<object> _recalc;

        public T Value => _value;

        public RxExpr(Func<T> compute, params IRxObservable[] dependencies)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
            _recalc = _ => Recalculate();

            foreach (var dep in dependencies)
            {
                if (dep is IRxDynamicSubscribable sub)
                {
                    sub.SubscribeByObject(_recalc, this, RxType.Functional);
                    _dependencies.Add(dep);
                }
            }

            _value = this.WithContext(() => _compute.Invoke());
        }

        private void Recalculate()
        {
            var newValue = this.WithContext(() => _compute.Invoke());
            if (!EqualityComparer<T>.Default.Equals(_value, newValue))
            {
                _value = newValue;
                _subscription.NotifyAll(_value);
            }
        }

        public void SubscribeLaw(Action<T> subscriber, object context, RxType relationType)
        {
            if (relationType != RxType.Functional && relationType != RxType.Logical)
                throw new InvalidOperationException("[RxExpr.Subscribe] Functional 또는 Logical 구독만 허용됩니다.");

            RxValidator.ValidateFieldSubscriber(context, this);
            _subscription.Add(subscriber, context, relationType);
            subscriber(_value);
        }

        public IDisposable Bind(Action<T> subscriber, object context, RxType relationType)
        {
            SubscribeLaw(subscriber, context, relationType);
            return new DelegateDisposable(() => UnsubscribeLaw(subscriber));
        }

        public void UnsubscribeLaw(Action<T> subscriber)
        {
            _subscription.Remove(subscriber);
        }

        public void SubscribeByObject(Action<object> callback, object context, RxType type)
        {
            void Wrapped(T value) => callback(value);
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
            foreach (var dep in _dependencies)
            {
                if (dep is IRxDynamicSubscribable sub)
                    sub.UnsubscribeByObject(_recalc);
            }

            _dependencies.Clear();
            _wrappedSubs.Clear();
        }
    }
}
