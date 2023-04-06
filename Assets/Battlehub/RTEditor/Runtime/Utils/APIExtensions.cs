using UnityEngine;

namespace Battlehub
{
    public static class ParticleSystemExt
    {
        public static int GetColliderCount(this ParticleSystem.TriggerModule o)
        {
            #if UNITY_2020_2_OR_NEWER
            return o.colliderCount;
            #else
            return o.maxColliderCount;
            #endif
        }

        public static int GetPlaneCount(this ParticleSystem.CollisionModule o)
        {
            #if UNITY_2020_2_OR_NEWER
            return o.planeCount;
            #else
            return o.maxPlaneCount;
            #endif
        }
    }
}
