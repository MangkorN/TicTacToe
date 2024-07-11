using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TicTacToe.Game
{
    public class UIEndingScreen : MonoBehaviour
    {
        [SerializeField] private GameObject[] _gameDrawLayers;
        [SerializeField] private GameObject[] _gameWinLayers;
        [SerializeField] private TextMeshProUGUI _textWin;

        private void OnEnable()
        {
            if (GameManager.MostRecentWinner != '\0') // Winner
            {
                ToggleGameObjects(_gameWinLayers, true);
                ToggleGameObjects(_gameDrawLayers, false);

                if (GameManager.MostRecentWinner == TicTacToeRunner.P1)
                    _textWin.text = "Duelist no.1";
                else if (GameManager.MostRecentWinner == TicTacToeRunner.P2)
                    _textWin.text = "Duelist no.2";
            }
            else // Draw
            {
                ToggleGameObjects(_gameDrawLayers, true);
                ToggleGameObjects(_gameWinLayers, false);
            }
        }

        private static void ToggleGameObjects(GameObject[] gameObjects, bool setActive)
        {
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(setActive);
            }
        }
    }
}
