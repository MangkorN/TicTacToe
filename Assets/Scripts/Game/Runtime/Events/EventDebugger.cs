using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Game
{
    [CreateAssetMenu(fileName = "EventDebugger", menuName = "TicTacToe/EventDebugger")]
    public class EventDebugger : ScriptableObject
    {
        public void DebugLog(string log)
        {
            Debug.Log(log);
        }
    }
}