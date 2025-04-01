using UnityEngine;

namespace Akasha
{
    public abstract class BaseView : RxContextBehaviour, IInteractLogicalSubscriber, IFiniteLocalEventSubscriber
    {
        protected override void OnInit()
        {
            BindUI();
        }

        protected abstract void BindUI();
        protected virtual void UnbindUI() { }
        public abstract void RefreshUI();
        protected override void OnDispose()
        {
            base.OnDispose();
            UnbindUI();
        }
    }
}