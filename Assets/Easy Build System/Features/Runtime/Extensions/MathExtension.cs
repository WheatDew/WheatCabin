/// <summary>
/// Project : Easy Build System
/// Class : MathExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Linq;

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class MathExtension
    {
        public static Bounds GetWorldBounds(this Transform transform, Bounds localBounds)
        {
            if (transform != null)
            {
                return new Bounds(transform.TransformPoint(localBounds.center), 
                    new Vector3(localBounds.size.x * transform.localScale.x,
                    localBounds.size.y * transform.localScale.y,
                    localBounds.size.z * transform.localScale.z));
            }
            else
            {
                return new Bounds(localBounds.center,
                    new Vector3(localBounds.size.x * transform.localScale.x,
                    localBounds.size.y * transform.localScale.y,
                    localBounds.size.z * transform.localScale.z));
            }
        }

        public static Bounds GetBounds(this GameObject target, Renderer[] excludeRenders)
        {
            MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();

            Quaternion currentRotation = target.transform.rotation;
            Vector3 currentScale = target.transform.localScale;

            target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            target.transform.localScale = Vector3.one;

            Bounds resultBounds = new Bounds(target.transform.position, Vector3.zero);

            foreach (Renderer renderer in renderers)
            {
                if (!excludeRenders.Contains(renderer))
                {
                    resultBounds.Encapsulate(renderer.bounds);
                }
            }

            resultBounds.center -= target.transform.position;
            resultBounds.size = resultBounds.size;

            target.transform.rotation = currentRotation;
            target.transform.localScale = currentScale;

            return resultBounds;
        }

        public static Vector3 Grid(float gridSize, float gridOffset, Vector3 position)
        {
            position -= Vector3.one * gridOffset;
            position /= gridSize;
            position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
            position *= gridSize;
            position += Vector3.one * gridOffset;
            return position;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
            return value;
        }
    }
}