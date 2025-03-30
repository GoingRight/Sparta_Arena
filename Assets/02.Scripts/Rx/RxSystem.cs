using System;
using System.Collections.Generic;

namespace Akasha
{
    public enum RxType
    {
        Functional,         // 상태 계산 목적 → World(Entity, Part, Gear), UI(Screen, View, Widget)
        InteractLogical,    // 상향식 모델 변화반응 목적 → World (Actor, Indicator,  Gear)  UI(Presenter, Widget)
        ControlLogical      // 하향식 객체 제어 목적 → Spirit, Manager
    }

    public enum RxListChangeType
    {
        Add,
        Remove,
        Replace,
        Clear
    }

    public enum ListChangeType { Add, Remove, Insert, Replace, Clear }

    public interface IReactiveReader
    {
        void SubscribeRaw(Action onChanged, object subscriber, RxType type);
        void UnsubscribeRaw(Action onChanged);
    }

    public interface IReactiveReader<T> : IReactiveReader
    {
        T Value { get; }
        void Subscribe(Action<T> callback, object subscriber, RxType type, int priority = 0);
        void Unsubscribe(Action<T> callback);
    }

    public interface IReactiveWriter<T>
    {
        T Value { set; }
    }
    
    public interface IReactiveValue<T> : IReactiveReader<T>, IReactiveWriter<T> { }

    public interface IIndicator 
    { 
        void Refresh(); 
    }

    public interface IGameEventManager { }
    public interface IReactiveEventIssuer { }
    public interface IControlLogicalSubscriber { }
    public interface IInteractLogicalSubscriber { }
    public interface IFunctionalSubscriber { }

    public static class RxFlow
    {
        public static bool IsPaused { get; private set; } = false;

        public static void Pause() => IsPaused = true;
        public static void Resume() => IsPaused = false;
    }

    public static class RxLogger
    {
        private static readonly List<string> _logs = new();

        public static void Log(string message)
        {
            _logs.Add($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void Dump()
        {
            Console.WriteLine("==== Reactive Event Log Dump ====");
            foreach (var log in _logs)
            {
                Console.WriteLine(log);
            }
            Console.WriteLine("=================================");
        }

        public static void Clear()
        {
            _logs.Clear();
        }
    }

    public static class RxQueue
    {
        private static readonly SortedDictionary<int, Queue<Action>> logicalQueue = new();

        public static void EnqueueLogical(Action action, int priority)
        {
            if (RxFlow.IsPaused)
            {
                RxLogger.Log($"[Scheduler] Skipped due to pause. Priority: {priority}");
                return;
            }

            if (!logicalQueue.TryGetValue(priority, out var queue))
            {
                queue = new Queue<Action>();
                logicalQueue[priority] = queue;
            }
            queue.Enqueue(action);
            RxLogger.Log($"[Scheduler] Enqueued action at priority {priority}");
        }

        public static void Flush()
        {      
            if (RxFlow.IsPaused)
            {
                RxLogger.Log("[Scheduler] Flush skipped due to pause.");
                return;
            }

            foreach (var (priority, queue) in logicalQueue)
            {
                while (queue.Count > 0)
                {
                    var action = queue.Dequeue();
                    RxLogger.Log($"[Scheduler] Executing action at priority {priority}");
                    action.Invoke();
                }
            }
            logicalQueue.Clear();
            RxLogger.Log("[Scheduler] Flush complete.");
        }
    }
    public static class RxBind
    {
        private static readonly Dictionary<object, List<IDisposable>> _bindings = new();
        [ThreadStatic]
        private static bool _isInRxBind;
        internal static bool IsInRxBind => _isInRxBind;

        private static void EnterBindContext() => _isInRxBind = true;
        private static void ExitBindContext() => _isInRxBind = false;

        public static void Bind<T>(IReactiveReader<T> source, Action<T> onChanged, object owner, RxType type = RxType.InteractLogical, int priority = 0)
        {
            ValidateSubscriber(owner, type);

            EnterBindContext();
            source.Subscribe(onChanged, owner, type, priority);
            ExitBindContext();

            if (type != RxType.ControlLogical)
                onChanged(source.Value);
            
            if (!_bindings.TryGetValue(owner, out var list))
            {
                list = new List<IDisposable>();
                _bindings[owner] = list;
            }

            list.Add(new BindingHandle(() => source.Unsubscribe(onChanged)));
        }

        public static void UnbindAll(object owner)
        {
            if (_bindings.TryGetValue(owner, out var list))
            {
                foreach (var binding in list)
                    binding.Dispose();

                _bindings.Remove(owner);
            }
        }

        private static void ValidateSubscriber(object subscriber, RxType type)
        {
            switch (type)
            {
                case RxType.Functional:
                    if (subscriber is not IFunctionalSubscriber)
                        throw new InvalidOperationException("Functional 구독자는 IFunctionalSubscriber를 구현해야 합니다.");
                    break;
                case RxType.InteractLogical:
                    if (subscriber is not IInteractLogicalSubscriber)
                        throw new InvalidOperationException("InteractLogical 구독자는 IInteractLogicalSubscriber를 구현해야 합니다.");
                    break;
                case RxType.ControlLogical:
                    if (subscriber is not IControlLogicalSubscriber)
                        throw new InvalidOperationException("ControlLogical 구독자는 IControlLogicalSubscriber를 구현해야 합니다.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"알 수 없는 RxRelationType: {type}");
            }
        }

        private class BindingHandle : IDisposable
        {
            private readonly Action _unsubscribe;
            public BindingHandle(Action unsubscribe) => _unsubscribe = unsubscribe;
            public void Dispose() => _unsubscribe?.Invoke();
        }
    }
}

