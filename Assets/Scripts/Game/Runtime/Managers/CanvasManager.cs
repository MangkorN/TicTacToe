using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoreLib.Behaviors;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasManager : SystemManager<CanvasManager>
    {
        [Header("Canvases (Drag to determine sorting order)"), HorizontalLine]
        [SerializeField] private CanvasSettings[] _canvasSettings;

        #region Initialization & Deinitialization

        protected override IEnumerator InitializeSystem()
        {
            if (!CanvasSettingsAreValid())
                Debug.LogWarning("Improper Canvas settings detected!");

            GenerateCanvasSettings();
            ApplyCanvasSettings();
            CanvasController.OnAllAnimationsEnded += RefreshCanvasRaycasterStatuses;

            foreach (var setting in _canvasSettings)
            {
                if (setting.SpawnOnStart)
                    setting.EventHandler.Enter();
            }

            yield break;
        }

        protected override IEnumerator DeinitializeSystem()
        {
            if (!IsInitialized)
                yield break;

            CanvasController.OnAllAnimationsEnded -= RefreshCanvasRaycasterStatuses;

            foreach (var setting in _canvasSettings)
                setting.EventHandler.ExitSceneForce();

            if (CanvasController.NumAnimationsGlobal != 0)
                Debug.LogError($"Total number of canvas animations is {CanvasController.NumAnimationsGlobal}, expected 0!");

            yield break;
        }

        #endregion

        /// <summary>
        /// Turn all active canvas graphic raycasters ON or OFF depending on sort order and individual canvas settings.
        /// </summary>
        private void RefreshCanvasRaycasterStatuses()
        {
            var activeCanvasList = _canvasSettings
                                  .Where(c => c.EventHandler.CanvasController != null && c.EventHandler.CanvasController.IsActive)
                                  .OrderBy(c => c.SortOrder)
                                  .ToList();

            for (int i = 0; i < activeCanvasList.Count; i++)
            {
                // Enable the raycaster of the current canvas.
                activeCanvasList[i].EventHandler.CanvasController.IsGraphicRaycasterEnabled = true;

                // If this canvas should disable raycasters below it, loop through the lower canvases and disable their raycasters.
                if (activeCanvasList[i].DisableRaycastersBeneath)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        // Skip already disabled raycasters.
                        if (!activeCanvasList[j].EventHandler.CanvasController.IsGraphicRaycasterEnabled) continue;

                        // Disable the raycaster.
                        activeCanvasList[j].EventHandler.CanvasController.IsGraphicRaycasterEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Checks through each canvas setting for any misconfigurations.
        /// </summary>
        /// <returns>Whether the canvas settings are valid or not.</returns>
        public bool CanvasSettingsAreValid()
        {
            if (_canvasSettings == null || _canvasSettings.Length == 0) // Check if there are no settings.
                return false;

            HashSet<UIEventHandler> handlers = new();
            foreach (var setting in _canvasSettings)
            {
                if (setting.EventHandler == null) // Check if setting does not contain a UIEventHandler.
                    return false;

                if (!handlers.Add(setting.EventHandler))  // Check if UIEventHandler is a duplicate.
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generate canvas settings based on the provided UIEventHandler.
        /// </summary>
        public void GenerateCanvasSettings()
        {
            if (_canvasSettings == null) return;

            for (int i = 0; i < _canvasSettings.Length; i++)
            {
                if (_canvasSettings[i].EventHandler != null)
                    _canvasSettings[i].Name = _canvasSettings[i].EventHandler.Name;
                else
                    _canvasSettings[i].Name = "MISSING REF";

                if (_canvasSettings[i].Overrides.OverrideSortOrder)
                    _canvasSettings[i].SortOrder = _canvasSettings[i].Overrides.SortOrder;
                else
                    _canvasSettings[i].SortOrder = i;
            }
        }

        /// <summary>
        /// Apply canvas settings to all UIEventHandlers.
        /// </summary>
        private void ApplyCanvasSettings()
        {
            foreach (var setting in _canvasSettings)
            {
                int sortOrder = setting.SortOrder;
                setting.EventHandler.SetSpawnSettings(sortOrder, transform);
            }
        }

        #region Helper Classes

        [Serializable]
        private class CanvasSettings
        {
            [Serializable]
            public class SettingOverrides
            {
                [Tooltip("Enable to override sorting order.")]
                public bool OverrideSortOrder;
                [Tooltip("Active only if OverrideSortOrder is enabled.")]
                public int SortOrder;
            }

            [ReadOnly] public string Name;
            [ReadOnly] public int SortOrder;
            public UIEventHandler EventHandler;

            [Tooltip("Enable to make this canvas spawn on Start.")]
            public bool SpawnOnStart = true;
            [Tooltip("Enable to make this canvas disable all canvas graphic raycasters beneath it (determined by sorting order).")]
            public bool DisableRaycastersBeneath = true;
            [Tooltip("Options to override some automated settings.")]
            public SettingOverrides Overrides;
        }

        #endregion

#if UNITY_EDITOR

        public void SetAllDisableRaycastersBeneath(bool enable)
        {
            if (_canvasSettings == null) return;

            for (int i = 0; i < _canvasSettings.Length; i++)
            {
                _canvasSettings[i].DisableRaycastersBeneath = enable;
            }
        }

        public void ManualRefreshGraphicRaycasters()
        {
            RefreshCanvasRaycasterStatuses();
        }

        [ContextMenu("Manual Initialize")]
        public void ManualInitialize()
        {
            StartCoroutine(Initialize());
        }

#endif
    }
}
