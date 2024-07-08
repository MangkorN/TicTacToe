using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using TicTacToe.Game;

namespace TicTacToe.Tools
{
    [CreateAssetMenu(fileName = "TicTacToeManualTester", menuName = "TicTacToe/TicTacToeManualTester")]
    public class TicTacToeManualTester : ScriptableObject
    {
        [Serializable]
        public class MoveList
        {
            [Multiline(20)]
            [SerializeField]
            [Tooltip("A list of moves to execute.")]
            public string Input;
        }

        [Header("Settings")]
        [SerializeField] private int _size = 3;
        [SerializeField] private char _player1Character = 'A';
        [SerializeField] private char _player2Character = 'B';

        [Header("Automation")]
        [SerializeField] private MoveList _moveList;

        private TicTacToeRunner _game;
        private char[,] _board = new char[0, 0];
        private (char, char) _p1;
        private (char, char) _p2;
        private string _gameStatus = "";

        public bool GameInProgress { get; private set; }
        public string GetGameStatus => _gameStatus;
        public int GetBoardSize
        {
            get
            {
                if (_board == null)
                    return 0;
                return _board.GetLength(0);
            }
        }
        public char[,] LocalBoard => _board;
        public char[,] SourceBoard
        {
            get
            {
                if (_game == null)
                    return null;
                return _game.GetCurrentBoard;
            }
        }

        public void StartGame()
        {
            try
            {
                _game = new TicTacToeRunner(_size);
                _board = new char[_size, _size];
                _p1 = (_game.GetPlayer1, _player1Character);
                _p2 = (_game.GetPlayer2, _player2Character);
                _gameStatus = $"It is now {_p1.Item2}'s turn...";
                RegisterToEvents(true);

                GameInProgress = true;
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"Failed to create TicTacToeRunner: {ex.Message}");
            }
        }

        public void ResetSettings()
        {
            _size = 3;
            _player1Character = 'A';
            _player2Character = 'B';
        }

