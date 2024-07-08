using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TicTacToe.Game.Editor
{
    [CustomEditor(typeof(EventDebugger))]
    public class EventDebuggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Insert me into any EventBroadcaster field, and use me to print logs, etc.");
        }
    }
}
