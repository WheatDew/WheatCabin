/// <summary>
/// Project : Easy Build System
/// Class : PhysicsExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class PhysicsExtension
    {
        const int MAX_ALLOC_COUNT = 32;

        static Collider[] m_Colliders = new Collider[MAX_ALLOC_COUNT];

        public static Rigidbody AddRigibody(GameObject target, bool useGravity, bool isKinematic)
        {
            if (target == null)
            {
                return null;
            }

            Rigidbody rigidbody = target.AddComponent<Rigidbody>();
            rigidbody.useGravity = useGravity;
            rigidbody.isKinematic = isKinematic;

            return rigidbody;
        }

        public static T[] GetNeighborsType<T>(Vector3 position, Vector3 size, Quaternion rotation, LayerMask layer, 
            QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
        {
            bool initQueries = Physics.queriesHitTriggers;

            Physics.queriesHitTriggers = true;

            m_Colliders = new Collider[MAX_ALLOC_COUNT];

            int colliderCount = Physics.OverlapBoxNonAlloc(position, size, m_Colliders, rotation, layer, query);

            Physics.queriesHitTriggers = initQueries;

            T[] types = new T[colliderCount];

            for (int i = 0; i < colliderCount; i++)
            {
                if (!m_Colliders[i].isTrigger)
                {
                    T type = m_Colliders[i].GetComponentInParent<T>();

                    if (type != null)
                    {
                        if (type is T)
                        {
                            types[i] = type;
                        }
                    }
                }
            }

            return types;
        }
    }
}