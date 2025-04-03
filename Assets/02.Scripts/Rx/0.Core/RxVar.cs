using System;
using System.Collections.Generic;

namespace Akasha
{
    public class RxVar<T> : IRxField, IRxWritable<T>, IRxObservable<T>, IRxDynamicSubscribable
    {
        private T _value;
        private readonly RxSubscription<T> _subscription = new();
        private readonly object _owner;
        private readonly Dictionary<Action<object>, Action<T>> _wrappedSubs = new();

        public RxVar(T initialValue = default, object owner = null)
        {
            ValidateOwner(owner);
            _value = initialValue;
            _owner = owner;
        }

        public T Value => _value;

        public void SetValue(T newValue, object caller)
        {
            if (!IsAuthorized(caller))
                throw new InvalidOperationException($"[RxVar.SetValue] {caller?.GetType().Name}는 RxVar의 값을 변경할 권한이 없습니다.");

            if (!EqualityComparer<T>.Default.Equals(_value, newValue))
            {
                _value = newValue;
                _subscription.NotifyAll(newValue);
            }
        }

        public void SubscribeLaw(Action<T> subscriber, object context, RxType relationType)
        {
            RxValidator.ValidateFieldSubscriber(context, _owner);
            _subscription.Add(subscriber, context, relationType);
        }

        public void UnsubscribeLaw(Action<T> subscriber)
        {
            _subscription.Remove(subscriber);
        }

        public IDisposable Bind(Action<T> subscriber, object context, RxType type)
        {
            SubscribeLaw(subscriber, context, type);
            return new DelegateDisposable(() => UnsubscribeLaw(subscriber));
        }

        public void SubscribeByObject(Action<object> callback, object context, RxType type)
        {
            void Wrapped(T val) => callback(val);
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

        private bool IsAuthorized(object caller)
        {
            return caller == _owner || caller is IRxUnsafe;
        }

        private void ValidateOwner(object owner)
        {
            if (owner is not IRxFieldOwner && owner is not IRxExprOwner && owner is not IRxUnsafe)
                throw new InvalidOperationException($"[RxVar] {owner?.GetType().Name}는 RxVar의 유효한 소유자가 아닙니다.");
        }
    }
}
