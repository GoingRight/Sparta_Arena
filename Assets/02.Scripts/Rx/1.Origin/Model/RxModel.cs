using System;
using Akasha;

namespace Akasha
{
    public abstract class RxModel : IRxModel, IFunctionalSubscriber
    {
        public object? ReactiveOwner { get; private set; }

        public void SetReactiveOwner(object owner)
        {
            ReactiveOwner = owner;
        }
    }
}