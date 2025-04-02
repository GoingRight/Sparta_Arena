using UnityEngine;

namespace Akasha
{
    public abstract class BaseScreen : RxContextBehaviour, IScreen, IInteractLogicalSubscriber, IFiniteLocalEventSubscriber
    {
        private BasePresenter? _presenter;

        public BasePresenter? Presenter => _presenter;

        protected override void OnInit()
        {
            if (_presenter == null)
                _presenter = GetComponent<BasePresenter>();
        }
    }
}