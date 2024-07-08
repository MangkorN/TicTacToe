using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using TicTacToe.Game;

namespace TicTacToe.UnitTest
{
    public class TicTacToeTests
    {
        private TicTacToeRunner _ticTacToeRunner;
        private char _p1;
        private char _p2;

        [SetUp]
        public void SetUp()
        {
            ResetFields();
        }

        [TearDown]
        public void TearDown()
        {
            ResetFields();
        }

        private void ResetFields()
        {
            _ticTacToeRunner = null;
            _p1 = '\0';
            _p2 = '\0';
        }

        [Test]
        public void WinningLinesHasCorrectAmount()
        {
            // Test for different board sizes
            for (int size = 2; size <= 10; size++)
            {
                _ticTacToeRunner = new TicTacToeRunner(size);

                // Expected number of winning lines: n rows + n columns + 2 diagonals
                int expectedWinningLinesCount = 2 * size + 2;

                Assert.AreEqual(expectedWinningLinesCount, _ticTacToeRunner.WinningLines.Count, $"Failed for size {size}");
            }
        }

        [Test]
        public void BlockToLinesHasCorrectAmount()
        {
            // Test for different board sizes
            for (int size = 2; size <= 10; size++)
            {
                _ticTacToeRunner = new TicTacToeRunner(size);

                foreach (var block in _ticTacToeRunner.BlockToLines)
                {
                    int lineCount = block.Value.Count;

                    // Each block must have at least two winning lines (one row and one column)
                    Assert.GreaterOrEqual(lineCount, 2, $"Block ({block.Key.Item1}, {block.Key.Item2}) failed for size {size}");

                    // Maximum winning lines count depends on whether size is odd or even
                    int expectedMax = size % 2 == 0 ? 3 : 4;
                    Assert.LessOrEqual(lineCount, expectedMax, $"Block ({block.Key.Item1}, {block.Key.Item2}) failed for size {size}");

                    // Additionally, if the block is on a diagonal, it will be part of the corresponding diagonal line(s)
                    bool onMainDiagonal = block.Key.Item1 == block.Key.Item2;
                    bool onAntiDiagonal = block.Key.Item1 + block.Key.Item2 == size - 1;
                    if (onMainDiagonal && onAntiDiagonal)
                    {
                        // Block is part of both diagonals
                        Assert.AreEqual(lineCount, 4, $"Block ({block.Key.Item1}, {block.Key.Item2}) failed for size {size}");
                    }
                    else if (onMainDiagonal || onAntiDiagonal)
                    {
                        // Block is part of one diagonal
                        Assert.AreEqual(lineCount, 3, $"Block ({block.Key.Item1}, {block.Key.Item2}) failed for size {size}");
                    }
                }
            }
        }

        [Test]
        public void PrintWinningLines()
        {
            for (int size = 2; size <= 10; size++)
            {
                Assert.DoesNotThrow(() =>
                {
                    _ticTacToeRunner = new TicTacToeRunner(size);
                    _ticTacToeRunner.PrintWinningLines();
                }, $"PrintWinningLines failed for size {size}");
            }
        }

        [Test]
        public void PrintBlockToLines()
        {
            for (int size = 2; size <= 10; size++)
            {
                Assert.DoesNotThrow(() =>
                {
                    _ticTacToeRunner = new TicTacToeRunner(size);
                    _ticTacToeRunner.PrintBlockToLines();
                }, $"PrintBlockToLines failed for size {size}");
            }
        }

        [Test]
        public void MakeMove_ValidMoves_AreCorrectlyRecorded()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);
            _p1 = _ticTacToeRunner.GetPlayer1;
            _p2 = _ticTacToeRunner.GetPlayer2;

            Assert.IsTrue(_ticTacToeRunner.MakeMove(0, 0));
            Assert.AreEqual(_p1, _ticTacToeRunner.GetCurrentBoard[0, 0]);

            Assert.IsTrue(_ticTacToeRunner.MakeMove(0, 1));
            Assert.AreEqual(_p2, _ticTacToeRunner.GetCurrentBoard[0, 1]);

