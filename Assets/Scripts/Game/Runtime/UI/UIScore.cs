using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Game
{
    public class UIScore : MonoBehaviour
    {
        [SerializeField] private GameObject HintPlayer1Turn;
        [SerializeField] private GameObject HintPlayer2Turn;

        private void Start()
        {
            // Assume that player 1 goes first
            HintPlayer1Turn.SetActive(true);
            HintPlayer2Turn.SetActive(false);

            if (GridBoardsController.Instance != null)
                GridBoardsController.Instance.OnPlayerMoveComplete += HandlePlayerMoveAndRespond;
        }

        private void OnDestroy()
        {
            if (GridBoardsController.Instance != null)
                GridBoardsController.Instance.OnPlayerMoveComplete -= HandlePlayerMoveAndRespond;
        }

        private void HandlePlayerMoveAndRespond(int row, int col, char player)
        {
            HintPlayer1Turn.SetActive(player != TicTacToeRunner.P1);
            HintPlayer2Turn.SetActive(player != TicTacToeRunner.P2);
        }
    }
}
