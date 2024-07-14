using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using CoreLib.Behaviors;
using CoreLib.Components;

namespace TicTacToe.Game
{
    public class SceneCameraController : SceneSingleton<SceneCameraController>
    {
        public const float DISTANCE_THRESHOLD = 0.01f;
        public const float CAM_Y_FIXED = 41.6f;
        public const float CAM_Z_ADD = -21.9f;

        #region Fields

        [Header("Settings")]
        [SerializeField] private float _gridMovementSpeed = 17.3f;
        [SerializeField] private float _cameraMoveDuration = 0.27f;
        [SerializeField] private float _camJiggleTargetFOV = 55f;
        [SerializeField] private float _camJiggleDuration = 0.17f;

        [Header("Child References")]
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        [Header("Scene References")]
        [SerializeField] private TagToPosition[] _positions;
        private Dictionary<string, Transform> _positionsMapping;
        private bool _initialized = false;

        // Camera menu movement ----------------------------------------
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private float _originalCameraMoveDuration;
        private Coroutine _camMovementCoroutine;

        // Camera grid movement ----------------------------------------
        private Vector3 _cameraGridTargetPosition;
        private bool _gridIsLive = false;

        // Camera FOV effects ----------------------------------------
        private float _camJiggleOriginalFOV;
        private Coroutine _camJiggleCoroutine;

        #endregion

        #region Initialization & Deinitialization

        protected override void Awake()
        {
            base.Awake();
            SystemLoader.OnSystemUnload += ResetCameraPositionAndRotation;

            if (GridBoardsController.Instance == null)
                GridBoardsController.OnInstantiated += () => { RegisterToGridBoardsController(true); };
            else
                RegisterToGridBoardsController(true);

            _positionsMapping = new();
            foreach (var position in _positions)
                _positionsMapping[position.Tag] = position.Position;

            _originalCameraPosition = _cameraTarget.position;
            _originalCameraRotation = _cameraTarget.rotation;
            _originalCameraMoveDuration = _cameraMoveDuration;

            if (_virtualCamera != null)
                _camJiggleOriginalFOV = _virtualCamera.m_Lens.FieldOfView;

            _initialized = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SystemLoader.OnSystemUnload -= ResetCameraPositionAndRotation;

            RegisterToGridBoardsController(false);
        }

        private void RegisterToGridBoardsController(bool register)
        {
            if (GridBoardsController.Instance == null)
                return;

            if (register)
            {
                GridBoardsController.Instance.OnPlayerGridMovement += HandlePlayerGridMovement;
                GridBoardsController.Instance.OnPlayerMoveSuccess += HandlePlayerMoveSuccess;
                GridBoardsController.Instance.OnDuelStart += HandleDuelStart;
                GridBoardsController.Instance.OnDuelEnd += HandleDuelEnd;
            }
            else
            {
                GridBoardsController.Instance.OnPlayerGridMovement -= HandlePlayerGridMovement;
                GridBoardsController.Instance.OnPlayerMoveSuccess -= HandlePlayerMoveSuccess;
                GridBoardsController.Instance.OnDuelStart -= HandleDuelStart;
                GridBoardsController.Instance.OnDuelEnd -= HandleDuelEnd;
            }
        }

        #endregion

        #region Event Handling

        private void HandleDuelStart(Vector3 position, char player)
        {
            HandlePlayerGridMovement(position, player);
            _gridIsLive = true;
        }

        private void HandleDuelEnd(bool result, char player)
        {
            _gridIsLive = false;
        }

        private void HandlePlayerGridMovement(Vector3 position, char player)
        {
            _cameraGridTargetPosition = new(position.x, CAM_Y_FIXED, position.z + CAM_Z_ADD);
        }

        private void HandlePlayerMoveSuccess(Vector3 position, char player)
        {
            if (GameManager.Instance == null)
                return;

            char otherPlayer = GameManager.Instance.GetOtherPlayer(player);
            Vector3 otherPlayerMarkerPosition = GridBoardsController.Instance.GetPlayerMarkerPositionInScene(otherPlayer);
            HandlePlayerGridMovement(otherPlayerMarkerPosition, otherPlayer);

            JiggleCamFOV();
        }

        #endregion

        #region Gameplay

        public void MoveToPosition(string tag, float duration)
        {
            _cameraMoveDuration = duration;
            MoveToPosition(tag);
        }

        public void MoveToPosition(string tag)
        {
            if (!_initialized)
            {
                Debug.LogError("Not initialized yet!");
                return;
            }

            if (!_positionsMapping.TryGetValue(tag, out Transform targetTransform))
            {
                Debug.LogError($"No position found for tag: {tag}");
                return;
            }

            if (_camMovementCoroutine != null)
                StopCoroutine(_camMovementCoroutine);
            _camMovementCoroutine = StartCoroutine(SmoothMoveAndRotate(targetTransform.position, targetTransform.rotation, _cameraMoveDuration));
        }

        public void ResetCameraPositionAndRotation()
        {
            StartCoroutine(SmoothMoveAndRotate(_originalCameraPosition, _originalCameraRotation, _cameraMoveDuration));
        }

        public void ResetCameraMoveDuration()
        {
            _cameraMoveDuration = _originalCameraMoveDuration;
        }

        private IEnumerator SmoothMoveAndRotate(Vector3 targetPosition, Quaternion targetRotation, float duration = 1.0f)
        {
            Vector3 initialPosition = _cameraTarget.position;
            Quaternion initialRotation = _cameraTarget.rotation;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                _cameraTarget.SetPositionAndRotation(Vector3.Lerp(initialPosition, targetPosition, t), Quaternion.Slerp(initialRotation, targetRotation, t));

                yield return null;
            }

            _cameraTarget.SetPositionAndRotation(targetPosition, targetRotation);
        }

        private void Update()
        {
            if (!_gridIsLive)
                return;

            // Move towards the grid block
            _cameraTarget.position = Vector3.Lerp(_cameraTarget.position, _cameraGridTargetPosition, _gridMovementSpeed * Time.deltaTime);
            if (Vector3.Distance(_cameraTarget.position, _cameraGridTargetPosition) < DISTANCE_THRESHOLD)
                _cameraTarget.position = _cameraGridTargetPosition;
        }

        public void JiggleCamFOV()
        {
            if (_camJiggleCoroutine != null)
                StopCoroutine(_camJiggleCoroutine);
            _camJiggleCoroutine = StartCoroutine(JiggleCamFOVCoroutine());
        }

        private IEnumerator JiggleCamFOVCoroutine()
        {
            // Lerp to target FOV
            float elapsed = 0f;
            while (elapsed < _camJiggleDuration / 2)
            {
                _virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(_camJiggleOriginalFOV, _camJiggleTargetFOV, elapsed / (_camJiggleDuration / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure we reach the exact target FOV
            _virtualCamera.m_Lens.FieldOfView = _camJiggleTargetFOV;

            // Lerp back to original FOV
            elapsed = 0f;
            while (elapsed < _camJiggleDuration)
            {
                _virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(_camJiggleTargetFOV, _camJiggleOriginalFOV, elapsed / (_camJiggleDuration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure we reach the exact original FOV
            _virtualCamera.m_Lens.FieldOfView = _camJiggleOriginalFOV;
        }

        #endregion

        #region Helper types

        [Serializable]
        private struct TagToPosition
        {
            [Tooltip("Tag to specify positions.")] public string Tag;
            [Tooltip("A list of camera positions, use Tags to specify positions.")] public Transform Position;
        }

        #endregion
    }
}
