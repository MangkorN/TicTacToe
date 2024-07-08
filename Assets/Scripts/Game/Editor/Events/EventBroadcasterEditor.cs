using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TicTacToe.Game.Editor
{
    [CustomEditor(typeof(EventBroadcaster))]
    public class EventBroadcasterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EventBroadcaster eventBroadcaster = (EventBroadcaster)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("MANUAL INVOKE");
            GUILayout.Space(5f);

            FieldInfo eventsField = typeof(EventBroadcaster).GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsField != null)
            {
                List<EventBroadcaster.EventTag> events = (List<EventBroadcaster.EventTag>)eventsField.GetValue(eventBroadcaster);

                if (events != null)
                {
                    foreach (EventBroadcaster.EventTag eventTag in events)
                    {
                        if (string.IsNullOrEmpty(eventTag.Tag))
                            eventTag.Tag = "Default";

                        string buttonName = eventTag.Tag;
                        string firstLetter = buttonName[..1].ToUpper();
                        string restOfName = buttonName[1..];
                        buttonName = firstLetter + restOfName;

                        if (GUILayout.Button($"{buttonName}"))
                        {
                            if (!Application.isPlaying)
                            {
                                Debug.LogWarning("Must be in PlayMode!");
                                return;
                            }

                            eventTag.Event?.Invoke();
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
