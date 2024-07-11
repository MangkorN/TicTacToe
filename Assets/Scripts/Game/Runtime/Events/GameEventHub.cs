using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    public enum GameMode
    {
        PlayerVsPlayer,
        PlayerVsBot
    }

    [CreateAssetMenu(fileName = "GameEventHub", menuName = "TicTacToe/GameEventHub")]
    public class GameEventHub : EventBroadcaster
    {
        /// <summary>
        /// Called when a board size has been selected.
        /// </summary>
        public event Action<int> OnSelectBoardSize;
        /// <summary>
        /// Called when a game mode has been selected.
        /// </summary>
        public event Action<GameMode> OnSelectGameMode;

        #region Fields & Properties

        [Header("Game Settings"), HorizontalLine]
        [SerializeField] private int _gameBoardSize = 3;
        [SerializeField] private GameMode _gameMode = GameMode.PlayerVsPlayer;

        private bool _lockSettings = false;

        public int GameBoardSize
        {
            get => _gameBoardSize;
            set
            {
                if (_lockSettings)
                {
                    Debug.LogWarning("Attempted to change settings mid-lock!");
                    return;
                }
                _gameBoardSize = value;

                // TODO: refactor
                PreviewGameBoard(value);
                MoveCamera($"Size{value}");

                OnSelectBoardSize?.Invoke(value);
            }
        }
        public GameMode GameMode
        {
            get => _gameMode;
            set
            {
                if (_lockSettings)
                {
                    Debug.LogWarning("Attempted to change settings mid-lock!");
                    return;
                }
                _gameMode = value;
                OnSelectGameMode?.Invoke(value);
            }
        }
        public int GameModeIndex
        {
            get => (int)_gameMode;
            set
            {
                if (_lockSettings)
                {
                    Debug.LogWarning("Attempted to change settings mid-lock!");
                    return;
                }
                if (!Enum.IsDefined(typeof(GameMode), value))
                {
                    Debug.LogWarning($"Invalid GameMode index: {value}. " +
                        $"Value must be between 0 and {Enum.GetValues(typeof(GameMode)).Length - 1}.");
                    return;
                }
                _gameMode = (GameMode)value;
                OnSelectGameMode?.Invoke((GameMode)value);
            }
        }

        #endregion

        public void LockSettings(bool lockSettings)
        {
            _lockSettings = lockSettings;
        }

        [ContextMenu("Reset Game")]
        public void RestartScene()
        {
            ResetManager.ResetGame();
            //string currentSceneName = SceneManager.GetActiveScene().name;
            //SceneManager.LoadScene(currentSceneName);
        }

        #region Event handling (TODO: refactor)

        private void PreviewGameBoard(int size)
        {
            if (GridBoardsController.Instance == null)
            {
                Debug.LogWarning("Scene Camera Controller not found!");
                return;
            }

            GridBoardsController.Instance.SelectBoard(size);
        }

        private void MoveCamera(string tag)
        {
            if (SceneCameraController.Instance == null)
            {
                Debug.LogWarning("Scene Camera Controller not found!");
                return;
            }

            SceneCameraController.Instance.MoveToPosition(tag);
        }

        #endregion
    }
}
