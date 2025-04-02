using UnityEngine;

namespace Akasha
{
    public abstract class Manager<T> : RxContextBehaviour,
        IManager,
        IGlobalLogicalSubscriber,
        IGlobalEventSubscriber,
        IUnfiniteTriggerSubscriber,
        IUnfiniteLocalEventSubscriber
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

                instance.InitIfNeeded();
                return instance!;
            }
        }

        public static bool IsInstance => instance != null;
        protected virtual bool IsPersistent => true;

        public bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            if (instance == null)
            {
                instance = (T)this;

                if (IsPersistent)
                    DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            InitIfNeeded();
        }

        public void InitIfNeeded()
        {
            if (isInitialized) return;

            OnSetup();
            OnInitialize();

            isInitialized = true;
        }

        protected virtual void OnSetup() { }
        protected virtual void OnInitialize() { }
    }
}