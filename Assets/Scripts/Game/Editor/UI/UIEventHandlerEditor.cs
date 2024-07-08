using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TicTacToe.Game.Editor
{
    [CustomEditor(typeof(UIEventHandler))]
    public class UIEventHandlerEditor : EventBroadcasterEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
