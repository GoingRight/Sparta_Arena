using System;
using System.Collections;
using System.Collections.Generic;

namespace Akasha
{
    public class RxListChange<T>
    {
        public RxListChangeType ActionType { get; }
        public int Index { get; } = -1;
        public List<T>? OldItems { get; }
        public List<T>? NewItems { get; }

        public RxListChange(RxListChangeType type, int index = -1, List<T>? newItems = null, List<T>? oldItems = null)
        {
            ActionType = type;
            Index = index;
            NewItems = newItems;
            OldItems = oldItems;
        }
    }

    [Serializable]
    public class RxList<T> : IList<T>
    {
        private readonly List<T> _list = new();
        private readonly List<IDisposable> _innerSubscriptions = new();

        public event Action? OnChanged;
        public event Action<RxListChange<T>>? OnChangedDetailed;

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _list[index];
            set
            {
                var old = _list[index];
                _list[index] = value;
                OnChanged?.Invoke();
                OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Replace, index, new() { value }, new() { old }));
            }
        }

        public void Add(T item)
        {
            _list.Add(item);
            OnChanged?.Invoke();
            OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Add, _list.Count - 1, new() { item }));
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnChanged?.Invoke();
            OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Add, index, new() { item }));
        }

        public bool Remove(T item)
        {
            int index = _list.IndexOf(item);
            if (index < 0) return false;
            _list.RemoveAt(index);
            OnChanged?.Invoke();
            OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Remove, index, null, new() { item }));
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _list.Count) return;
            var removed = _list[index];
            _list.RemoveAt(index);
            OnChanged?.Invoke();
            OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Remove, index, null, new() { removed }));
        }

        public void Clear()
        {
            var old = new List<T>(_list);
            _list.Clear();
            OnChanged?.Invoke();
            OnChangedDetailed?.Invoke(new RxListChange<T>(RxListChangeType.Clear, -1, null, old));
        }

        public bool Contains(T item) => _list.Contains(item);
        public int IndexOf(T item) => _list.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // SubscribeInner: 요소 내부 필드 구독
        public void SubscribeInner(Func<T, IReactiveReader> selector, object subscriber, Action onChanged, RxType type = RxType.InteractLogical)
        {
            UnsubscribeInner();

            foreach (var item in _list)
            {
                var reader = selector(item);
                reader.SubscribeRaw(onChanged, subscriber, type);
                _innerSubscriptions.Add(new InnerSubscription(reader, onChanged));
            }

            OnChangedDetailed += _ =>
            {
                UnsubscribeInner();
                foreach (var item in _list)
                {
                    var reader = selector(item);
                    reader.SubscribeRaw(onChanged, subscriber, type);
                    _innerSubscriptions.Add(new InnerSubscription(reader, onChanged));
                }
            };
        }

        private void UnsubscribeInner()
        {
            foreach (var sub in _innerSubscriptions)
                sub.Dispose();
            _innerSubscriptions.Clear();
        }

        private readonly struct InnerSubscription : IDisposable
        {
            private readonly IReactiveReader _reader;
            private readonly Action _handler;

            public InnerSubscription(IReactiveReader reader, Action handler)
            {
                _reader = reader;
                _handler = handler;
            }

            public void Dispose() => _reader.UnsubscribeRaw(_handler);
        }

        // BindEach 체이닝 지원
        public RxListBinder<T> BindEach(Func<T, IReactiveReader> selector)
        {
            return new RxListBinder<T>(this, selector);
        }

        // RxExpr 통합 계산기 생성 (예: 총 스탯 계산)
        public RxExpr<TResult> ToExpr<TResult>(Func<T, bool> predicate, Func<T, TResult> selector, RxVar<TResult> baseValue, Func<TResult, TResult, TResult> adder)
        {
            return new RxExpr<TResult>(() =>
            {
                var result = baseValue.Value;
                foreach (var item in _list)
                {
                    if (predicate(item))
                        result = adder(result, selector(item));
                }
                return result;
            });
        }
    }

    public class RxListBinder<TItem>
    {
        private readonly RxList<TItem> _source;
        private readonly Func<TItem, IReactiveReader> _selector;

        public RxListBinder(RxList<TItem> source, Func<TItem, IReactiveReader> selector)
        {
            _source = source;
            _selector = selector;
        }

        public void OnChanged(Action onChanged, object subscriber, RxType type = RxType.InteractLogical)
        {
            _source.SubscribeInner(_selector, subscriber, onChanged, type);
        }
    }
}