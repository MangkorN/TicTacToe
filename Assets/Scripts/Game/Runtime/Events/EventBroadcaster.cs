using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CoreLib.Utilities;

namespace TicTacToe.Game
{
    [CreateAssetMenu(fileName = "EventBroadcaster", menuName = "TicTacToe/EventBroadcaster")]
    public class EventBroadcaster : ScriptableObject
    {
        [Serializable]
        public class EventTag
        {
            public string Tag;
            public UnityEvent Event;
        }

        [Header("Info"), HorizontalLine]
        [SerializeField] private string _name;
        [SerializeField, TextArea, Tooltip("Does not affect runtime.")] private string _description;
        [SerializeField] private string _lastInvokedTag;

        [Header("Events"), HorizontalLine]
        [SerializeField] private List<EventTag> _events = new();

        public string Name => _name;
#if UNITY_EDITOR
        public bool ShowDebugLog = false;
#endif

        /// <summary>
        /// Specify tag to invoke.
        /// </summary>
        /// <param name="tag">The tag of the event.</param>
        public void InvokeEventByTag(string tag)
        {
            InvokeTag(tag);
        }

        public bool InvokeTag(string tag)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (tag == _events[i].Tag)
                {
                    _events[i].Event?.Invoke();

                    Log($"tag '{tag}' invoked!");
                    _lastInvokedTag = tag;
                    return true;
                }
            }
            Debug.LogError($"tag '{tag}' not found! Failed to invoke.");
            return false;
        }

        public bool AddListener(string tag, UnityAction action)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (tag == _events[i].Tag)
                {
                    _events[i].Event.AddListener(action);
                    return true;
                }
            }
            Debug.LogError($"tag '{tag}' not found! Failed to add listener.");
            return false;
        }

        public bool RemoveListener(string tag, UnityAction action)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (tag == _events[i].Tag)
                {
                    _events[i].Event.RemoveListener(action);
                    return true;
                }
            }
            Debug.LogError($"tag '{tag}' not found! Failed to remove listener.");
            return false;
        }

        private void Log(string log)
        {
#if UNITY_EDITOR
            if (ShowDebugLog) Debug.Log($"EventBroadcaster '{_name}': {log}");
#endif
        }
    }
}