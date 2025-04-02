using UnityEngine;

namespace Akasha
{
    /// <summary>
    /// Entity와 Interactor를 함께 제어하는 게임 로직 중심 컨트롤러입니다.
    /// 명령을 받아 동작하고 Entity에 명령을 전달합니다.
    /// </summary>
    public abstract class BaseController : RxContextBehaviour { }
    public abstract class BaseController<TEntity> : BaseController, IFiniteLocalEventSubscriber, IGlobalEventSubscriber, IRxStateMachine
        where TEntity : BaseEntity
    {
        [SerializeField] private TEntity? entity;
        [SerializeField] private BaseInteractor? interactor;

        public TEntity? Entity => entity;
        public BaseInteractor? Interactor => interactor;

        protected override void OnInit()
        {
            foreach (var child in GetComponentsInChildren<RxContextBehaviour>())
            {
                child.InjectControllerContext(this);
            }

            if (entity == null) entity = GetComponent<TEntity>();
            if (interactor == null) interactor = GetComponent<BaseInteractor>();

            if (entity != null) OnEntityInjected();
            if (interactor != null) OnInteractorInjected();
        }

        public void InjectEntity(TEntity entity)
        {
            this.entity = entity;
            OnEntityInjected();
        }

        public void InjectInteractor(BaseInteractor interactor)
        {
            this.interactor = interactor;
            OnInteractorInjected();
        }

        protected virtual void OnEntityInjected() { }
        protected virtual void OnInteractorInjected() { }
    }
}