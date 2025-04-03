using System;

namespace Akasha
{
    // ----- 공통 이벤트 베이스 ----- 

    public abstract class RxEventBase : IRxEvent, IRxSubscribable<Unit>
    {
        protected readonly RxSubscription _subscription = new();

        public void Raise()
        {
            RxQueue.Enqueue(() =>
            {
                this.WithContext(() => _subscription.NotifyAll());
            }, this);
        }

        public abstract void Subscribe(Action subscriber, object context, RxType relationType);

        public void Unsubscribe(Action subscriber) => _subscription.Remove(subscriber);

        public void Unsubscribe(Action subscriber, object context)
        {
            _subscription.Remove(_ => subscriber(), context);
        }

        public IDisposable Bind(Action subscriber, object context)
        {
            return RxEventDisposable.Create(this, subscriber, context);
        }

        // 공통 인터페이스 구현
        void IRxSubscribable<Unit>.SubscribeLaw(Action<Unit> subscriber, object context, RxType relationType)
        {
            if (relationType != RxType.Logical)
                throw new InvalidOperationException("[RxEvent] 이벤트는 Logical 구독만 허용됩니다.");

            _subscription.Add(_ => subscriber(Unit.Default), context, relationType);
        }

        void IRxSubscribable<Unit>.UnsubscribeLaw(Action<Unit> subscriber)
        {
            _subscription.Remove(_ => subscriber(Unit.Default));
        }
    }

    // ----- RxTrigger (구독자만 구분) -----
    public class RxTrigger : RxEventBase, IUnfiniteTriggerSubscriber, IFiniteTriggerSubscriber
    {
        public override void Subscribe(Action subscriber, object context, RxType relationType)
        {
            if (relationType != RxType.Logical)
                throw new InvalidOperationException("[RxTrigger] Trigger는 Logical 구독만 허용됩니다.");

            RxValidator.ValidateEventSubscriber(context, this);
            _subscription.Add(subscriber, context, relationType);
        }
    }

    // ----- RxLocalEvent (소유자, 구독자 인터페이스 추가) -----
    public class RxLocalEvent : RxEventBase, ILocalEventOwner, IFiniteLocalEventSubscriber, IUnfiniteLocalEventSubscriber
    {
        public override void Subscribe(Action subscriber, object context, RxType relationType)
        {
            if (relationType != RxType.Logical)
                throw new InvalidOperationException("[RxLocalEvent] LocalEvent는 Logical 구독만 허용됩니다.");

            RxValidator.ValidateEventSubscriber(context, this);
            _subscription.Add(subscriber, context, relationType);
        }
    }

    // ----- RxGlobalEvent (소유자, 구독자 인터페이스 추가) -----
    public class RxGlobalEvent : RxEventBase, IGlobalEventOwner, IGlobalEventSubscriber
    {
        public override void Subscribe(Action subscriber, object context, RxType relationType)
        {
            if (relationType != RxType.Logical)
                throw new InvalidOperationException("[RxGlobalEvent] GlobalEvent는 Logical 구독만 허용됩니다.");

            RxValidator.ValidateEventSubscriber(context, this);
            _subscription.Add(subscriber, context, relationType);
        }
    }
}