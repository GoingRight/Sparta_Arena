using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Akasha
{
    public class RxVar<T> : IReactiveValue<T>
    {
        private T _value;
        private event Action<T> _onFunctionalChanged;
        private readonly List<LogicalSubscriber> _logicalSubscribers = new();
        private readonly Dictionary<Action, Action<T>> _rawDelegateMap = new();

        public RxVar(T initialValue)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;

                _value = value;

                _onFunctionalChanged?.Invoke(_value);
                foreach (var sub in _logicalSubscribers)
                    RxQueue.EnqueueLogical(() => sub.Callback(_value), sub.Priority);
            }
        }
      
        public void Subscribe(Action<T> callback, object subscriber, RxType type, int priority = 0)
        {
            if (!RxBind.IsInRxBind && subscriber is MonoBehaviour mono)
            {
                Debug.LogWarning($"[Rx] {mono.name}에서 Subscribe 직접 호출됨. RxBind 사용을 권장합니다.");
            }

            if (subscriber == null) 
                throw new ArgumentNullException(nameof(subscriber));

            switch (type)
            {
                case RxType.Functional:
                    ValidateFunctionalSubscriber(subscriber);
                    _onFunctionalChanged += callback;
                    break;

                case RxType.InteractLogical:
                    ValidateInteractLogicalSubscriber(subscriber);
                    InsertLogicalSubscriberSorted(callback, priority);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"ReactiveProperty는 {type} 관계로 구독할 수 없습니다.");
            }
        }


        public void Unsubscribe(Action<T> callback)
        {
            _onFunctionalChanged -= callback;
            _logicalSubscribers.RemoveAll(sub => sub.Callback == callback);
        }

        void IReactiveReader.SubscribeRaw(Action onChanged, object subscriber, RxType type)
        {
            if (_rawDelegateMap.ContainsKey(onChanged)) return;

            Action<T> wrapper = _ => onChanged();
            _rawDelegateMap[onChanged] = wrapper;
            Subscribe(wrapper, subscriber, type);
        }

        void IReactiveReader.UnsubscribeRaw(Action onChanged)
        {
            if (_rawDelegateMap.TryGetValue(onChanged, out var wrapper))
            {
                Unsubscribe(wrapper);
                _rawDelegateMap.Remove(onChanged);
            }
        }

        private void ValidateFunctionalSubscriber(object subscriber)
        {
            if (subscriber is not IFunctionalSubscriber)
                throw new InvalidOperationException("기능적 관계는 Entity 또는 Part만 허용됩니다.");
            if (ReferenceEquals(subscriber, this))
                throw new InvalidOperationException("ReactiveProperty는 자기 자신을 기능적으로 구독할 수 없습니다. (Backbiting 방지)");
        }

        private void ValidateInteractLogicalSubscriber(object subscriber)
        {
            if (subscriber is not IInteractLogicalSubscriber)
                throw new InvalidOperationException("InteractLogical 구독자는 IInteractLogicalSubscriber를 구현해야 합니다.");
        }

        private void InsertLogicalSubscriberSorted(Action<T> callback, int priority)
        {
            var sub = new LogicalSubscriber(callback, priority);

            int index = _logicalSubscribers.BinarySearch(sub, LogicalSubscriberComparer.Instance);
            if (index < 0) index = ~index;
            _logicalSubscribers.Insert(index, sub);
        }

        private readonly struct LogicalSubscriber
        {
            public readonly Action<T> Callback;
            public readonly int Priority;
            public LogicalSubscriber(Action<T> callback, int priority)
            {
                Callback = callback;
                Priority = priority;
            }
        }

        private class LogicalSubscriberComparer : IComparer<LogicalSubscriber>
        {
            public static readonly LogicalSubscriberComparer Instance = new();
            public int Compare(LogicalSubscriber x, LogicalSubscriber y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }
    }

    public class RxExpr<T> : IReactiveReader<T>, IFunctionalSubscriber, IDisposable
    {
        private T _value;
        private readonly Func<T> _expression;
        private readonly Dictionary<IReactiveReader, Action> _subscriptions = new();
        private readonly Dictionary<Action, Action<T>> _rawDelegateMap = new();
        private readonly List<LogicalSubscriber> _interactSubscribers = new();
        private static readonly HashSet<RxExpr<T>> _evaluationStack = new();

        private event Action<T> _onChanged;

        public RxExpr(Func<T> expression, params IReactiveReader[] sources)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));

            foreach (var source in sources)
            {
                Action handler = Recalculate;
                source.SubscribeRaw(handler, this, RxType.Functional);
                _subscriptions[source] = handler;
            }

            Recalculate();
        }

        public T Value => _value;
        public void Refresh() => Recalculate();

        public void Subscribe(Action<T> callback, object subscriber, RxType type, int priority = 0)
        {
            if (!RxBind.IsInRxBind && subscriber is MonoBehaviour mono)
            {
                Debug.LogWarning($"[Rx] {mono.name}에서 Subscribe 직접 호출됨. RxBind 사용을 권장합니다.");
            }

            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            switch (type)
            {
                case RxType.Functional:
                    if (subscriber is not IFunctionalSubscriber)
                        throw new InvalidOperationException("ReactiveExpression은 Functional 구독자는 IFunctionalSubscriber만 허용합니다.");
                    _onChanged += callback;
                    break;

                case RxType.InteractLogical:
                    if (subscriber is not IInteractLogicalSubscriber)
                        throw new InvalidOperationException("ReactiveExpression은 InteractLogical 구독자는 IInteractLogicalSubscriber만 허용합니다.");
                    InsertInteractSubscriber(callback, priority);
                    break;

                default:
                    throw new InvalidOperationException("ReactiveExpression은 Functional 또는 InteractLogical로만 구독할 수 있습니다.");
            }
        }

        public void Unsubscribe(Action<T> callback)
        {
            _onChanged -= callback;
            _interactSubscribers.RemoveAll(sub => sub.Callback == callback);
        }

        public void UnsubscribeAll()
        {
            _onChanged = null;
            _interactSubscribers.Clear();
        }

        public void Dispose()
        {
            foreach (var (source, handler) in _subscriptions)
                source.UnsubscribeRaw(handler);

            _subscriptions.Clear();
            _rawDelegateMap.Clear();
            UnsubscribeAll();
        }

        public void SubscribeRaw(Action onChanged, object subscriber, RxType type)
        {
            if (_rawDelegateMap.ContainsKey(onChanged)) return;

            Action<T> wrapper = _ => onChanged();
            _rawDelegateMap[onChanged] = wrapper;
            Subscribe(wrapper, subscriber, type);
        }

        public void UnsubscribeRaw(Action onChanged)
        {
            if (_rawDelegateMap.TryGetValue(onChanged, out var wrapper))
            {
                Unsubscribe(wrapper);
                _rawDelegateMap.Remove(onChanged);
            }
        }

        private void Recalculate()
        {
            if (_evaluationStack.Contains(this))
                throw new InvalidOperationException("ReactiveExpression 내부에서 순환 참조가 감지되었습니다.");

            _evaluationStack.Add(this);
            var newValue = _expression();

            if (!EqualityComparer<T>.Default.Equals(_value, newValue))
            {
                _value = newValue;
                _onChanged?.Invoke(_value);

                foreach (var sub in _interactSubscribers)
                    RxQueue.EnqueueLogical(() => sub.Callback(_value), sub.Priority);
            }

            _evaluationStack.Remove(this);
        }

        private void InsertInteractSubscriber(Action<T> callback, int priority)
        {
            var sub = new LogicalSubscriber(callback, priority);
            int index = _interactSubscribers.BinarySearch(sub, LogicalSubscriberComparer.Instance);
            if (index < 0) index = ~index;
            _interactSubscribers.Insert(index, sub);
        }

        private readonly struct LogicalSubscriber
        {
            public readonly Action<T> Callback;
            public readonly int Priority;
            public LogicalSubscriber(Action<T> callback, int priority)
            {
                Callback = callback;
                Priority = priority;
            }
        }

        private class LogicalSubscriberComparer : IComparer<LogicalSubscriber>
        {
            public static readonly LogicalSubscriberComparer Instance = new();
            public int Compare(LogicalSubscriber x, LogicalSubscriber y) => x.Priority.CompareTo(y.Priority);
        }
    }

    public class RxEvent
    {
        private bool _isRaising = false;
        private readonly List<(Action callback, int priority)> _subscribers = new();

        public void Subscribe(Action callback, object subscriber, int priority = 0)
        {
            if (subscriber is not IControlLogicalSubscriber && subscriber is not IInteractLogicalSubscriber)
                throw new InvalidOperationException("ReactiveCommand는 IControlLogicalSubscriber이거나 IInteractLogicalSubscriber만 구독할 수 있습니다.");

            _subscribers.Add((callback, priority));
        }

        public void Unsubscribe(Action callback)
        {
            _subscribers.RemoveAll(pair => pair.callback == callback);
        }

        public void Raise(object issuer)
        {
            if (issuer is not IReactiveEventIssuer)
                throw new InvalidOperationException("ReactiveCommand는 Actor 또는 Presenter만 Raise할 수 있습니다.");

            if (_isRaising) return;
            _isRaising = true;

            foreach (var (callback, priority) in _subscribers)
            {
                RxQueue.EnqueueLogical(callback, priority);
            }

            _isRaising = false;
        }
    }
    public class GameEvent
    {
        private bool _isRaising = false;
        private readonly List<(Action callback, int priority)> _subscribers = new();

        public void Subscribe(Action callback, object subscriber, int priority = 0)
        {
            if (subscriber is not IControlLogicalSubscriber)
                throw new InvalidOperationException("GameEvent는 IControlLogicalSubscriber만 구독할 수 있습니다.");
            _subscribers.Add((callback, priority));
        }

        public void Unsubscribe(Action callback)
        {
            _subscribers.RemoveAll(pair => pair.callback == callback);
        }

        public void Raise(object issuer)
        {
            if (issuer is not IControlLogicalSubscriber)
                throw new InvalidOperationException("GameEvent는 IControlLogicalSubscriber만 Raise할 수 있습니다.");

            if (_isRaising) return;
            _isRaising = true;

            foreach (var (callback, priority) in _subscribers)
            {
                RxQueue.EnqueueLogical(callback, priority);
            }

            _isRaising = false;
        }
    }

}

