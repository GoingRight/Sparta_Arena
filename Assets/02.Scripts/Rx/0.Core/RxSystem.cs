using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Akasha
{
    public enum RxType
    {
        Functional,
        Logical
    }

    // ----- Reactive Core -----

    public interface IRxField { }
    public interface IRxComputed : IRxField { }
    public interface IRxEvent
    {
        void Subscribe(Action subscriber, object context, RxType relationType);
        void Unsubscribe(Action subscriber);
        void Unsubscribe(Action subscriber, object context);
        IDisposable Bind(Action subscriber, object context);
    }

    public interface IRxReadable<T> { T Value { get; } }
    public interface IRxWritable<T> { void SetValue(T value, object caller); }
    public interface IRxDynamicSubscribable
    {
        void SubscribeByObject(Action<object> callback, object context, RxType type);
        void UnsubscribeByObject(Action<object> callback);
    }
    public interface IRxSubscribable { }
    public interface IRxSubscribable<T> : IRxSubscribable
    {
        void SubscribeLaw(Action<T> subscriber, object context, RxType relationType);
        void UnsubscribeLaw(Action<T> subscriber);
    }
    public interface IRxObservable { }
    public interface IRxObservable<T> : IRxReadable<T>, IRxSubscribable<T>, IRxObservable { }

    // ----- Ownership Marker Interfaces -----

    public interface IRxFieldOwner { }
    public interface IRxExprOwner : IRxFieldOwner { }
    public interface ILocalEventOwner { }
    public interface IGlobalEventOwner { }

    // ----- Field Subscriber Role Interfaces -----

    public interface IFiniteFieldSubscriber { }
    public interface IUnfiniteFieldSubscriber { }

    // ----- Event Subscriber Role Interfaces -----

    public interface IFiniteTriggerSubscriber { }
    public interface IUnfiniteTriggerSubscriber { }
    public interface IFiniteLocalEventSubscriber { }
    public interface IUnfiniteLocalEventSubscriber { }
    public interface IGlobalEventSubscriber { }

    // ----- Object Identity Marker Interfaces -----

    public interface IRxModel : IRxExprOwner, IFiniteFieldSubscriber { }
    public interface IRxStateMachine : IRxExprOwner, IFiniteFieldSubscriber, IFiniteTriggerSubscriber, IFiniteLocalEventSubscriber, ILocalEventOwner { }
    public interface IScreen : IRxFieldOwner { }
    public interface IRxUnsafe { }

    public interface IInteractor { }
    public interface IManager : IRxExprOwner, IUnfiniteFieldSubscriber, IUnfiniteTriggerSubscriber, IUnfiniteLocalEventSubscriber, IGlobalEventOwner, IGlobalEventSubscriber { }
    public interface IPresenter : IUnfiniteFieldSubscriber, IUnfiniteLocalEventSubscriber, IGlobalEventSubscriber { }

    public interface IWidget : IRxFieldOwner, IUnfiniteTriggerSubscriber, IUnfiniteLocalEventSubscriber, IGlobalEventSubscriber { }
    public interface IGoldbug : IRxFieldOwner, IUnfiniteTriggerSubscriber, IUnfiniteLocalEventSubscriber, IGlobalEventSubscriber { }

    // ----- Execution Queue -----

    public static class RxQueue
    {
        private static readonly Queue<Action> _queue = new();
        private static readonly HashSet<object> _keys = new();
        private static readonly Dictionary<object, Action> _keyedActions = new();

        public static void Enqueue(Action action, object key = null)
        {
            if (action == null) return;

            if (key == null)
            {
                _queue.Enqueue(action);
            }
            else if (!_keys.Contains(key))
            {
                _keys.Add(key);
                _keyedActions[key] = action;
                _queue.Enqueue(() =>
                {
                    _keys.Remove(key);
                    _keyedActions.Remove(key);
                    action();
                });
            }
        }

        public static void ExecuteAll()
        {
            while (_queue.Count > 0)
            {
                var action = _queue.Dequeue();
                try { action(); }
                catch (Exception e)
                {
                    Debug.LogError($"[RxQueue] 작업 실행 중 오류: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        public static void Cancel(object key)
        {
            if (key != null && _keys.Contains(key))
            {
                _keys.Remove(key);
                _keyedActions.Remove(key);
            }
        }

        public static int Count => _queue.Count;
    }

    // ----- Reactive Context -----

    public static class RxFlow
    {
        [ThreadStatic] private static Stack<object> _contextStack;

        public static object CurrentContext => (_contextStack != null && _contextStack.Count > 0) ? _contextStack.Peek() : null;

        public static void PushContext(object context)
        {
            _contextStack ??= new Stack<object>();
            _contextStack.Push(context);
        }

        public static void PopContext() => _contextStack?.Pop();

        public static void Clear() => _contextStack?.Clear();

        public static string Snapshot()
        {
            if (_contextStack == null || _contextStack.Count == 0)
                return "[RxFlow] (empty)";
            return "[RxFlow] Stack Trace:\n" + string.Join("\n", _contextStack.Select(x => $"- {x?.GetType().Name}"));
        }
    }

    public static class RxFlowExtension
    {
        public static TResult WithContext<TResult>(this object contextOwner, Func<TResult> action)
        {
            RxFlow.PushContext(contextOwner);
            try { return action(); }
            finally { RxFlow.PopContext(); }
        }

        public static void WithContext(this object contextOwner, Action action)
        {
            RxFlow.PushContext(contextOwner);
            try { action(); }
            finally { RxFlow.PopContext(); }
        }
    }

    // ----- Disposable Helpers -----

    public class SubscriptionDisposable<T> : IDisposable
    {
        private IRxSubscribable<T> _target;
        private Action<T> _subscriber;

        public SubscriptionDisposable(IRxSubscribable<T> target, Action<T> subscriber)
        {
            _target = target;
            _subscriber = subscriber;
        }

        public void Dispose()
        {
            _target?.UnsubscribeLaw(_subscriber);
            _target = null;
            _subscriber = null;
        }
    }

    public class DeltaDisposable<T> : IDisposable
    {
        private RxList<T> _target;
        private Action<ListDelta<T>> _handler;

        public DeltaDisposable(RxList<T> target, Action<ListDelta<T>> handler)
        {
            _target = target;
            _handler = handler;
        }

        public void Dispose()
        {
            _target?.UnsubscribeDelta(_handler);
            _target = null;
            _handler = null;
        }
    }

    // ----- Event Binding Helper -----

    public static class RxEventDisposable
    {
        public static IDisposable Create(IRxEvent rxEvent, Action subscriber, object context)
        {
            rxEvent.Subscribe(subscriber, context, RxType.Logical);
            return new SubscriptionDisposable<Unit>(
                (IRxSubscribable<Unit>)rxEvent,
                _ => subscriber()
            );
        }
    }

    // ----- Subscription Core -----

    public class RxSubscription<T>
    {
        private readonly Dictionary<object, List<Subscriber>> _subscribers = new();

        private struct Subscriber
        {
            public Action<T> Callback;
            public RxType RelationType;
            public Subscriber(Action<T> callback, RxType type) { Callback = callback; RelationType = type; }
        }

        public void Add(Action<T> subscriber, object context, RxType relationType)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!_subscribers.TryGetValue(context, out var list))
                _subscribers[context] = list = new List<Subscriber>();

            if (!list.Any(s => s.Callback == subscriber && s.RelationType == relationType))
                list.Add(new Subscriber(subscriber, relationType));
        }

        public void Remove(Action<T> subscriber)
        {
            var keysToRemove = new List<object>();
            foreach (var kvp in _subscribers)
            {
                kvp.Value.RemoveAll(sub => sub.Callback == subscriber);
                if (kvp.Value.Count == 0) keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove) _subscribers.Remove(key);
        }

        public void Remove(Action<T> subscriber, object context)
        {
            if (_subscribers.TryGetValue(context, out var list))
            {
                list.RemoveAll(sub => sub.Callback == subscriber);
                if (list.Count == 0)
                    _subscribers.Remove(context);
            }
        }

        public void NotifyAll(T value)
        {
            foreach (var list in _subscribers.Values)
                foreach (var sub in list)
                    sub.Callback?.Invoke(value);
        }

        public int SubscriberCount => _subscribers.Sum(kvp => kvp.Value.Count);
    }

    public class RxSubscription : RxSubscription<Unit>, IRxSubscribable<Unit>
    {
        public void Add(Action subscriber, object context, RxType relationType)
            => Add(_ => subscriber(), context, relationType);

        public void Remove(Action subscriber)
            => Remove(_ => subscriber());

        public void NotifyAll()
            => NotifyAll(Unit.Default);

        void IRxSubscribable<Unit>.SubscribeLaw(Action<Unit> subscriber, object context, RxType relationType)
            => Add(subscriber, context, relationType);

        void IRxSubscribable<Unit>.UnsubscribeLaw(Action<Unit> subscriber)
            => Remove(subscriber);
    }

    public readonly struct Unit { public static readonly Unit Default = new(); }

    // ----- Binding System -----

    public static class RxBinder
    {
        private static readonly Dictionary<object, List<IDisposable>> _bindings = new();

        public static IDisposable Bind<T>(IRxReadable<T> source, Action<T> apply, object context)
        {
            if (source is not IRxSubscribable<T> subscribable)
                throw new InvalidOperationException("[RxBind] 해당 RxReadable은 구독할 수 없습니다.");

            void Callback(T value) => apply?.Invoke(value);

            subscribable.SubscribeLaw(Callback, context, RxType.Logical);

            var disposable = new SubscriptionDisposable<T>(subscribable, Callback);

            if (!_bindings.ContainsKey(context))
                _bindings[context] = new List<IDisposable>();

            _bindings[context].Add(disposable);
            return disposable;
        }

        public static IDisposable BindEach<T>(
            RxList<T> list,
            Action<T> onAdd,
            Action<T> onRemove = null,
            object context = null
        )
        {
            void DeltaHandler(ListDelta<T> delta)
            {
                switch (delta)
                {
                    case ListDelta<T>.Add add:
                        onAdd?.Invoke(add.Item);
                        break;
                    case ListDelta<T>.Insert insert:
                        onAdd?.Invoke(insert.Item);
                        break;
                    case ListDelta<T>.Remove remove:
                        onRemove?.Invoke(remove.Item);
                        break;
                    case ListDelta<T>.RemoveAt removeAt:
                        onRemove?.Invoke(removeAt.Item);
                        break;
                    case ListDelta<T>.Replace replace:
                        onRemove?.Invoke(replace.OldItem);
                        onAdd?.Invoke(replace.NewItem);
                        break;
                    case ListDelta<T>.Clear:
                        break;
                }
            }

            list.SubscribeDelta(DeltaHandler, context, RxType.Logical);

            var disposable = new DeltaDisposable<T>(list, DeltaHandler);

            if (!_bindings.ContainsKey(context))
                _bindings[context] = new List<IDisposable>();

            _bindings[context].Add(disposable);
            return disposable;
        }

        public static void UnbindAll(object context)
        {
            if (_bindings.TryGetValue(context, out var list))
            {
                foreach (var disposable in list)
                    disposable.Dispose();

                _bindings.Remove(context);
            }
        }
    }
}