        public void MakeMove(int row, int col)
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }
            _game.MakeMove(row, col);
        }

        public void EndGame()
        {
            _game = null;
            _board = new char[0, 0];
            _p1 = ('\0', '\0');
            _p2 = ('\0', '\0');
            _gameStatus = "";
            RegisterToEvents(false);

            GameInProgress = false;
        }

        private void UpdateBoard(int row, int col, char player)
        {
            _board[row, col] = player == _game.GetPlayer1 ? _p1.Item2 : _p2.Item2;
            _gameStatus = $"It is now {(player == _game.GetPlayer1 ? _p2.Item2 : _p1.Item2)}'s turn...";
        }

        private void HandleWin(int row, int col, char player)
        {
            string log = $"{(player == _game.GetPlayer1 ? _p1.Item2 : _p2.Item2)} is the winner!"
                + $" The winning move was [{row},{col}]";
            _gameStatus = $"{log}";
            Debug.Log(log);
        }

        private void HandleDraw(int row, int col, char player)
        {
            string log = $"{(player == _game.GetPlayer1 ? _p1.Item2 : _p2.Item2)} made a draw!"
                + $" The final move was [{row},{col}]";
            _gameStatus = log;
            Debug.Log(log);
        }

        private void RegisterToEvents(bool register)
        {
            if (_game == null)
                return;

            if (register)
            {
                _game.OnPlayerMove += UpdateBoard;
                _game.OnWin += HandleWin;
                _game.OnDraw += HandleDraw;
            }
            else
            {
                _game.OnPlayerMove -= UpdateBoard;
                _game.OnWin -= HandleWin;
                _game.OnDraw -= HandleDraw;
            }
        }

        public void PrintLocalBoard(bool coordinatesOnly = false)
        {
            if (_board == null)
            {
                Debug.LogError("Local board not initialized yet!");
                return;
            }
            Debug.Log(TicTacToeRunner.GetStylizedBoard(_board, coordinatesOnly));
        }

        public void PrintSourceBoard(bool coordinatesOnly = false)
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }
            Debug.Log(TicTacToeRunner.GetStylizedBoard(_game.GetCurrentBoard, coordinatesOnly));
        }

        public void PrintSourceWinningLines()
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }
            _game.PrintWinningLines();
        }

        public void PrintSourceBlockToLines()
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }
            _game.PrintBlockToLines();
        }

        public void PrintMoveHistory()
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }
            _game.PrintMoveHistory();
        }

        public void ExecuteMoveList()
        {
            if (_game == null)
            {
                Debug.LogError("Game not initialized yet!");
                return;
            }

            var moveList = TicTacToeRunner.ParseTuples(_moveList.Input);
            for (int i = 0; i < moveList.Count; i++)
            {
                MakeMove(moveList[i].Item1, moveList[i].Item2);
            }
        }
    }

    [CustomEditor(typeof(TicTacToeManualTester))]
    public class TicTacToeManualTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TicTacToeManualTester tester = (TicTacToeManualTester)target;

            GUILayout.Space(10f);

            if (tester.GameInProgress)
            {
                EditorGUILayout.LabelField($"STATUS: {tester.GetGameStatus}", EditorStyles.boldLabel);

                GUILayout.Space(5f);//////////////////////////////////////////////////

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("LOCAL BOARD", EditorStyles.boldLabel);
                if (tester.LocalBoard != null)
                    DrawBoard(tester.LocalBoard, tester);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5f);//////////////////////////////////////////////////

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("SOURCE BOARD", EditorStyles.boldLabel);
                if (tester.SourceBoard != null)
                    DrawBoard(tester.SourceBoard, tester);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5f);//////////////////////////////////////////////////

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("COORDINATES BOARD", EditorStyles.boldLabel);
                if (tester.LocalBoard != null)
                    DrawBoardButtons(tester.GetBoardSize, tester);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5f);//////////////////////////////////////////////////

                EditorGUILayout.BeginVertical("box");
                if (GUILayout.Button("PRINT LOCAL BOARD"))
                    tester.PrintLocalBoard();
                if (GUILayout.Button("PRINT SOURCE BOARD"))
                    tester.PrintSourceBoard();
                if (GUILayout.Button("PRINT LOCAL BOARD (Coordinates)"))
                    tester.PrintLocalBoard(true);
                if (GUILayout.Button("PRINT SOURCE BOARD (Coordinates)"))
                    tester.PrintSourceBoard(true);
                if (GUILayout.Button("PRINT SOURCE WINNING LINES"))
                    tester.PrintSourceWinningLines();
                if (GUILayout.Button("PRINT SOURCE BLOCK TO LINES"))
                    tester.PrintSourceBlockToLines();
                if (GUILayout.Button("PRINT MOVE HISTORY"))
                    tester.PrintMoveHistory();
                if (GUILayout.Button("EXECUTE MOVE LIST (Warning: Moves aren't always compatible!)"))
                    tester.ExecuteMoveList();
                if (GUILayout.Button("END GAME"))
                    tester.EndGame();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField($"STATUS: Game hasn't started.", EditorStyles.boldLabel);

                GUILayout.Space(5f);//////////////////////////////////////////////////

                EditorGUILayout.BeginVertical("box");
                if (GUILayout.Button("START GAME"))
                    tester.StartGame();
                if (GUILayout.Button("RESET"))
                    tester.ResetSettings();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBoard(char[,] board, TicTacToeManualTester tester)
        {
            int size1 = board.GetLength(0);
            for (int i = 0; i < size1; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < size1; j++)
                {
                    if (GUILayout.Button($"{(board[i, j] == '\0' ? "_" : board[i, j])}", GUILayout.Width(50), GUILayout.Height(50)))
                        OnButtonClick(i, j, tester);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBoardButtons(int size, TicTacToeManualTester tester)
        {
            for (int i = 0; i < size; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < size; j++)
                {
                    if (GUILayout.Button($"{i},{j}", GUILayout.Width(50), GUILayout.Height(50)))
                        OnButtonClick(i, j, tester);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void OnButtonClick(int i, int j, TicTacToeManualTester tester)
        {
            tester.MakeMove(i, j);
        }
    }
}
