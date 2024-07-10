using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib.Behaviors
{
    public interface ISystemManager
    {
        IEnumerator Initialize();
        bool IsInitialized { get; }
        string Name { get; }
    }

    public abstract class SystemManager<T> : Singleton<T>, ISystemManager where T : SystemManager<T>
    {
        public static event Action OnInitialized;

        private bool _isInitialized = false;
        
        public bool IsInitialized => _isInitialized;
        public string Name => name;

        public IEnumerator Initialize()
        {
            if (!_isInitialized)
            {
                yield return InitializationRoutine();
            }
            else
            {
                Debug.LogWarning("Attempted to initialize a system manager that has already been initialized.");
            }
        }

        private IEnumerator InitializationRoutine()
        {
            yield return InitializeSystem();
            yield return new WaitForEndOfFrame();
            
            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        protected abstract IEnumerator InitializeSystem();
    }
}
