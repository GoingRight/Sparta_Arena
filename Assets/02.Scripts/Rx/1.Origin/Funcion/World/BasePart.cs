using UnityEngine;

namespace Akasha
{
    public abstract class BasePart : RxContextBehaviour, IFiniteFieldSubscriber
    {
        public BaseEntity? Entity { get; private set; }

        internal void SetParent(BaseEntity? entity)
        {
            Entity = entity;

            if (entity != null)
                OnAttachedToEntity(entity);
            else
                OnDetachedFromEntity();
        }

        protected virtual void OnAttachedToEntity(BaseEntity entity) { }

        protected virtual void OnDetachedFromEntity() { }
    }
}