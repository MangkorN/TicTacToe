using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Behaviors;

namespace TicTacToe.Game
{
    public class SceneCameraController : SceneSingleton<SceneCameraController>
    {
        [Serializable]
        private struct TagToPosition
        {
            [Tooltip("Tag to specify positions.")] public string Tag;
            [Tooltip("A list of camera positions, use Tags to specify positions.")] public Transform Position;
        }

        [Header("Settings")]
        [SerializeField] private float _cameraMoveDuration = 0.5f;

        [Header("Child References")]
        [SerializeField] private Transform _cameraTarget;

        [Header("Scene References")]
        [SerializeField] private TagToPosition[] _positions;
        private Dictionary<string, Transform> _positionsMapping;

        private float _originalCameraMoveDuration;
        private bool _initialized = false;

        protected override void Awake()
        {
            base.Awake();

            _positionsMapping = new();
            foreach (var position in _positions)
                _positionsMapping[position.Tag] = position.Position;

            _originalCameraMoveDuration = _cameraMoveDuration;
            _initialized = true;
        }

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

            StopAllCoroutines();
            StartCoroutine(SmoothMoveAndRotate(_cameraTarget, targetTransform, _cameraMoveDuration));
        }

        public void ResetCameraMoveDuration()
        {
            _cameraMoveDuration = _originalCameraMoveDuration;
        }

        private IEnumerator SmoothMoveAndRotate(Transform fromTransform, Transform toTransform, float duration = 1.0f)
        {
            Vector3 initialPosition = fromTransform.position;
            Quaternion initialRotation = fromTransform.rotation;
            Vector3 targetPosition = toTransform.position;
            Quaternion targetRotation = toTransform.rotation;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                fromTransform.position = Vector3.Lerp(initialPosition, targetPosition, t);
                fromTransform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                yield return null;
            }

            fromTransform.position = targetPosition;
            fromTransform.rotation = targetRotation;
        }
    }
}
