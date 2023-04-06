using Battlehub.RTSL.Interface;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    public class RuntimeSceneManager : IRuntimeSceneManager
    {
        public event EventHandler NewSceneCreating;
        public event EventHandler NewSceneCreated;

        public void CreateNewScene()
        {
            if (NewSceneCreating != null)
            {
                NewSceneCreating(this, EventArgs.Empty);
            }

            ClearScene();

            if (NewSceneCreated != null)
            {
                NewSceneCreated(this, EventArgs.Empty);
            }
        }

        public void ClearScene()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                if (rootGO.GetComponent<RTSLIgnore>() || (rootGO.hideFlags & HideFlags.DontSave) != 0)
                {
                    continue;
                }

                if (RTSLSettings.SaveIncludedObjectsOnly)
                {
                    if (rootGO.GetComponent<RTSLInclude>() == null)
                    {
                        continue;
                    }
                }

                UnityObject.Destroy(rootGO);
            }
        }
    }
}

