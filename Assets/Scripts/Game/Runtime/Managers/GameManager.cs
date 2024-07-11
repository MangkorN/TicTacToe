using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Behaviors;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    public class GameManager : SystemManager<GameManager>
    {
        #region Events
        /// <summary>
        /// Gets called when a game session starts. <br></br>
        /// The <see cref="int"/> parameter represents the size of the game board. <br></br>
        /// The <see cref="char"/> parameters represents the players 1 and 2, respectively. <br></br>
        /// The <see cref="bool"/> parameter represents the game mode, TRUE if Player vs. Player, FALSE if Player vs. Bot.
        /// </summary>
        public event Action<int, char, char, bool> OnGameSessionStart;
        /// <summary>
        /// Forwards the event invocations from <see cref="TicTacToeRunner.OnWin"/>.
        /// </summary>
        public event Action<int, int, char> OnWin;
        /// <summary>
        /// Forwards the event invocations from <see cref="TicTacToeRunner.OnDraw"/>.
        /// </summary>
        public event Action<int, int, char> OnDraw;
        /// <summary>
        /// Forwards the event invocations from <see cref="TicTacToeRunner.OnPlayerMove"/>.
        /// </summary>
        public event Action<int, int, char> OnPlayerMove;
        #endregion

        #region Fields & Properties

        private static char _mostRecentWinner = '\0';
        public static char MostRecentWinner => _mostRecentWinner;

        [Header("References"), HorizontalLine]
        [SerializeField] private GameEventHub _gameEventHub;
        private TicTacToeRunner _gameSession;

        public char GetCurrentPlayer => _gameSession.GetCurrentPlayer;
        public bool IsPlayer1Turn => _gameSession.GetCurrentPlayer == _gameSession.GetPlayer1;
        public bool IsBotTurn => (_gameEventHub.GameMode == GameMode.PlayerVsBot) && (_gameSession.GetCurrentPlayer == _gameSession.GetPlayer2);
        public int GetGameSize => _gameSession.GetCurrentBoard.GetLength(0);
        public List<List<(int, int)>> GetWinningLines => _gameSession.WinningLines;

        #endregion

        protected override IEnumerator InitializeSystem()
        {
            _gameEventHub.AddListener("StartGame", StartGameSession);
            yield break;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsInitialized)
                return;

            _gameEventHub.RemoveListener("StartGame", StartGameSession);
        }

        private void StartGameSession()
        {
            if (_gameEventHub == null)
            {
                Debug.LogError("No GameEventHub referenced! Cannot start game session.");
                return;
            }

            try
            {
                _gameSession = new TicTacToeRunner(_gameEventHub.GameBoardSize);

                _gameSession.OnWin += OnWin;
                _gameSession.OnDraw += OnDraw;
                _gameSession.OnPlayerMove += OnPlayerMove;

                _gameSession.OnWin += HandleGameWin;
                _gameSession.OnDraw += HandleGameDraw;

                OnGameSessionStart?.Invoke(
                    _gameEventHub.GameBoardSize,
                    _gameSession.GetPlayer1,
                    _gameSession.GetPlayer2,
                    _gameEventHub.GameMode == GameMode.PlayerVsPlayer);
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"Failed to create TicTacToeRunner: {ex.Message}");
            }
        }

        private void HandleGameWin(int row, int col, char player)
        {
            _mostRecentWinner = player;
            HandleGameEnd(row, col, player);
        }
        private void HandleGameDraw(int row, int col, char player)
        {
            _mostRecentWinner = '\0';
            HandleGameEnd(row, col, player);
        }

        private void HandleGameEnd(int row, int col, char player)
        {
            _gameSession.PrintCurrentBoard();
            _gameSession.PrintMoveHistory();

            _gameEventHub.InvokeEventByTag("EndGame");
            //CleanupGameSession();
        }

        private void RestartGame()
        {
            //if (_gameSession == null)
            //    return;

            //_gameSession.OnWin -= OnWin;
            //_gameSession.OnDraw -= OnDraw;
            //_gameSession.OnPlayerMove -= OnPlayerMove;
            //_gameSession = null;
        }

        /// <summary>
        /// Select a grid block in the board to make a move.
        /// </summary>
        /// <param name="row">Row of the block.</param>
        /// <param name="col">Column of the block.</param>
        /// <returns>TRUE if the move was accepted, FALSE otherwise.</returns>
        public bool MakeMoveAtBlock(int row, int col)
        {
            if (_gameSession == null)
            {
                Debug.LogWarning("There is no game session currently running.");
                return false;
            }

            return _gameSession.MakeMove(row, col);
        }
    }
}
