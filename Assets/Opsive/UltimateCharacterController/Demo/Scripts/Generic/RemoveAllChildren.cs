/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Generic
{
    using System;
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// This component has a function to remove all children of a gameobject.
    /// </summary>
    public class RemoveAllChildren : MonoBehaviour
    {
        /// <summary>
        /// Remove all the children of the gameobject.
        /// </summary>
        public void DoRemoveAllChildren()
        {
            DoRemoveAllChildren(gameObject);
        }
        
        /// <summary>
        /// Remove all the children of the specified gameobject.
        /// </summary>
        /// <param name="other">The gameobject to remove the children from.</param>
        public void DoRemoveAllChildren(GameObject other)
        {
            var gameObjectTransform = other.transform;
            for (int i = gameObjectTransform.childCount - 1; i >= 0; i--) {
                var child = gameObjectTransform.GetChild(i).gameObject;
                if (ObjectPool.IsPooledObject(child)) {
                    ObjectPool.Destroy(child);
                } else {
                    Destroy(child);
                }
            }
        }
    }
}
