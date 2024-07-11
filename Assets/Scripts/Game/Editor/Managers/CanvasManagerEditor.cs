using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CoreLib.Utilities.Editor;

namespace TicTacToe.Game.Editor
{
    [CustomEditor(typeof(CanvasManager))]
    public class CanvasManagerEditor : UnityEditor.Editor
    {
        private Color _defaultColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        public override void OnInspectorGUI()
        {
            GUILayout.Space(5f);
            GUILayout.Label("Canvas Controller Status", EditorStyles.boldLabel);
            
            GUILayout.Space(5f);

            GUILayout.Label($"Canvas Controller | Active animations: {CanvasController.NumAnimationsGlobal}");
            GUILayout.Label($"Canvas Controller | Highest anim count: {CanvasController.HighestAnimationsCountedPerTransition}");

            GUILayout.Space(5f);

            CustomInspectorUtilities.DrawUILine(_defaultColor);

            GUILayout.Space(5f);

            DrawDefaultInspector();
            CanvasManager canvasManager = (CanvasManager)target;
            canvasManager.GenerateCanvasSettings();

            GUIStyle successLabelStyle = new(EditorStyles.label);
            successLabelStyle.normal.textColor = Color.green;
            successLabelStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle errorLabelStyle = new(EditorStyles.boldLabel);
            errorLabelStyle.normal.textColor = Color.red;
            errorLabelStyle.alignment = TextAnchor.MiddleCenter;

            if (canvasManager.CanvasSettingsAreValid())
                GUILayout.Label("Canvas settings are valid!", successLabelStyle);
            else
                GUILayout.Label("Canvas settings are invalid!", errorLabelStyle);

            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("UTILITIES");
            GUILayout.Space(5f);

            if (GUILayout.Button("Set DisableRaycastersBeneath ON for all"))
            {
                canvasManager.SetAllDisableRaycastersBeneath(true);
                EditorUtility.SetDirty(canvasManager);
            }

            if (GUILayout.Button("Set DisableRaycastersBeneath OFF for all"))
            {
                canvasManager.SetAllDisableRaycastersBeneath(false);
                EditorUtility.SetDirty(canvasManager);
            }

            if (GUILayout.Button("Refresh Graphic Raycasters"))
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("Must be in PlayMode!");
                    return;
                }

                canvasManager.ManualRefreshGraphicRaycasters();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
