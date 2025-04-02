using UnityEngine;

namespace Akasha
{
    public abstract class BasePresenter : RxContextBehaviour, IPresenter, IInteractLogicalSubscriber, IUnfiniteTriggerSubscriber, IUnfiniteLocalEventSubscriber, IGlobalEventSubscriber
    {
        protected override void OnInit()
        {
            base.OnInit();
            foreach (var child in GetComponentsInChildren<RxContextBehaviour>())
            {
                child.InjectPresenterContext(this);
            }
        }
    }
}