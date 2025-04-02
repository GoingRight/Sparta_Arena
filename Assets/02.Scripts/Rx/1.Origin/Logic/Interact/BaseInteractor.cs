using UnityEngine;

namespace Akasha
{
    public abstract class BaseInteractor : RxContextBehaviour
    {

    }

    public abstract class BaseInteractor<TEntity> : BaseInteractor, IInteractor, IInteractLogicalSubscriber, IFiniteTriggerSubscriber, IFiniteLocalEventSubscriber, IGlobalEventSubscriber
        where TEntity : BaseEntity
    {
        [SerializeField, Tooltip("이 Interactor가 제어할 Entity")]
        private TEntity? entity;

        public TEntity? Entity => entity;

        public void InjectEntity(TEntity entity)
        {
            this.entity = entity;
            OnEntityInjected();
        }

        protected override void OnInit()
        {
            base.OnInit();
            if (entity == null)
                entity = GetComponent<TEntity>();
        }

        protected virtual void OnEntityInjected() { }
    }
}