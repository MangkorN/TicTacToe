using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Components;

namespace TicTacToe.Game
{
    public class WizardController : MonoBehaviour
    {
        #region Fields & Properties

        [Header("Status")]
        [SerializeField] private char _player = '0';
        [SerializeField] private bool _isCastingSpell = false;

        [Header("Position Settings")]
        [Tooltip("A list of positions to place the wizard at. Position is chosen depending on game size.")]
        [SerializeField] private GameSizeToPosition[] _positionSettings;

        [Header("Look Rotation Settings")]
        [SerializeField] private float _lookSpeed = 7.3f;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField, Range(2, 5)] private int _testGameSize;
#endif

        private Animator _animator;
        private Vector3 _lookAtPosition;

        public bool IsCastingSpell
        {
            get => _isCastingSpell;
            set
            {
                _isCastingSpell = value;
                if (_animator != null)
                    _animator.SetBool("isCastingSpell", _isCastingSpell);
            }
        }

        #endregion

        #region Initialization & Deinitialization

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            gameObject.SetActive(false);

            if (GridBoardsController.Instance == null)
                GridBoardsController.OnInstantiated += () => { RegisterToGridBoardsController(true); };
            else
                RegisterToGridBoardsController(true);

            SystemLoader.OnSystemUnload += HideWizard;
        }

        private void OnDestroy()
        {
            RegisterToGridBoardsController(false);

            SystemLoader.OnSystemUnload -= HideWizard;
        }

        private void RegisterToGridBoardsController(bool register)
        {
            if (GridBoardsController.Instance == null)
                return;

            if (register)
            {
                GridBoardsController.Instance.OnPlayerGridMovement += SetLookAtPosition;
                GridBoardsController.Instance.OnPlayerMoveSuccess += UpdateSpellAnimations;
                GridBoardsController.Instance.OnDuelEnd += HandleDuelEnd;
            }
            else
            {
                GridBoardsController.Instance.OnPlayerGridMovement -= SetLookAtPosition;
                GridBoardsController.Instance.OnPlayerMoveSuccess -= UpdateSpellAnimations;
                GridBoardsController.Instance.OnDuelEnd -= HandleDuelEnd;
            }
        }

        public void Initialize(char player, int gameSize, bool isPlayerTurn)
        {
            if (!FindAndMoveToPosition(gameSize))
                Debug.LogWarning("No appropriate position found! Could not place wizard.");

            _player = player;
            gameObject.SetActive(true);
            
            IsCastingSpell = isPlayerTurn;
            if (isPlayerTurn)
                _lookAtPosition = GridBoardsController.Instance.GetPlayerMarkerPositionInScene(_player);
            else
                _lookAtPosition = GridBoardsController.Instance.transform.position;
        }

        private bool FindAndMoveToPosition(int gameSize)
        {
            bool wizardPlaced = false;
            foreach (var setting in _positionSettings)
            {
                if (setting.GameSize == gameSize)
                {
                    transform.position = setting.Position.position;
                    wizardPlaced = true;
                }
            }
            return wizardPlaced;
        }

        private void HideWizard()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Gameplay

        private void Update()
        {
            var targetRotation = Quaternion.LookRotation(_lookAtPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _lookSpeed * Time.deltaTime);
        }

        private void SetLookAtPosition(Vector3 position, char player)
        {
            _lookAtPosition = position;
        }

        /// <summary>
        /// Called when a player has successfully confirmed a move on the grid.
        /// </summary>
        /// <param name="position">The position of the grid block in the scene.</param>
        /// <param name="player">The player that confirmed the move.</param>
        private void UpdateSpellAnimations(Vector3 position, char player)
        {
            if (GridBoardsController.Instance.GameInProgress)
                IsCastingSpell = player != _player;

            if (player == _player)
                _animator.SetTrigger("Shoot");

            _lookAtPosition = GridBoardsController.Instance.GetPlayerMarkerPositionInScene(_player);
        }

        private void HandleDuelEnd(bool gameResult, char player)
        {
            IsCastingSpell = false;
            _lookAtPosition = GridBoardsController.Instance.transform.position;
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Test Game Size")]
        private void TestGameSize()
        {
            if (!FindAndMoveToPosition(_testGameSize))
                Debug.LogWarning("No appropriate position found! Could not place wizard.");
        }
#endif

        #region Helper types

        [Serializable]
        private struct GameSizeToPosition
        {
            [Tooltip("The game size required to use the position.")] public int GameSize;
            [Tooltip("The position to use when the game size requirement is met.")] public Transform Position;
        }

        #endregion
    }
}
