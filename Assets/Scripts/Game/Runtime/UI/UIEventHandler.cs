using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    [CreateAssetMenu(fileName = "UIEventHandler", menuName = "TicTacToe/UIEventHandler")]
    public class UIEventHandler : EventBroadcaster
    {
        #region Fields & Properties

        [Header("UI References"), HorizontalLine]
        [SerializeField] private GameObject _canvasPrefab;

        // Spawn Settings
        private int _canvasSortOrder = 0;
        private Transform _canvasRoot;

        // Spawn Result & Validation
        public GameObject SpawnedCanvas { get; private set; }
        public CanvasController CanvasController { get; private set; } // Component of spawned canvas.
        public bool HasSpawnedCanvas => SpawnedCanvas != null && CanvasController != null;

        #endregion

        #region Canvas Initialization/Deinitialization

        /// <summary>
        /// Must be executed before spawning the canvas.
        /// </summary>
        public void SetSpawnSettings(int sortOrder, Transform root)
        {
            _canvasSortOrder = sortOrder;
            _canvasRoot = root;
        }

        private void InstantiateCanvasAndInitialize()
        {
            SpawnedCanvas = Instantiate(_canvasPrefab, _canvasRoot);

            if (!SpawnedCanvas.TryGetComponent<CanvasController>(out var canvasHandler))
            {
                Debug.LogError("Created object does not contain a CanvasController!");
                return;
            }
            CanvasController = canvasHandler;

            if (SpawnedCanvas.TryGetComponent<Canvas>(out var canvas))
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = _canvasSortOrder;
            }
        }

        #endregion

        #region Main features

        /// <summary>
        /// Make canvas enter the scene. Works only if canvas has been OR can be spawned.
        /// </summary>
        public void Enter()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterScene();
        }

        /// <summary>
        /// Make canvas exit the scene. Works only if canvas has been spawned.
        /// </summary>
        public void Exit() => StartExit(false);

        /// <summary>
        /// Make  exit scene and destroy it afterwards. Works only if canvas has been spawned.
        /// </summary>
        public void ExitAndDespawn() => StartExit(true);

        private void StartExit(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitScene(destroy);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Ensures the canvas is ready. If the canvas has not been spawned, attempts to spawn it.
        /// </summary>
        /// <returns>Whether the canvas is ready or not. If not in PlayMode, returns FALSE always.</returns>
        private bool EnsureCanvasIsReadyInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"UIEventHandler '{Name}': Must be in PlayMode!");
                return false;
            }

            if (SpawnedCanvas != null)
                return true;

            if (_canvasRoot != null && _canvasPrefab != null)
            {
                InstantiateCanvasAndInitialize();
                return true;
            }
            else
            {
                Debug.LogError($"UIEventHandler '{Name}': Settings not configured correctly, cannot spawn Canvas!");
                return false;
            }
        }

        /// <summary>
        /// Checks whether the canvas has been spawned
        /// </summary>
        /// <returns>Whether the canvas has been spawned. If not in PlayMode, returns FALSE always.</returns>
        private bool CanvasIsSpawnedInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"UIEventHandler '{Name}': Must be in PlayMode!");
                return false;
            }

            return SpawnedCanvas != null;
        }

        #endregion

        #region EnterScene Animation Overrides

        public void EnterFromLeft() => OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption.FromLeft);
        public void EnterFromRight() => OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption.FromRight);
        public void EnterFromTop() => OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption.FromTop);
        public void EnterFromBottom() => OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption.FromBottom);
        public void EnterExpand() => OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption.ShrinkExpand);

        private void OverrideBeforeStartEnter(CanvasController.AnimatorOverrideOption overrideOption)
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.OverrideBeforeEnterOrExit(true, overrideOption);
        }

        #endregion

        #region ExitScene Animation Overrides

        public void ExitToLeft() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromLeft, false);
        public void ExitToLeftAndDespawn() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromLeft, true);
        public void ExitToRight() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromRight, false);
        public void ExitToRightAndDespawn() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromRight, true);
        public void ExitToTop() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromTop, false);
        public void ExitToTopAndDespawn() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromTop, true);
        public void ExitToBottom() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromBottom, false);
        public void ExitToBottomAndDespawn() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.FromBottom, true);
        public void ExitShrink() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.ShrinkExpand, false);
        public void ExitShrinkAndDespawn() => OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption.ShrinkExpand, true);

        private void OverrideBeforeStartExit(CanvasController.AnimatorOverrideOption overrideOption, bool markForDestroy = false)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.OverrideBeforeEnterOrExit(false, overrideOption, markForDestroy);
        }

        #endregion
    }
}
