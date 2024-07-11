using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CoreLib.Components;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class CanvasController : MonoBehaviour
    {
        /// <summary>
        /// Broadcasts when all canvas animations have ended (after at least one animation has started).
        /// </summary>
        public static event Action OnAllAnimationsEnded;

        #region Fields & Properties

        // Private static fields --------------------------------------------------------------------
        /// <summary>
        /// Number of current requests to disable canvas controls.
        /// </summary>
        private static int _numDisableRequests = 0;
        /// <summary>
        ///  Number of active canvas animations.
        /// </summary>
        private static int _numAnimationsGlobal = 0;

        // Public static properties --------------------------------------------------------------------
        /// <summary>
        /// Number of current requests to disable canvas controls.
        /// </summary>
        public static int NumDisableRequests => _numDisableRequests;
        /// <summary>
        ///  Number of active canvas animations.
        /// </summary>
        public static int NumAnimationsGlobal => _numAnimationsGlobal;
        /// <summary>
        /// Indicates if the canvas is disabled. The canvas remains disabled as long as there is at least one disable request.
        /// </summary>
        public static bool IsDisabled => _numDisableRequests > 0;
        /// <summary>
        /// Indicates if any canvas is currently running animations.
        /// </summary>
        public static bool HasActiveAnimations => _numAnimationsGlobal > 0;
#if UNITY_EDITOR
        public static int HighestAnimationsCountedPerTransition { get; private set; }
#endif

        // Private fields --------------------------------------------------------------------
        [Header("Status")]
        [SerializeField] private bool _isActive = false;
        [SerializeField, Tooltip("Number of local active animations.")] private int _numAnimations;

        [Header("Events"), HorizontalLine]
        [SerializeField] private CanvasEvents _canvasEvents;

        [Header("Animation Overrides"), HorizontalLine]
        [SerializeField] private AnimatorOverrides _animatorOverrides;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = false;

        private AnimatorPair[] _animatorPairs;
        private GraphicRaycaster _graphicRaycaster;
        private bool _markForDestroy = false;
        private bool _isEnteringScene;
        private bool _isExitingScene;

        // Public Properties --------------------------------------------------------------------
        public bool IsActive => _isActive;
        /// <summary>
        /// Gets or sets a value indicating whether the GraphicRaycaster is enabled.
        /// Setting this property will invoke the OnGraphicRaycasterEnabledChanged event.
        /// </summary>
        public bool IsGraphicRaycasterEnabled
        {
            get
            {
                return _graphicRaycaster.enabled;
            }
            set
            {
                _graphicRaycaster.enabled = value;
                _canvasEvents.OnGraphicRaycasterEnabledChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// Indicates if the canvas is busy due to being disabled or performing an enter/exit sequence.
        /// </summary>
        public bool IsBusy => _isEnteringScene || _isExitingScene || IsDisabled;

        #endregion

        #region Initialization/Deinitialization

        private void Awake()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();

            // Find all AnimationEventHandler components in children
            var animHandlers = GetComponentsInChildren<AnimationEventHandler>();

            // Create pairs with corresponding RectTransform components
            _animatorPairs = animHandlers.Select(handler => new AnimatorPair
            {
                AnimationEventHandler = handler,
                RectTransform = handler.GetComponent<RectTransform>()
            }).ToArray();

            // Register to all animation events
            foreach (var animPair in _animatorPairs)
            {
                animPair.AnimationEventHandler.OnAnimationStart += HandleAnimationStart;
                animPair.AnimationEventHandler.OnAnimationEnd += HandleAnimationEnd;
            }
        }

        private void OnDestroy()
        {
            // Deregister from all animation events
            foreach (var animPair in _animatorPairs)
            {
                animPair.AnimationEventHandler.OnAnimationStart -= HandleAnimationStart;
                animPair.AnimationEventHandler.OnAnimationEnd -= HandleAnimationEnd;
            }
        }

        #endregion

        #region Main Features

        public void EnterScene()
        {
            if (IsBusy)
            {
                if (_showDebugLog)
                    Debug.Log($"EnterScene(): Entering={_isEnteringScene}, Exiting={_isExitingScene}, Disabled={IsDisabled}");
                return;
            }

            _isEnteringScene = true;
            _isExitingScene = false;

            gameObject.SetActive(true);
            PlayAnimations("Enter");
        }

        public void ExitScene(bool destroy = false)
        {
            if (IsBusy)
            {
                if (_showDebugLog)
                    Debug.Log($"ExitScene({destroy}): Entering={_isEnteringScene}, Exiting={_isExitingScene}, Disabled={IsDisabled}");
                return;
            }

            _isEnteringScene = false;
            _isExitingScene = true;
            _markForDestroy = destroy;

            if (!gameObject.activeInHierarchy) // Skip animation if inactive.
            {
                if (destroy)
                    Destroy(gameObject);
                _isExitingScene = false; // Reset flag.
                return;
            }
            PlayAnimations("Exit");
        }

        /// <summary>
        /// Overrides the canvas animator before entering or exiting the scene.
        /// </summary>
        /// <param name="isEnter">TRUE if canvas should enter the scene, FALSE if canvas should exit the scene.</param>
        /// <param name="overrideOption">Select new animator to override the current canvas animator.</param>
        /// <param name="markForDestroy">If canvas will exit the scene, TRUE if canvas should be destroyed afterwards, FALSE otherwise.</param>
        public void OverrideBeforeEnterOrExit(bool isEnter, AnimatorOverrideOption overrideOption, bool markForDestroy = false)
        {
            switch (overrideOption)
            {
                case AnimatorOverrideOption.FromLeft:
                    OverrideAnimator(_animatorOverrides.LeftToCenterCenterToLeft, AnimationEventHandler.AnimatorType.UIElements);
                    break;
                case AnimatorOverrideOption.FromRight:
                    OverrideAnimator(_animatorOverrides.RightToCenterCenterToRight, AnimationEventHandler.AnimatorType.UIElements);
                    break;
                case AnimatorOverrideOption.FromTop:
                    OverrideAnimator(_animatorOverrides.TopToCenterCenterToTop, AnimationEventHandler.AnimatorType.UIElements);
                    break;
                case AnimatorOverrideOption.FromBottom:
                    OverrideAnimator(_animatorOverrides.BottomToCenterCenterToBottom, AnimationEventHandler.AnimatorType.UIElements);
                    break;
                case AnimatorOverrideOption.ShrinkExpand:
                    OverrideAnimator(_animatorOverrides.ShrinkToExpandExpandToShrink, AnimationEventHandler.AnimatorType.UIElements);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overrideOption), overrideOption, null);
            }

            if (isEnter)
                EnterScene();
            else
                ExitScene(markForDestroy);
        }

        /// <summary>
        /// Makes a request to disable all canvases (canvases won't start enter/exit animation sequences).
        /// </summary>
        public static void DisableAll()
        {
            CounterStateUtilities.ProcessCounterState(ref _numDisableRequests, true, HandleDisableAll, () => Debug.LogError("Error trying to DisableAll"));
        }

        /// <summary>
        /// Makes a request to enable all canvases (canvases will be able to start enter/exit animation sequences,
        /// assuming that there are no other requests to disable the canvas).
        /// </summary>
        public static void EnableAll()
        {
            CounterStateUtilities.ProcessCounterState(ref _numDisableRequests, false, HandleEnableAll, () => Debug.LogError("Error trying to EnableAll"));
        }

        #endregion

        #region Animation Event Handling

        private void HandleAnimationStart()
        {
            CounterStateUtilities.ProcessCounterState(ref _numAnimations, true, AnimationsStarted, AnimationError);
            CounterStateUtilities.ProcessCounterState(ref _numAnimationsGlobal, true, AnimationsStartedGlobal, AnimationErrorGlobal);
#if UNITY_EDITOR
            if (_numAnimationsGlobal > HighestAnimationsCountedPerTransition)
                HighestAnimationsCountedPerTransition = _numAnimationsGlobal;
#endif
        }

        private void HandleAnimationEnd()
        {
            CounterStateUtilities.ProcessCounterState(ref _numAnimations, false, AnimationsEnded, AnimationError);
            CounterStateUtilities.ProcessCounterState(ref _numAnimationsGlobal, false, AnimationsEndedGlobal, AnimationErrorGlobal);
        }

        private void AnimationsStarted()
        {
            if (_isEnteringScene)
            {
                _isActive = true;
                _canvasEvents.OnStartEnterScene?.Invoke();
            }
            else if (_isExitingScene)
            {
                _isActive = false;
                _canvasEvents.OnStartExitScene?.Invoke();
            }
            else
            {
                Debug.LogError("Unexpected state!");
            }
        }

        private void AnimationsEnded()
        {
            if (_isEnteringScene)
            {
                _isEnteringScene = false;
                _canvasEvents.OnEnterScene?.Invoke();
            }
            else if (_isExitingScene)
            {
                if (_markForDestroy) Destroy(gameObject);
                else gameObject.SetActive(false);

                _isExitingScene = false;
                _canvasEvents.OnExitScene?.Invoke();
            }
            else
            {
                Debug.LogError("Unexpected state!");
            }
        }

        private void AnimationError()
        {
            string error = $"Unexpected animation count! Num animations: {_numAnimations}";
            Debug.LogError(error);
        }

        private void AnimationsStartedGlobal() { }

        private void AnimationsEndedGlobal() => OnAllAnimationsEnded?.Invoke();

        private void AnimationErrorGlobal()
        {
            string error = $"Unexpected global animation count! Num animations: {_numAnimationsGlobal}";
            Debug.LogError(error);
        }

        #endregion

        #region Helpers

        private void PlayAnimations(string name)
        {
            foreach (var animPair in _animatorPairs)
            {
                ResetRectTransform(animPair);
                animPair.AnimationEventHandler.SetAnimatorTrigger(name);
            }
        }

        private void ResetRectTransform(AnimatorPair animPair)
        {
            animPair.RectTransform.anchorMax = Vector2.one;
            animPair.RectTransform.anchorMin = Vector2.zero;
            animPair.RectTransform.anchoredPosition = Vector2.zero;
            animPair.RectTransform.sizeDelta = Vector2.zero;
            animPair.RectTransform.localScale = Vector3.one;
            animPair.RectTransform.localRotation = Quaternion.identity;
        }

        private void StopAnimations()
        {
            foreach (var animPair in _animatorPairs)
            {
                animPair.AnimationEventHandler.StopAnimator();
                ResetRectTransform(animPair);
            }
        }

        private void OverrideAnimator(RuntimeAnimatorController newAnimator, AnimationEventHandler.AnimatorType targetAnimatorHandlerType)
        {
            foreach (var animPair in _animatorPairs)
            {
                if (animPair.AnimationEventHandler.Type == targetAnimatorHandlerType)
                    animPair.AnimationEventHandler.ReplaceAnimator(newAnimator);
            }
        }

        private static void HandleDisableAll() { }

        private static void HandleEnableAll() { }

        #endregion

        #region Helper Enums & Classes

        public enum AnimatorOverrideOption
        {
            FromLeft,
            FromRight,
            FromTop,
            FromBottom,
            ShrinkExpand
        }

        [Serializable]
        private class AnimatorOverrides
        {
            public RuntimeAnimatorController LeftToCenterCenterToLeft;
            public RuntimeAnimatorController RightToCenterCenterToRight;
            public RuntimeAnimatorController TopToCenterCenterToTop;
            public RuntimeAnimatorController BottomToCenterCenterToBottom;
            public RuntimeAnimatorController ShrinkToExpandExpandToShrink;
        }

        [Serializable]
        private class CanvasEvents
        {
            public UnityEvent OnStartEnterScene;
            public UnityEvent OnStartExitScene;
            public UnityEvent OnEnterScene;
            public UnityEvent OnExitScene;
            public UnityEvent<bool> OnGraphicRaycasterEnabledChanged;
        }

        /// <summary>
        /// A class that pairs an AnimationEventHandler with a RectTransform.
        /// This class is used to group components that are expected to be on the same GameObject.
        /// </summary>
        private class AnimatorPair
        {
            public AnimationEventHandler AnimationEventHandler;
            public RectTransform RectTransform;
        }

        #endregion
    }
}