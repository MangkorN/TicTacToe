using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CoreLib.Behaviors;

namespace CoreLib.Utilities
{
    public static class ResetManager
    {
        private static List<GameObject> persistentObjects = new List<GameObject>();

        public static void RegisterPersistentObject(GameObject obj)
        {
            persistentObjects.Add(obj);
        }

        public static void ResetGame()
        {
            foreach (var obj in persistentObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            persistentObjects.Clear();

            // Optionally: unload all unused assets to free memory
            //Resources.UnloadUnusedAssets();

            // Reload the scene after a short delay to ensure all objects are destroyed
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}