            Assert.IsTrue(_ticTacToeRunner.MakeMove(1, 0));
            Assert.AreEqual(_p1, _ticTacToeRunner.GetCurrentBoard[1, 0]);

            Assert.IsTrue(_ticTacToeRunner.MakeMove(1, 1));
            Assert.AreEqual(_p2, _ticTacToeRunner.GetCurrentBoard[1, 1]);
        }

        [Test]
        public void MakeMove_InvalidMoves_AreRejected()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);

            Assert.IsTrue(_ticTacToeRunner.MakeMove(0, 0));

            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_INVALID_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(0, 0)); // Attempt to place in the same spot

            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_INVALID_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(-1, 0)); // Out of bounds

            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_INVALID_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(3, 0)); // Out of bounds

            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_INVALID_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(0, -1)); // Out of bounds

            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_INVALID_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(0, 3)); // Out of bounds
        }

        [Test]
        public void MakeMove_WinningMove_TriggersWinEvent()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);

            bool winEventTriggered = false;
            _ticTacToeRunner.OnWin += (row, col, player) => { winEventTriggered = true; };

            _ticTacToeRunner.MakeMove(0, 0); // X
            _ticTacToeRunner.MakeMove(1, 0); // O
            _ticTacToeRunner.MakeMove(0, 1); // X
            _ticTacToeRunner.MakeMove(1, 1); // O
            _ticTacToeRunner.MakeMove(0, 2); // X wins

            Assert.IsTrue(winEventTriggered);
        }

        [Test]
        public void MakeMove_DrawMove_TriggersDrawEvent()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);
            bool drawEventTriggered = false;
            _ticTacToeRunner.OnDraw += (row, col, player) => { drawEventTriggered = true; };

            // Simulate moves resulting in a draw
            _ticTacToeRunner.MakeMove(0, 0); // X
            _ticTacToeRunner.MakeMove(0, 1); // O
            _ticTacToeRunner.MakeMove(0, 2); // X
            _ticTacToeRunner.MakeMove(1, 1); // O
            _ticTacToeRunner.MakeMove(1, 0); // X
            _ticTacToeRunner.MakeMove(1, 2); // O
            _ticTacToeRunner.MakeMove(2, 1); // X
            _ticTacToeRunner.MakeMove(2, 0); // O, Early Draw
            //_ticTacToeRunner.MakeMove(2, 2); // X, Complete Draw

            Assert.IsTrue(drawEventTriggered);
        }

        [Test]
        public void OnPlayerMove_EventIsTriggered()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);
            bool playerMoveEventTriggered = false;
            _ticTacToeRunner.OnPlayerMove += (row, col, player) => { playerMoveEventTriggered = true; };

            _ticTacToeRunner.MakeMove(0, 0); // X

            Assert.IsTrue(playerMoveEventTriggered);
        }

        [Test]
        public void GameOver_PreventsFurtherMoves()
        {
            _ticTacToeRunner = new TicTacToeRunner(3);
            _ticTacToeRunner.OnWin += (row, col, player) => { /* Do nothing */ };
            
            // X wins
            _ticTacToeRunner.MakeMove(0, 0); // X
            _ticTacToeRunner.MakeMove(1, 0); // O
            _ticTacToeRunner.MakeMove(0, 1); // X
            _ticTacToeRunner.MakeMove(1, 1); // O
            _ticTacToeRunner.MakeMove(0, 2); // X wins

            // Attempt further moves
            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_POSTGAME_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(2, 2)); // Should not be allowed
            LogAssert.Expect(LogType.Error, TicTacToeRunner.MOVE_POSTGAME_ERR);
            Assert.IsFalse(_ticTacToeRunner.MakeMove(2, 1)); // Should not be allowed
        }

        [Test]
        public void BruteForce_RandomMovesAndVerifyAtTheEnd()
        {
            for (int size = 2; size < 10; size++)
            {
                _ticTacToeRunner = new TicTacToeRunner(size);
                _p1 = _ticTacToeRunner.GetPlayer1;
                _p2 = _ticTacToeRunner.GetPlayer2;

                (int, int) lastMove;

                (int, int) wMove = new();
                List<(int, int)> wLine = null;
                bool isDraw = false;

                _ticTacToeRunner.OnWin += (row, col, player) => { wMove = (row, col); };
                _ticTacToeRunner.OnDraw += (row, col, player) => { isDraw = true; };

                var moves = GenerateAndShuffleBlocks(size);
                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    lastMove = moves[i];
                    moves.RemoveAt(i);

                    if (!_ticTacToeRunner.MakeMove(lastMove.Item1, lastMove.Item2))
                    {
                        Debug.LogError($"Last move [{lastMove.Item1},{lastMove.Item2}] was invalid!");
                        _ticTacToeRunner.PrintCurrentBoard();
                        _ticTacToeRunner.PrintMoveHistory();
                        Assert.Fail("Unexpected failure, check console logs.");
                    }

                    if (_ticTacToeRunner.GameOver)
                    {
                        if (isDraw)
                        {
                            Debug.Log($"Claim is Draw (size {size}):\n\n"
                                + $"{TicTacToeRunner.GetStylizedBoard(_ticTacToeRunner.GetCurrentBoard)}");
                            Assert.IsTrue(BoardIsDraw(_ticTacToeRunner.GetCurrentBoard));
                        }
                        else
                        {
                            Debug.Log($"Claim is Win (size {size}):\n\n"
                                + $"{TicTacToeRunner.GetStylizedBoard(_ticTacToeRunner.GetCurrentBoard)}");
                            Assert.IsTrue(BoardHasWin(_ticTacToeRunner.GetCurrentBoard, wMove));
                        }
                        break;
                    }
                }
            }
        }

        public static List<(int, int)> GenerateAndShuffleBlocks(int n)
        {
            List<(int, int)> blocks = new();

            // Generate all possible blocks
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    blocks.Add((i, j));
                }
            }

            // Shuffle the list of blocks
            System.Random rng = new();
            int count = blocks.Count;
            while (count > 1)
            {
                count--;
                int k = rng.Next(count + 1);
                (int, int) value = blocks[k];
                blocks[k] = blocks[count];
                blocks[count] = value;
            }

            return blocks;
        }

        public static string GetStylizedLine(char[] line)
        {
            return string.Join("", line.Select(c => c == '\0' ? "[_]" : $"[{c}]"));
        }

        private bool BoardIsDraw(char[,] board)
        {
            int size = board.GetLength(0);
            char[] temp = new char[size];

            // Helper function to check if a line contains both 'X' and 'O'
            bool ContainsBoth(char[] line)
            {
                bool hasX = false;
                bool hasO = false;

                foreach (char cell in line)
                {
                    if (cell == _p1) hasX = true;
                    if (cell == _p2) hasO = true;
                }

                return hasX && hasO;
            }

            // Check rows
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    temp[j] = board[i, j];
                }

                if (!ContainsBoth(temp))
                {
                    Debug.Log($"Problem line: {GetStylizedLine(temp)}");
                    return false;
                }
            }

            // Check columns
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    temp[i] = board[i, j];
                }

                if (!ContainsBoth(temp))
                {
                    Debug.Log($"Problem line: {GetStylizedLine(temp)}");
                    return false;
                }
            }

            // Check main diagonal
            for (int i = 0; i < size; i++)
            {
                temp[i] = board[i, i];
            }

            if (!ContainsBoth(temp))
            {
                Debug.Log($"Problem line: {GetStylizedLine(temp)}");
                return false;
            }

            // Check anti-diagonal
            for (int i = 0; i < size; i++)
            {
                temp[i] = board[i, size - 1 - i];
            }

            if (!ContainsBoth(temp))
            {
                Debug.Log($"Problem line: {GetStylizedLine(temp)}");
                return false;
            }

            // If all checks passed, it's a draw
            return true;
        }

        private bool BoardHasWin(char[,] board, (int, int) winningMove)
        {
            int size = board.GetLength(0);
            List<List<(int, int)>> foundWinningLines = new();

            // Helper function to check if a line is a winning line
            bool IsWinningLine(List<(int, int)> line)
            {
                char firstChar = board[line[0].Item1, line[0].Item2];
                if (firstChar == '\0') return false;

                foreach (var (row, col) in line)
                {
                    if (board[row, col] != firstChar)
                    {
                        return false;
                    }
                }
                return true;
            }

            // Check rows
            for (int i = 0; i < size; i++)
            {
                List<(int, int)> row = new();
                for (int j = 0; j < size; j++)
                {
                    row.Add((i, j));
                }
                if (IsWinningLine(row))
                {
                    foundWinningLines.Add(row);
                }
            }

            // Check columns
            for (int j = 0; j < size; j++)
            {
                List<(int, int)> column = new();
                for (int i = 0; i < size; i++)
                {
                    column.Add((i, j));
                }
                if (IsWinningLine(column))
                {
                    foundWinningLines.Add(column);
                }
            }

            // Check main diagonal
            List<(int, int)> mainDiagonal = new();
            for (int i = 0; i < size; i++)
            {
                mainDiagonal.Add((i, i));
            }
            if (IsWinningLine(mainDiagonal))
            {
                foundWinningLines.Add(mainDiagonal);
            }

            // Check anti-diagonal
            List<(int, int)> antiDiagonal = new();
            for (int i = 0; i < size; i++)
            {
                antiDiagonal.Add((i, size - 1 - i));
            }
            if (IsWinningLine(antiDiagonal))
            {
                foundWinningLines.Add(antiDiagonal);
            }

            // If there are no winning lines, return false
            if (foundWinningLines.Count == 0)
            {
                return false;
            }

            // If there are multiple winning lines
            if (foundWinningLines.Count > 1)
            {
                // All winning lines must share the same winning block
                for (int i = 0; i < foundWinningLines.Count; i++)
                {
                    if (!foundWinningLines[i].Contains(winningMove))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                // The only winning line left must contain the winning block
                return foundWinningLines[0].Contains(winningMove);
            }
        }

        [Test]
        public void PredeterminedBoards_WinWithTwoWinningLines()
        {
            (int, int) lastMove = new();
            char winner = '\0';

            _ticTacToeRunner = new TicTacToeRunner(3);
            _ticTacToeRunner.OnWin += (row, col, player) => { lastMove.Item1 = row; lastMove.Item2 = col; winner = player; };

            // Player 1 should win with 2 winning lines
            List<(int, int)> moves = TicTacToeRunner.ParseTuples("[1,0]\r\n[0,0]\r\n[1,2]\r\n[2,2]\r\n[2,0]\r\n[0,1]\r\n[0,2]\r\n[2,1]\r\n[1,1]");
            moves.Reverse();

            for (int i = moves.Count - 1; i >= 0; i--)
            {
                lastMove = moves[i];
                moves.RemoveAt(i);

                if (!_ticTacToeRunner.MakeMove(lastMove.Item1, lastMove.Item2))
                {
                    Debug.LogError($"Last move [{lastMove.Item1},{lastMove.Item2}] was invalid!");
                    _ticTacToeRunner.PrintCurrentBoard();
                    _ticTacToeRunner.PrintMoveHistory();
                    Assert.Fail("Unexpected failure, check console logs.");
                }

                if (_ticTacToeRunner.GameOver)
                {
                    Debug.Log($"Predetermined Boards: Claim is Win (size {3}):\n\n"
                               + $"{TicTacToeRunner.GetStylizedBoard(_ticTacToeRunner.GetCurrentBoard)}");
                    Assert.IsTrue(BoardHasWin(_ticTacToeRunner.GetCurrentBoard, lastMove));
                    return;
                }
            }

            Assert.Fail("Unexpected failure, check console logs.");
        }
    }
}
