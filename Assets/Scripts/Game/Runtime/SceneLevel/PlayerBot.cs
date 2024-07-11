using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TicTacToe.Game
{
    public enum BotDifficulty
    {
        Easy,
        Average
    }

    public class PlayerBot : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private bool _activeInGame = false;
        [SerializeField] private char _player = '0';
        [SerializeField] private BotDifficulty botDifficulty = BotDifficulty.Easy;

        private HashSet<(int, int)> _moveSet;

        #region Initialization & Cleanup

        private void Awake()
        {
            if (GameManager.Instance == null)
                GameManager.OnInitialized += () => { RegisterToGameManager(true); };
            else
                RegisterToGameManager(true);
        }

        private void Start()
        {
            if (GridBoardsController.Instance != null)
                GridBoardsController.Instance.OnPlayerMoveComplete += HandlePlayerMoveAndRespond;
        }

        private void OnDestroy()
        {
            RegisterToGameManager(false);
            if (GridBoardsController.Instance != null)
                GridBoardsController.Instance.OnPlayerMoveComplete -= HandlePlayerMoveAndRespond;
        }

        private void RegisterToGameManager(bool register)
        {
            if (GameManager.Instance == null)
                return;

            if (register)
            {
                GameManager.Instance.OnGameSessionStart += InitializeBot;
                GameManager.Instance.OnWin += HandleWin;
                GameManager.Instance.OnDraw += HandleDraw;
            }
            else
            {
                GameManager.Instance.OnGameSessionStart -= InitializeBot;
                GameManager.Instance.OnWin -= HandleWin;
                GameManager.Instance.OnDraw -= HandleDraw;
            }
        }

        #endregion

        #region Gameplay

        private void InitializeBot(int gameSize, char player1, char player2, bool isPlayerVsPlayer)
        {
            _activeInGame = !isPlayerVsPlayer;

            if (!_activeInGame)
                return;

            _moveSet = GenerateMoveSet(gameSize);
            _player = player2; // Bot is always player 2
        }

        private void HandlePlayerMoveAndRespond(int row, int col, char player)
        {
            if (!_activeInGame)
                return;

            // Update list of available moves
            if (!_moveSet.Remove((row, col))) 
            {
                Debug.LogError("Error removing latest move from moveset!");
                return;
            }

            if (player == _player)
                return; // No need to respond to your own move

            if (!GridBoardsController.Instance.GameInProgress)
                return;

            // Select winning line randomly
            var winningLines = GameManager.Instance.GetWinningLines;
            if (winningLines == null || winningLines.Count == 0)
            {
                Debug.LogError("Winning Lines are unavailable but game is still running!");
                return;
            }
            int randIndex = Random.Range(0, winningLines.Count);
            var winningLine = winningLines[randIndex];

            // Select empty block to make a move on
            bool foundEmptyBlock = false;
            (int, int) lastBlock = new();

            foreach (var block in winningLine)
            {
                lastBlock = block;
                if (_moveSet.Contains(block))
                {
                    foundEmptyBlock = true;
                    break;
                }
            }
            if (!foundEmptyBlock)
            {
                Debug.LogError($"Did not find any empty blocks in the winning line {GetStylizedBlockLine(winningLine)}! " +
                    $"The last block to be checked was [{lastBlock.Item1},{lastBlock.Item2}].");
                return;
            }

            StartCoroutine(SimulatePlayerMove(lastBlock));
        }

        private IEnumerator SimulatePlayerMove((int, int) targetBlock)
        {
            if (GridBoardsController.Instance == null)
                yield break;

            var markerPosition = GridBoardsController.Instance.GetPlayerMarkerPosition(_player);

            int moveRow = targetBlock.Item1 - markerPosition.Item1;
            int moveCol = targetBlock.Item2 - markerPosition.Item2;

            bool rNeg = moveRow < 0; 
            bool cNeg = moveCol < 0;

            int newMoveRowSize = rNeg ? (moveRow *= -1) : moveRow;
            int newMoveColSize = cNeg ? (moveCol *= -1) : moveCol;

            for (int i = 0; i < newMoveRowSize; i++)
            {
                if (!GridBoardsController.Instance.MovePlayerInGrid(rNeg ? (-1, 0) : (1, 0)))
                    Debug.LogError($"Bot failed a grid movement at index({i})!");
                yield return new WaitForSeconds(Random.Range(0.3f, 1.5f));
            }

            for (int i = 0; i < newMoveColSize; i++)
            {
                if (!GridBoardsController.Instance.MovePlayerInGrid(cNeg ? (0, -1) : (0, 1)))
                    Debug.LogError($"Bot failed a grid movement at index({i})!");
                yield return new WaitForSeconds(Random.Range(0.3f, 1.5f));
            }

            // Confirm that we arrived at target
            var result = GridBoardsController.Instance.GetPlayerMarkerPosition(_player);
            if (result != targetBlock)
            {
                Debug.LogError($"Did not reach target block {Blockf(targetBlock)} " +
                    $"when applying {moveRow} (row) and {moveCol} (col) from {Blockf(markerPosition)} ! " +
                    $"Instead the current marker position is at {Blockf(result)} .");
                yield break;
            }

            GridBoardsController.Instance.ConfirmBlock(); // Finalize player move
            yield break;
        }

        private void HandleWin(int row, int col, char player)
        {
            //
        }

        private void HandleDraw(int row, int col, char player)
        {
            //
        }

        #endregion

        private static string Blockf((int, int) block)
        {
            return $"[{block.Item1},{block.Item2}]";
        }

        /// <summary>
        /// Generates a set of all possible player moves in a game of size N.
        /// </summary>
        /// <param name="n">Size of the game.</param>
        /// <returns>All possible player moves.</returns>
        public static HashSet<(int, int)> GenerateMoveSet(int n)
        {
            HashSet<(int, int)> moves = new();

            // Generate all possible moves
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    moves.Add((i, j));
                }
            }

            return moves;
        }

        public static string GetStylizedBlockLine(List<(int, int)> blockLine)
        {
            StringBuilder sb = new();

            foreach (var block in blockLine)
                sb.Append($"[{block.Item1},{block.Item2}]");

            return sb.ToString();
        }
    }
}
