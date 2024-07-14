using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CoreLib.Behaviors;
using CoreLib.Components;

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
        /// Called when the duel has begun. <br></br>
        /// The <see cref="Vector3"/> parameter represents the global position of the current grid block. <br></br>
        /// The <see cref="char"/> parameter represents the current player.
        /// </summary>
        public event Action<Vector3, char> OnDuelStart;
        /// <summary>
        /// Called when the duel has ended. <br></br>
        /// The <see cref="bool"/> parameter represents a Win (TRUE) or a Draw (FALSE). <br></br>
        /// The <see cref="char"/> parameter represents the player who made the final move.
        /// </summary>
        public event Action<bool, char> OnDuelEnd;

        #endregion

        #region Fields & Properties

        // Private fields --------------------------------------------------------------------------------
        
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
        private PlayerInputSettings _inputSettings;

        private bool _gameInProgress = false;
        private List<GameObject> _playerMarkers;

        // Public properties --------------------------------------------------------------------------------
        
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
        public Dictionary<int, Dictionary<(int, int), GridBlock>> GridBoardMap
        {
            get
            {
                _gridBlockMap ??= GridBlock.GenerateGridBlockMap(transform);
                return _gridBlockMap;
            }
        }
        public Dictionary<int, GameObject> GridBoards
        {
            get
            {
                _gridBoards ??= GridBlock.GenerateGridBoards(transform);
                return _gridBoards;
            }
        }

        #endregion

        #region Initialization & Deinitialization

        protected override void Awake()
        {
            base.Awake();

            if (GameManager.Instance == null)
                GameManager.OnInitialized += () => { RegisterToGameManager(true); };
            else
                RegisterToGameManager(true);

            _inputSettings = new PlayerInputSettings();
            _inputSettings.Enable();
            _inputSettings.GridMovement.SetCallbacks(this);
            SystemLoader.OnSystemUnload += ResetSceneSettings;

            ResetSceneSettings();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RegisterToGameManager(false);

            _inputSettings.Disable();
            _inputSettings.GridMovement.RemoveCallbacks(this);
            SystemLoader.OnSystemUnload -= ResetSceneSettings;
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

        private void ResetSceneSettings()
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            _gridBlockMap = null;
            _gridBoards = null;
            _playerMarkerPositions = null;
            if (_playerMarkers != null && _playerMarkers.Count > 0)
            {
                foreach(var playerMarker in _playerMarkers)
                    Destroy(playerMarker);
            }
            _playerMarkers = null;
        }

        #endregion

        #region Game Scene Initialization

        private void SetupGameScene(int gameSize, char player1, char player2, bool isPlayerVsPlayer)
        {
            // Set up grid
            if (!GridBlock.VerifyGridBoardsSetup(transform))
            {
                Debug.LogError("Failed to verify grid boards setup. Aborting initialization.");
                return;
            }
            SelectBoard(gameSize);
            SpawnPlayerMarkers(player1, player2, gameSize);

            // Set up wizards
            _wizardPlayer1.Initialize(player1, gameSize, true);
            _wizardPlayer2.Initialize(player2, gameSize, false);

            OnDuelStart?.Invoke(GetPlayerMarkerPositionInScene(player1), player1);
            _gameInProgress = true;
        }

        public void SelectBoard(int size)
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
            GridBoards[size].SetActive(true);
        }

        private void SpawnPlayerMarkers(char player1, char player2, int gameSize)
        {
            _playerMarkerPositions = new()
            {
                [player1] = (0, 0),
                [player2] = (gameSize - 1, gameSize - 1)
            };
            _playerMarkers = new();

            /*------------------------------------------------------------------------------------------------*/

            var obj = Instantiate(_player1MarkerPrefab,
                GridBlock.GetBlockPositionFromMap(GameManager.Instance.GetGameSize,_playerMarkerPositions[player1], GridBoardMap),
                Quaternion.identity);
            if (!obj.TryGetComponent(out SelectionMarker marker1))
            {
                Debug.LogWarning("Player1 Marker prefab does not have a SelectionMarker component!");
                return;
            }
            marker1.Initialize(player1, GameManager.Instance != null && !GameManager.Instance.IsPlayer1Turn);
            _playerMarkers.Add(obj);

            /*------------------------------------------------------------------------------------------------*/

            obj = Instantiate(_player2MarkerPrefab,
                GridBlock.GetBlockPositionFromMap(GameManager.Instance.GetGameSize, _playerMarkerPositions[player2], GridBoardMap),
                Quaternion.identity);
            if (!obj.TryGetComponent(out SelectionMarker marker2))
            {
                Debug.LogWarning("Player1 Marker prefab does not have a SelectionMarker component!");
                return;
            }
            marker2.Initialize(player2, GameManager.Instance != null && GameManager.Instance.IsPlayer1Turn);
            _playerMarkers.Add(obj);
        }

        #endregion

        #region Handle Game Events

        private void HandlePlayerMove(int row, int col, char player)
        {

        }

        private void HandleWin(int row, int col, char player)
        {
            _gameInProgress = false;
            OnDuelEnd?.Invoke(true, player);
        }

        private void HandleDraw(int row, int col, char player)
        {
            _gameInProgress = false;
            OnDuelEnd?.Invoke(false, player);
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
