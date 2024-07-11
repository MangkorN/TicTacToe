using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CoreLib.Behaviors;

namespace TicTacToe.Game
{
    public class GridBoardsController : SceneSingleton<GridBoardsController>, PlayerInputSettings.IGridMovementActions
    {
        #region Events

        /// <summary>
        /// Called whenever a player moves around the grid. <br></br>
        /// The <see cref="Vector3"/> parameter represents the global position of the current grid block. <br></br>
        /// The <see cref="char"/> parameter represents the player making the move.
        /// </summary>
        public event Action<Vector3, char> OnPlayerGridMovement;
        /// <summary>
        /// Called when a player move is rejected. <br></br>
        /// The <see cref="Vector3"/> parameter represents the position of the move. <br></br>
        /// The <see cref="char"/> parameter represents the player making the move.
        /// </summary>
        public event Action<Vector3, char> OnPlayerMoveFail;
        /// <summary>
        /// Called when a player move is accepted. <br></br>
        /// The <see cref="Vector3"/> parameter represents the position of the move. <br></br>
        /// The <see cref="char"/> parameter represents the player making the move.
        /// </summary>
        public event Action<Vector3, char> OnPlayerMoveSuccess;
        /// <summary>
        /// Called when a player move is completely over, that is, right when it's the next player's turn. <br></br>
        /// The <see cref="int"/> parameters represents the row and column of the move. <br></br>
        /// The <see cref="char"/> parameter represents the player who made the move.
        /// </summary>
        public event Action<int, int, char> OnPlayerMoveComplete;
        /// <summary>
        /// Called when the game has ended. <br></br>
        /// The <see cref="bool"/> parameter represents a Win (TRUE) or a Draw (FALSE). <br></br>
        /// The <see cref="char"/> parameter represents the player who made the final move.
        /// </summary>
        public event Action<bool, char> OnGameEnd;

        #endregion

        #region Fields & Properties

        /// <summary>
        /// A mapping of grid blocks for different game board sizes. <br></br>
        /// See <see cref="GridBlock.GenerateGridBlockMap"/> to learn about what this type of mapping is used for.
        /// </summary>
        private Dictionary<int, Dictionary<(int, int), GridBlock>> _gridBlockMap;
        /// <summary>
        /// A mapping of grid boards for different game board sizes. <br></br>
        /// See <see cref="GridBlock.GenerateGridBoards"/> to learn about what this type of mapping is used for.
        /// </summary>
        private Dictionary<int, GameObject> _gridBoards;
        /// <summary>
        /// A mapping of player characters to their current marker positions represented by row and column.
        /// </summary>
        private Dictionary<char, (int, int)> _playerMarkerPositions;

        [Header("Settings")]
        [SerializeField] private GameObject _player1MarkerPrefab;
        [SerializeField] private GameObject _player2MarkerPrefab;

        [Header("Scene References")]
        [SerializeField] private WizardController _wizardPlayer1;
        [SerializeField] private WizardController _wizardPlayer2;

        private bool _gameInProgress = false;
        private PlayerInputSettings _inputSettings;

        public bool GameInProgress => _gameInProgress;
        public (int, int) GetPlayerMarkerPosition(char player)
        {
            return _playerMarkerPositions[player];
        }
        public Vector3 GetPlayerMarkerPositionInScene(char player)
        {
            return GridBlock.GetBlockPositionFromMap(
                GameManager.Instance.GetGameSize, _playerMarkerPositions[player], _gridBlockMap);
        }

        #endregion

        #region Initialization & Cleanup

