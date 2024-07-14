using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLib.Components;

namespace TicTacToe.Game
{
    public class Marker : MonoBehaviour
    {
        private void Awake()
        {
            SystemLoader.OnSystemUnload += SelfDestruct;
        }

        private void OnDestroy()
        {
            SystemLoader.OnSystemUnload -= SelfDestruct;
        }

        private void SelfDestruct()
        {
            Destroy(gameObject);
        }
    }
}