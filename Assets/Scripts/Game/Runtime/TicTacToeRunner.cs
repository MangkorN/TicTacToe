using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TicTacToe.Game
{
    public class TicTacToeRunner
    {
        public const char P1 = 'X';
        public const char P2 = 'O';
        public const string MOVE_INVALID_ERR = "Invalid move.";
        public const string MOVE_POSTGAME_ERR = "Game has already ended. Move not accepted.";

        #region Events
        /// <summary>
        /// Announces the winning move and the winner of the game. <br></br>
        /// e.g. 0,0,X means that player 'X' won by selecting [0,0]
        /// </summary>
        public event Action<int, int, char> OnWin;
        /// <summary>
        /// Announces the move that made a draw and the player who made that move. <br></br>
        /// e.g. 0,0,X means that player 'X' made a draw by selecting [0,0]
        /// </summary>
        public event Action<int, int, char> OnDraw;
        /// <summary>
        /// Announces the row, column, and player whenever a move takes place. <br></br>
        /// e.g. 0,0,X means that player 'X' selected block [0,0]
        /// </summary>
        public event Action<int, int, char> OnPlayerMove;
        #endregion

        #region Private Fields
        /// <summary>
        /// The size of the board. For example, n=3 implies a 3x3 board.
        /// </summary>
        private readonly int _n;
        /// <summary>
        /// The current state of the board during gameplay.
        /// </summary>
        private readonly char[,] _board;
        /// <summary>
        /// A list of all possible winning lines (lines that, when fully occupied by a single player's marks, result in a victory) <br></br>
        /// Each winning line is a list of coordinates representing the blocks in that line. e.g. [0,0],[0,1],[0,2] for a 3x3 board <br></br>
        /// </summary>
        private readonly List<List<(int, int)>> _winningLines;
        /// <summary>
        /// Maps each block to the winning lines it is part of. <br></br>
        /// Each KEY is a block's coordinate (e.g., [0,0]), and the VALUE is a list of winning lines that include that block. <br></br>
        /// For example, the block [0,0] is part of the winning lines [0,0],[0,1],[0,2] and [0,0],[1,0],[2,0] among others.
        /// </summary>
        private readonly Dictionary<(int, int), List<List<(int, int)>>> _blockToLines;
        /// <summary>
        /// Indicates whose turn it is, Player1 or Player2.
        /// </summary>
        private char _currentPlayer;
        /// <summary>
        /// Keeps track of player moves. First entry is always Player1's, second is Player2's, and so on.
        /// </summary>
        private List<(int, int)> _moveHistory;
        #endregion

        #region Properties
        public List<List<(int, int)>> WinningLines => _winningLines;
        public Dictionary<(int, int), List<List<(int, int)>>> BlockToLines => _blockToLines;
        public bool GameOver { get; private set; }
        public char[,] GetCurrentBoard => _board;
        public char GetPlayer1 => P1;
        public char GetPlayer2 => P2;
        public char GetCurrentPlayer => _currentPlayer;
        public List<(int, int)> GetMoveHistory => _moveHistory;
        #endregion

        public TicTacToeRunner(int size)
        {
            if (size < 2)
            {
                throw new ArgumentException("Size cannot be smaller than 2.");
            }

            _n = size;
            _board = new char[_n, _n];
            _winningLines = new List<List<(int, int)>>();
            _blockToLines = new Dictionary<(int, int), List<List<(int, int)>>>();
            _currentPlayer = P1;
            _moveHistory = new();
            GameOver = false;

            GenerateWinningLines();
        }

        private void GenerateWinningLines()
        {
            // Generate rows
            for (int i = 0; i < _n; i++)
            {
                var row = new List<(int, int)>();
                for (int j = 0; j < _n; j++)
                {
                    row.Add((i, j)); // Add block to the current row

                    // Initialize dictionary entry for the block if it doesn't already exist
                    if (!_blockToLines.ContainsKey((i, j)))
                        _blockToLines[(i, j)] = new();

                    // Add a reference to the current row to the dictionary entry for this block
                    _blockToLines[(i, j)].Add(row);
                }
                // Add the current row to the list of all winning lines
                _winningLines.Add(row);
            }

            // Generate columns
            for (int j = 0; j < _n; j++)
            {
                var column = new List<(int, int)>();
                for (int i = 0; i < _n; i++)
                {
                    column.Add((i, j)); // Add block to the current column

                    // Initialize dictionary entry for the block if it doesn't already exist
                    if (!_blockToLines.ContainsKey((i, j)))
                        _blockToLines[(i, j)] = new();

                    // Add a reference to the current column to the dictionary entry for this block
                    _blockToLines[(i, j)].Add(column);
                }
                // Add the current column to the list of all winning lines
                _winningLines.Add(column);
            }

            // Generate main diagonal
            var mainDiagonal = new List<(int, int)>();
            for (int i = 0; i < _n; i++)
            {
                mainDiagonal.Add((i, i)); // Add block to the diagonal

                // Initialize dictionary entry for the block if it doesn't already exist
                if (!_blockToLines.ContainsKey((i, i)))
                    _blockToLines[(i, i)] = new();

                // Add a reference to the diagonal to the dictionary entry for this block
                _blockToLines[(i, i)].Add(mainDiagonal);
            }
            // Add the diagonal to the list of all winning lines
            _winningLines.Add(mainDiagonal);

            // Generate anti-diagonal
            var antiDiagonal = new List<(int, int)>();
            for (int i = 0; i < _n; i++)
            {
                antiDiagonal.Add((i, _n - 1 - i)); // Add block to the anti-diagonal

                // Initialize dictionary entry for the block if it doesn't already exist
                if (!_blockToLines.ContainsKey((i, _n - 1 - i)))
                    _blockToLines[(i, _n - 1 - i)] = new();

                // Add a reference to the anti-diagonal to the dictionary entry for this block
                _blockToLines[(i, _n - 1 - i)].Add(antiDiagonal);
            }
            // Add the anti-diagonal to the list of all winning lines
            _winningLines.Add(antiDiagonal);
        }

        public bool MakeMove(int row, int col)
        {
            if (GameOver)
            {
                Debug.LogError(MOVE_POSTGAME_ERR);
                return false;
            }

            if (row < 0 || row >= _n || col < 0 || col >= _n || _board[row, col] != '\0')
            {
                Debug.LogError(MOVE_INVALID_ERR);
                return false;
            }

            _board[row, col] = _currentPlayer; // Current player makes a move
            _moveHistory.Add((row, col));
            OnPlayerMove?.Invoke(row, col, _currentPlayer);

            // Invalidate lines that can no longer result in a win
            InvalidateLines(row, col);

            // Check if latest move was a winning move
            if (CheckWin(row, col, _currentPlayer))
            {
                GameOver = true;
                OnWin?.Invoke(row, col, _currentPlayer);
                return true;
            }

            // Check for a draw by verifying if there are no more potential winning lines
            if (CheckDraw())
            {
                GameOver = true;
                OnDraw?.Invoke(row, col, _currentPlayer);
                return true;
            }

            // Toggle player
            _currentPlayer = _currentPlayer == P1 ? P2 : P1;
            return true;
        }

        private void InvalidateLines(int row, int col)
        {
            var invalidatedLines = new List<List<(int, int)>>();

            // Iterate through each winning line that the block is part of
            foreach (var line in _blockToLines[(row, col)])
            {
                bool hasP1 = false;
                bool hasP2 = false;

                // Check if the line contains both P1 and P2, making it no longer possible to be a winning line
                foreach (var (r, c) in line)
                {
                    if (_board[r, c] == P1) hasP1 = true;
                    if (_board[r, c] == P2) hasP2 = true;
                    if (hasP1 && hasP2)
                    {
                        invalidatedLines.Add(line); // Line is invalidated
                        break;
                    }
                }
            }

            foreach (var line in invalidatedLines)
            {
                // Remove the invalidated line from the list of winning lines
                _winningLines.Remove(line);

                // Update each block entry by removing the invalidated line
                foreach (var (r, c) in line)
                {
                    _blockToLines[(r, c)].Remove(line);
                }
            }
        }

        /// <summary>
        /// Checks for a win and also outputs the winning line if a win occured.
        /// </summary>
        /// <param name="row">Row of the move.</param>
        /// <param name="col">Column of the move.</param>
        /// <param name="player">Player who made the move.</param>
        /// <param name="winningLine">The winning line. If no win occured, outputs a null.</param>
        /// <returns></returns>
        private bool CheckWin(int row, int col, char player)
        {
            foreach (var line in _blockToLines[(row, col)])
            {
                if (line.TrueForAll(cell => _board[cell.Item1, cell.Item2] == player))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckDraw()
        {
            // A draw occurs if there are no more potential winning lines
            if (_winningLines.Count == 0)
            {
                return true;
            }

            return false;
        }

        #region Utilities

        /// <summary>
        /// View all the winning lines. Tip: For a more consistent log format, go to Console settings -> Use Monoface font
        /// </summary>
        public void PrintWinningLines()
        {
            for (int lineIndex = 0; lineIndex < _winningLines.Count; lineIndex++)
            {
                StringBuilder sb = new();
                sb.AppendLine($"Winning Line (S{_n:D2}.L{lineIndex + 1:D2}):");

                // Create a blank board representation
                string[,] boardRepresentation = new string[_n, _n];
                for (int i = 0; i < _n; i++)
                {
                    for (int j = 0; j < _n; j++)
                    {
                        boardRepresentation[i, j] = "[___]";
                    }
                }

                // Place the winning line coordinates in the board representation
                foreach (var (row, col) in _winningLines[lineIndex])
                {
                    boardRepresentation[row, col] = $"[{row},{col}]";
                }

                //// Build the string representation of the board
                //for (int i = _n - 1; i >= 0; i--) // Start from the top row
                //{
                //    for (int j = 0; j < _n; j++)
                //    {
                //        sb.Append(boardRepresentation[i, j]);
                //        //if (j < _n - 1)
                //        //    sb.Append(" ");
                //    }
                //    sb.AppendLine();
                //}

                // Build the string representation of the board
                for (int i = 0; i < _n; i++)
                {
                    for (int j = 0; j < _n; j++)
                    {
                        sb.Append(boardRepresentation[i, j]);
                    }
                    sb.AppendLine();
                }

                Debug.Log(sb.ToString());
            }
        }

        /// <summary>
        /// View the block to lines mapping. Tip: For a more consistent log format, go to Console settings -> Use Monoface font
        /// </summary>
        public void PrintBlockToLines()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Block to Lines Mapping for size {_n}:");
            sb.AppendLine();
            foreach (var key in _blockToLines.Keys)
            {
                int count = 0;
                sb.Append($"Block [{key.Item1},{key.Item2}]:   ");
                foreach (var line in _blockToLines[key])
                {
                    sb.Append($"{++count}. ");
                    foreach (var (row, col) in line)
                    {
                        sb.Append($"[{row},{col}]");
                    }
                    sb.Append("   ");
                }
                sb.AppendLine();
                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// View the current board. Tip: For a more consistent log format, go to Console settings -> Use Monoface font
        /// </summary>
        public void PrintCurrentBoard()
        {
            Debug.Log(GetStylizedBoard(_board));
        }

        /// <summary>
        /// Returns a formatted 2D array that is human-readable and ready to be printed onto the console.
        /// </summary>
        public static string GetStylizedBoard(char[,] board, bool coordinatesOnly = false)
        {
            StringBuilder sb = new();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == '\0')
                        sb.Append($"[{(coordinatesOnly ? "___" : "_")}]");
                    else
                        sb.Append($"[{(coordinatesOnly ? $"{i},{j}" : $"{board[i, j]}")}]");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the current move history.
        /// </summary>
        public void PrintMoveHistory()
        {
            StringBuilder sb = new();
            for (int i = 0; i < _moveHistory.Count; i++)
            {
                sb.AppendLine($"[{_moveHistory[i].Item1},{_moveHistory[i].Item2}]");
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Converts a string containing a list of moves (like the ones generated from <see cref="PrintMoveHistory()"/>)
        /// into a list of player moves represented as tuples. This is useful for reconstructing player matches.
        /// </summary>
        /// <param name="input">A string containing the moves, with each move on a new line in the format [row,column].</param>
        /// <returns>A list of tuples, where each tuple represents a move as [row,column].</returns>
        public static List<(int, int)> ParseTuples(string input)
        {
            // Split the input string by newlines
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<(int, int)> tuples = new List<(int, int)>();

            foreach (string line in lines)
            {
                // Remove the square brackets
                string trimmedLine = line.Trim(new[] { '[', ']' });

                // Split the coordinates by comma
                string[] parts = trimmedLine.Split(',');

                if (parts.Length == 2)
                {
                    // Parse the coordinates as integers
                    if (int.TryParse(parts[0], out int item1) && int.TryParse(parts[1], out int item2))
                    {
                        // Add the tuple to the list
                        tuples.Add((item1, item2));
                    }
                }
            }

            return tuples;
        }

        #endregion
    }
}
