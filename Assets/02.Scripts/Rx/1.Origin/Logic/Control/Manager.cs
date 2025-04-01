using UnityEngine;

namespace Akasha
{
    public abstract class Manager<T> : RxContextBehaviour, IManager, IGlobalLogicalSubscriber, IGlobalEventSubscriber, IUnfiniteTriggerSubscriber, IUnfiniteLocalEventSubscriber
        where T : Manager<T>
    {
        private static T? instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name);
                        instance = singleton.AddComponent<T>();
                    }
                }
                return instance!;
            }
        }

        public static bool IsInstance => instance != null;
        protected virtual bool IsPersistent => true;

        protected override void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;

                if (IsPersistent)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            OnSetup();
            base.Awake(); 
        }

        protected virtual void OnSetup() { }

    }
}