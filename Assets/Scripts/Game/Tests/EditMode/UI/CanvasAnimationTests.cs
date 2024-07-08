using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TicTacToe.UnitTest.UI
{
    public class CanvasAnimationTests
    {
        private const string AnimationFolderPath = "Assets/UI/Animators/Clips";
        private const string StartEventName = "AnimationStart";
        private const string EndEventName = "AnimationEnd";
        private const float EndEventTime = 0.5f;

        [Test]
        public void ValidateAnimationEvents()
        {
            string[] animationClipPaths = AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationFolderPath })
                                                       .Select(AssetDatabase.GUIDToAssetPath)
                                                       .ToArray();

            foreach (string path in animationClipPaths)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                Assert.IsTrue(HasEventAtTime(clip, 0, StartEventName), $"Animation clip '{clip.name}' is missing the '{StartEventName}' event at time 0.");
                Assert.IsTrue(HasEventAtTime(clip, EndEventTime, EndEventName), $"Animation clip '{clip.name}' is missing the '{EndEventName}' event at time {EndEventTime}.");
            }
        }

        private bool HasEventAtTime(AnimationClip clip, float time, string eventName)
        {
            return clip.events.Any(e => Mathf.Approximately(e.time, time) && e.functionName == eventName);
        }

        [Test]
        public void ValidateAnchorProperties()
        {
            string[] animationClipPaths = AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationFolderPath })
                                                       .Select(AssetDatabase.GUIDToAssetPath)
                                                       .ToArray();

            foreach (string path in animationClipPaths)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip.name.Contains("Outro"))
                {
                    ValidateAnchorAtTime(clip, 0, "Outro");
                }
                else if (clip.name.Contains("Intro"))
                {
                    ValidateAnchorAtTime(clip, EndEventTime, "Intro");
                }
            }
        }

        private void ValidateAnchorAtTime(AnimationClip clip, float time, string type)
        {
            bool valid = HasAnchorPropertyAtTime(clip, "m_AnchorMax.x", time, 1) &&
                         HasAnchorPropertyAtTime(clip, "m_AnchorMax.y", time, 1) &&
                         HasAnchorPropertyAtTime(clip, "m_AnchorMin.x", time, 0) &&
                         HasAnchorPropertyAtTime(clip, "m_AnchorMin.y", time, 0);

            Assert.IsTrue(valid, $"Animation clip '{clip.name}' ({type}) does not have correct anchor properties at time {time}.");
        }

        private bool HasAnchorPropertyAtTime(AnimationClip clip, string propertyName, float time, float expectedValue)
        {
            var curveBindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in curveBindings)
            {
                if (binding.propertyName == propertyName)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve != null)
                    {
                        foreach (var key in curve.keys)
                        {
                            if (Mathf.Approximately(key.time, time) && Mathf.Approximately(key.value, expectedValue))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