        protected override void Awake()
        {
            base.Awake();

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            if (GameManager.Instance == null)
                GameManager.OnInitialized += () => { RegisterToGameManager(true); };
            else
                RegisterToGameManager(true);

            _inputSettings = new PlayerInputSettings();
            _inputSettings.Enable();
            _inputSettings.GridMovement.SetCallbacks(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RegisterToGameManager(false);

            _inputSettings.Disable();
            _inputSettings.GridMovement.RemoveCallbacks(this);
        }

        private void RegisterToGameManager(bool register)
        {
            if (GameManager.Instance == null)
                return;

            if (register)
            {
                GameManager.Instance.OnGameSessionStart += SetupGameScene;
                GameManager.Instance.OnWin += HandleWin;
                GameManager.Instance.OnDraw += HandleDraw;
                GameManager.Instance.OnPlayerMove += HandlePlayerMove;
            }
            else
            {
                GameManager.Instance.OnGameSessionStart -= SetupGameScene;
                GameManager.Instance.OnWin -= HandleWin;
                GameManager.Instance.OnDraw -= HandleDraw;
                GameManager.Instance.OnPlayerMove -= HandlePlayerMove;
            }
        }

        #endregion

        #region Game Scene Initialization

        public void SelectBoard(int size)
        {
            if (this == null || transform == null) // additional null check for the object itself
            {
                Debug.LogWarning("GridBoardsController has been destroyed. Aborting initialization.");
                return;
            }

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
            if (_gridBoards == null)
                _gridBoards = GridBlock.GenerateGridBoards(transform);
            _gridBoards[size].SetActive(true);
        }

        private void SetupGameScene(int gameSize, char player1, char player2, bool isPlayerVsPlayer)
        {
            // Set up grid
            InitializeGridBoards();
            SelectBoard(gameSize);
            SpawnPlayerMarkers(player1, player2, gameSize);

            // Set up wizards
            _wizardPlayer1.Initialize(player1, gameSize, true);
            _wizardPlayer2.Initialize(player2, gameSize, false);

            _gameInProgress = true;
        }

        private void InitializeGridBoards()
        {

            if (this == null || transform == null) // additional null check for the object itself
            {
                Debug.LogWarning("GridBoardsController has been destroyed. Aborting initialization.");
                return;
            }

            if (!GridBlock.VerifyGridBoardsSetup(transform))
            {
                Debug.LogError("Failed to verify grid boards setup. Aborting initialization.");
                return;
            }

            _gridBlockMap = GridBlock.GenerateGridBlockMap(transform);
            _gridBoards = GridBlock.GenerateGridBoards(transform);
        }

        private void SpawnPlayerMarkers(char player1, char player2, int gameSize)
        {
            _playerMarkerPositions = new()
            {
                [player1] = (0, 0),
                [player2] = (gameSize - 1, gameSize - 1)
            };

            /*------------------------------------------------------------------------------------------------*/

            var obj = Instantiate(_player1MarkerPrefab,
                GridBlock.GetBlockPositionFromMap(GameManager.Instance.GetGameSize,_playerMarkerPositions[player1], _gridBlockMap),
                Quaternion.identity);
            if (!obj.TryGetComponent(out SelectionMarker marker1))
            {
                Debug.LogWarning("Player1 Marker prefab does not have a SelectionMarker component!");
                return;
            }
            marker1.Initialize(player1, GameManager.Instance != null && !GameManager.Instance.IsPlayer1Turn);

            /*------------------------------------------------------------------------------------------------*/

            obj = Instantiate(_player2MarkerPrefab,
                GridBlock.GetBlockPositionFromMap(GameManager.Instance.GetGameSize, _playerMarkerPositions[player2], _gridBlockMap),
                Quaternion.identity);
            if (!obj.TryGetComponent(out SelectionMarker marker2))
            {
                Debug.LogWarning("Player1 Marker prefab does not have a SelectionMarker component!");
                return;
            }
            marker2.Initialize(player2, GameManager.Instance != null && GameManager.Instance.IsPlayer1Turn);
        }

        #endregion

        #region Handle Game Events

        private void HandlePlayerMove(int row, int col, char player)
        {

        }

        private void HandleWin(int row, int col, char player)
        {
            _gameInProgress = false;
            OnGameEnd?.Invoke(true, player);
        }

        private void HandleDraw(int row, int col, char player)
        {
            _gameInProgress = false;
            OnGameEnd?.Invoke(false, player);
        }

        #endregion

        #region Handle Player Input

        public void OnMoveUp(InputAction.CallbackContext context)
        {
            if (!context.performed || GameManager.Instance.IsBotTurn)
                return;
            MovePlayerInGrid((-1, 0));
        }

        public void OnMoveDown(InputAction.CallbackContext context)
        {
            if (!context.performed || GameManager.Instance.IsBotTurn)
                return;
            MovePlayerInGrid((1, 0));
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (!context.performed || GameManager.Instance.IsBotTurn)
                return;
            MovePlayerInGrid((0, -1));
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (!context.performed || GameManager.Instance.IsBotTurn)
                return;
            MovePlayerInGrid((0, 1));
        }

        /// <summary>
        /// Update the current player's position according to the movement that occured.
        /// </summary>
        /// <param name="movement">The movement that occured.</param>
        /// <returns>Whether the grid movement was successful or not.</returns>
        public bool MovePlayerInGrid((int, int) movement)
        {
            if (!_gameInProgress || GameManager.Instance == null)
                return false;

            int gameSize = GameManager.Instance.GetGameSize;
            char player = GameManager.Instance.GetCurrentPlayer;

            int row = _playerMarkerPositions[player].Item1 + movement.Item1;
            int col = _playerMarkerPositions[player].Item2 + movement.Item2;

            if (row < 0 || row >= gameSize || col < 0 || col >= gameSize)
                return false;

            _playerMarkerPositions[player] = (row, col);
            OnPlayerGridMovement?.Invoke(GetPlayerMarkerPositionInScene(player), player);
            return true;
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed || GameManager.Instance.IsBotTurn)
                return;
            ConfirmBlock();
        }

        public void ConfirmBlock()
        {
            if (!_gameInProgress || GameManager.Instance == null)
                return;

            int gameSize = GameManager.Instance.GetGameSize;
            char player = GameManager.Instance.GetCurrentPlayer;
            (int, int) playerPosition = _playerMarkerPositions[player];

            bool moveAccepted = GameManager.Instance.MakeMoveAtBlock(playerPosition.Item1, playerPosition.Item2);

            if (!moveAccepted)
                OnPlayerMoveFail?.Invoke(GetPlayerMarkerPositionInScene(player), player);
            else
            {
                OnPlayerMoveSuccess?.Invoke(GetPlayerMarkerPositionInScene(player), player);
                OnPlayerMoveComplete?.Invoke(playerPosition.Item1, playerPosition.Item2, player);
            }
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Verify GridBoards setup")]
        private void VerifyGridBoardsSetup()
        {
            GridBlock.VerifyGridBoardsSetup(transform, true);
        }
#endif
    }
}
