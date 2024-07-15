using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Components;

namespace TicTacToe.Game
{
    public class SelectionMarker : MonoBehaviour
    {
        public const float DISTANCE_THRESHOLD = 0.01f;
        public const float MARKER_HEIGHT = 1.7f;
        public const float MARKGRID_HEIGHT = 4.5f;

        [Header("Status")]
        [SerializeField] private char _player = '0';

        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 17.3f;
        [SerializeField] private GameObject _markerPrefab;

        private Vector3 _targetPosition;
        private bool _isPlayerTurn;

        private void Awake()
        {
            SystemLoader.OnSystemUnload += SelfDestruct;

            if (GridBoardsController.Instance == null)
                GridBoardsController.OnInstantiated += () => { RegisterToGridBoardsController(true); };
            else
                RegisterToGridBoardsController(true);
        }

        private void OnDestroy()
        {
            SystemLoader.OnSystemUnload -= SelfDestruct;

            RegisterToGridBoardsController(false);
        }

        private void RegisterToGridBoardsController(bool register)
        {
            if (GridBoardsController.Instance == null)
                return;

            if (register)
            {
                GridBoardsController.Instance.OnPlayerGridMovement += SetTargetPosition;
                GridBoardsController.Instance.OnPlayerMoveSuccess += MarkPosition;
                GridBoardsController.Instance.OnDuelEnd += HandleDuelEnd;
            }
            else
            {
                GridBoardsController.Instance.OnPlayerGridMovement -= SetTargetPosition;
                GridBoardsController.Instance.OnPlayerMoveSuccess -= MarkPosition;
                GridBoardsController.Instance.OnDuelEnd -= HandleDuelEnd;
            }
        }

        private void SelfDestruct()
        {
            Destroy(gameObject);
        }

        public void Initialize(char player, bool hide)
        {
            _player = player;
            transform.position = new Vector3(transform.position.x, MARKER_HEIGHT, transform.position.z);

            GetComponentInChildren<ParticleSystem>().Play();
            gameObject.SetActive(!hide);
        }

        private void Update()
        {
            if (!_isPlayerTurn)
                return;

            // Move towards the grid block
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

            // Check if the object is close enough to the target position
            if (Vector3.Distance(transform.position, _targetPosition) < DISTANCE_THRESHOLD)
            {
                transform.position = _targetPosition;
            }
        }

        private void SetTargetPosition(Vector3 position, char player)
        {
            bool isCurrentPlayer = player == _player;

            gameObject.SetActive(isCurrentPlayer);

            if (isCurrentPlayer)
            {
                _targetPosition = new Vector3(position.x, MARKER_HEIGHT, position.z);
                _isPlayerTurn = true;
            }
        }

        /// <summary>
        /// Called when a player has successfully confirmed a move on the grid.
        /// </summary>
        /// <param name="position">The position of the grid block in the scene.</param>
        /// <param name="player">The player that confirmed the move.</param>
        private void MarkPosition(Vector3 position, char player)
        {
            if (player != _player) // If the last move wasn't from this player...
            {
                gameObject.SetActive(true); // It's now this player's turn to make the next move.
                return;
            }

            // Last move was from this player, therefore mark the last move with this player's marker.
            Instantiate(_markerPrefab, new Vector3(position.x, MARKGRID_HEIGHT, position.z), Quaternion.identity);

            gameObject.SetActive(false); // Deactivate, and wait for the other player's turn.
        }

        private void HandleDuelEnd(bool gameResult, char player)
        {
            Destroy(gameObject);
        }
    }
}
