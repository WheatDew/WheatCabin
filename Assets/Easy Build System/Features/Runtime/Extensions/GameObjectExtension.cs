/// <summary>
/// Project : Easy Build System
/// Class : GameObjectExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class GameObjectExtension
    {
        public static Transform RecursiveFindChild(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}