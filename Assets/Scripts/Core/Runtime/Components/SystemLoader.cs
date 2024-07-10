using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Behaviors;

namespace CoreLib.Components
{
    public class SystemLoader : Singleton<SystemLoader>
    {
        [Header("Systems")]
        [SerializeField] private List<GameObject> _systemManagerObjects;
        private readonly List<ISystemManager> _systemManagers = new();

        [Header("Settings")]
        [SerializeField] private bool _showDebugLogs;

        protected override void Awake()
        {
            base.Awake();

            // Retrieve system manager component from all the system manager prefabs referenced
            foreach (var go in _systemManagerObjects)
            {
                if (go.TryGetComponent<ISystemManager>(out var systemManager))
                {
                    _systemManagers.Add(systemManager);
                }
                else
                {
                    Debug.LogWarning($"GameObject {go.name} does not have a component implementing ISystemManager");
                }
            }
        }

        private IEnumerator Start()
        {
            DebugLog("Begin loading systems...");

            foreach (var manager in _systemManagers)
            {
                yield return manager.Initialize();

                if (manager.IsInitialized)
                    DebugLog($"> {manager.Name} initialized.");
                else
                    DebugLog($"> {manager.Name} failed to initialize.");
            }

            DebugLog("... all systems loaded.");
        }

        private void DebugLog(string log)
        {
            if (_showDebugLogs)
                Debug.Log(log);
        }
    }
}
