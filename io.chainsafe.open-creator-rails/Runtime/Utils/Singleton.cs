using System;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        static T _instance;

        private bool _initialized = false;
        
        public static T Instance
        {
            get
            {
                // Sometimes OnEnable calls before Awake on two different scripts,
                // fetching this property will set the singleton instance.
                // So T.Instance can be called safely.
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

                    if (_instance != null) _instance._initialized = true;
                }
                
                return _instance;
            }
        }

        [SerializeField] private bool _dontDestroyOnLoad;

        protected virtual void Awake()
        {
            if (!_initialized)
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("There is more than one Singleton Instance of " + _instance.name);
                }
                
                _instance = (T)this;
                
                _initialized = true;
            }

            if (_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}