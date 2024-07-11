using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Game
{
    /// <summary>
    /// References a grid block in the scene.
    /// </summary>
    public class GridBlock
    {
        private readonly GameObject _block;

        public Vector3 Position
        {
            get
            {
                if (_block == null)
                {
                    Debug.LogWarning("GridBlock: The referenced GameObject has been destroyed.");
                    return Vector3.zero; // Return a default value or handle it as needed
                }
                return _block.transform.position;
            }
        }

        public GridBlock(GameObject block)
        {
            _block = block;
        }

        #region Utilities

        /// <summary>
        /// Verifies that all the SizeN boards in the gameBoardsParent are set up correctly.
        /// </summary>
        /// <param name="gameBoardsParent">The parent transform containing the game board size objects.</param>
        /// <returns>True if all boards are set up correctly, false otherwise.</returns>
        public static bool VerifyGridBoardsSetup(Transform gameBoardsParent, bool verbose = false)
        {
            foreach (Transform sizeTransform in gameBoardsParent)
            {
                string sizeName = sizeTransform.name;
                if (sizeName.StartsWith("Size"))
                {
                    if (!int.TryParse(sizeName[4..], out int size))
                    {
                        Debug.LogWarning($"Invalid board size format: {sizeName}");
                        return false;
                    }

                    int expectedChildCount = size * size;
                    if (sizeTransform.childCount != expectedChildCount)
                    {
                        Debug.LogWarning($"Size{size} board has incorrect number of children. Expected: {expectedChildCount}, Found: {sizeTransform.childCount}");
                        return false;
                    }

                    var existingCoordinates = new HashSet<(int, int)>();
                    foreach (Transform blockTransform in sizeTransform)
                    {
                        var blockName = blockTransform.name;
                        var coordinates = ParseCoordinates(blockName);
                        if (!existingCoordinates.Add(coordinates))
                        {
                            Debug.LogWarning($"Duplicate coordinates found: {blockName} in Size{size} board.");
                            return false;
                        }
                    }

                    // Verify that all expected coordinates are present
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            if (!existingCoordinates.Remove((i, j)))
                            {
                                Debug.LogWarning($"Missing or incorrectly named block at ({i},{j}) in Size{size} board.");
                                return false;
                            }
                        }
                    }

                    // If there are any coordinates left in the set, something went wrong
                    if (existingCoordinates.Count != 0)
                    {
                        Debug.LogWarning($"Extra blocks found in Size{size} board.");
                        return false;
                    }
                }
            }

            DebugLog("All grid boards are set up correctly.", verbose);
            return true;
        }

        /// <summary>
        /// Generates a mapping of grid blocks for different game board sizes. <br></br>
        /// Each key in the outer dictionary represents the board size (e.g. 5 for a 5x5 board), <br></br>
        /// and the value is another dictionary mapping grid coordinates to their corresponding grid block. <br></br>
        /// e.g. KEY=5, VALUE(KEY=(2,3)) => Grid block at (2,3) in the 5x5 game board.
        /// </summary>
        /// <param name="gameBoardsParent">The parent transform containing the game board size objects.</param>
        /// <returns>A dictionary mapping board sizes to their grid blocks and coordinates.</returns>
        public static Dictionary<int, Dictionary<(int, int), GridBlock>> GenerateGridBlockMap(Transform gameBoardsParent)
        {
            var gridBlockMap = new Dictionary<int, Dictionary<(int, int), GridBlock>>();

            foreach (Transform sizeTransform in gameBoardsParent)
            {
                string sizeName = sizeTransform.name;
                if (sizeName.StartsWith("Size"))
                {
                    int size = int.Parse(sizeName[4..]);
                    var sizeDictionary = new Dictionary<(int, int), GridBlock>();

                    foreach (Transform blockTransform in sizeTransform)
                    {
                        string blockName = blockTransform.name;
                        var coordinates = ParseCoordinates(blockName);
                        sizeDictionary[coordinates] = new GridBlock(blockTransform.gameObject);
                    }

                    gridBlockMap[size] = sizeDictionary;
                }
            }

            return gridBlockMap;
        }

        /// <summary>
        /// Generates a mapping of grid boards for different game board sizes. <br></br>
        /// Each key in the dictionary represents the board size (e.g. 5 for a 5x5 board), <br></br>
        /// and the value is the GameObject representing the grid board. <br></br>
        /// Note: A grid board (of size n) is the parent object of n x n amount of grid blocks.
        /// </summary>
        /// <param name="gameBoardsParent">The parent transform containing the game board size objects.</param>
        /// <returns>A dictionary mapping board sizes to their corresponding grid board GameObjects.</returns>
        public static Dictionary<int, GameObject> GenerateGridBoards(Transform gameBoardsParent)
        {
            var gridBoards = new Dictionary<int, GameObject>();

            foreach (Transform sizeTransform in gameBoardsParent)
            {
                string sizeName = sizeTransform.name;
                if (sizeName.StartsWith("Size"))
                {
                    int size = int.Parse(sizeName[4..]);
                    gridBoards[size] = sizeTransform.gameObject;
                }
            }

            return gridBoards;
        }

        /// <summary>
        /// Parses a string in the format "x,y" into a tuple of integers (x, y).
        /// </summary>
        /// <param name="blockName">The string to parse, representing the coordinates of a block.</param>
        /// <returns>A tuple (x, y) representing the parsed coordinates.</returns>
        public static (int, int) ParseCoordinates(string blockName)
        {
            var parts = blockName.Split(',');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            return (x, y);
        }

        /// <summary>
        /// Retrieves the position of a grid block in the scene.
        /// </summary>
        /// <param name="gameSize">The size of the game board.</param>
        /// <param name="blockPosition">The row and column of the grid block.</param>
        /// <param name="gridBlockMap">The grid block mapping that contains the grid block.</param>
        /// <returns></returns>
        public static Vector3 GetBlockPositionFromMap
            (int gameSize, (int, int) blockPosition, Dictionary<int, Dictionary<(int, int), GridBlock>> gridBlockMap)
        {
            if (!gridBlockMap.TryGetValue(gameSize, out var gameBoard)) // Find game board by game size.
            {
                Debug.LogWarning($"Cannot find game board of size {gameSize}!");
                return Vector3.zero;
            }

            if (!gameBoard.TryGetValue(blockPosition, out var block)) // Find grid block by coordinates (row, column).
            {
                Debug.LogWarning($"Cannot find grid block at [{blockPosition.Item1},{blockPosition.Item2}]!");
                return Vector3.zero;
            }

            return block.Position;
        }

        private static void DebugLog(string log, bool verbose = true)
        {
            if (verbose) Debug.Log(log);
        }

        #endregion
    }
}
