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

        public void EnterFromLeft()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterSceneFromLeft();
        }

        public void EnterFromRight()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterSceneFromRight();
        }

        public void EnterFromTop()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterSceneFromTop();
        }

        public void EnterFromBottom()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterSceneFromBottom();
        }

        public void EnterExpand()
        {
            if (!EnsureCanvasIsReadyInPlayMode())
                return;

            CanvasController.EnterSceneExpand();
        }

        #endregion

        #region ExitScene Animation Overrides

        public void ExitToLeft() => StartExitToLeft(false);
        public void ExitToLeftAndDespawn() => StartExitToLeft(true);
        private void StartExitToLeft(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitSceneToLeft(destroy);
        }

        public void ExitToRight() => StartExitToRight(false);
        public void ExitToRightAndDespawn() => StartExitToRight(true);
        private void StartExitToRight(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitSceneToRight(destroy);
        }

        public void ExitToTop() => StartExitToTop(false);
        public void ExitToTopAndDespawn() => StartExitToTop(true);
        private void StartExitToTop(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitSceneToTop(destroy);
        }

        public void ExitToBottom() => StartExitToBottom(false);
        public void ExitToBottomAndDespawn() => StartExitToBottom(true);
        private void StartExitToBottom(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitSceneToBottom(destroy);
        }

        public void ExitShrink() => StartExitShrink(false);
        public void ExitShrinkAndDespawn() => StartExitShrink(true);
        private void StartExitShrink(bool destroy)
        {
            if (!CanvasIsSpawnedInPlayMode())
                return;

            CanvasController.ExitSceneShrink(destroy);
        }

        #endregion
    }
}
