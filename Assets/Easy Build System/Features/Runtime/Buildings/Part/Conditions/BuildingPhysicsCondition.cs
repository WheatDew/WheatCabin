/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Physics Condition",
        "Checks if the Building Part is stable. Otherwise the gravity will be applied to it.\n\n" +
        "You can find more information on the Building Physics Condition component in the documentation.", 3)]
    public class BuildingPhysicsCondition : BuildingCondition
    {
        #region Fields

        [Serializable]
        public class StabilityRay
        {
            [SerializeField] LayerMask m_LayerMask = 1 << 0;

            public LayerMask LayerMask { get { return m_LayerMask; } }

            [SerializeField] Bounds m_Bounds;
            public Bounds Bounds { get { return m_Bounds; } }

            [SerializeField] bool m_RequireAnyCollider;
            public bool RequireAnyCollider { get { return m_RequireAnyCollider; } }

            [BuildingType, SerializeField] string[] m_RequireType;

            public bool IsStable(BuildingPart buildingPart)
            {
                if (buildingPart == null)
                {
                    return false;
                }

                if (m_RequireAnyCollider)
                {
                    Collider[] colliders = PhysicsExtension.GetNeighborsType<Collider>(buildingPart.transform.TransformPoint(m_Bounds.center),
                        m_Bounds.extents, buildingPart.transform.rotation, m_LayerMask);

                    for (int x = 0; x < colliders.Length; x++)
                    {
                        if (colliders[x] != null && !buildingPart.Colliders.Contains(colliders[x]))
                        {
                            return true;
                        }
                    }
                }

                BuildingPart[] buildingParts =
                    PhysicsExtension.GetNeighborsType<BuildingPart>(buildingPart.transform.TransformPoint(m_Bounds.center),
                        m_Bounds.extents, buildingPart.transform.rotation, m_LayerMask);

                for (int i = 0; i < buildingParts.Length; i++)
                {
                    if (buildingParts[i] != null)
                    {
                        if (buildingParts[i] != null && buildingParts[i] != buildingPart)
                        {
                            if (m_RequireType.Contains(buildingParts[i].GetGeneralSettings.Type))
                            {
                                if (buildingParts[i].TryGetPhysicsCondition != null)
                                {
                                    if (!buildingParts[i].TryGetPhysicsCondition.IsFalling)
                                    {
                                        return true;
                                    }
                                }
                                else
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

        [SerializeField] StabilityRay[] m_Points;
        public StabilityRay[] Points { get { return m_Points; } set { m_Points = value; } }

        [SerializeField] bool m_IsSleeping = false;
        public bool IsSleeping { get { return m_IsSleeping; } set { m_IsSleeping = value; } }

        [SerializeField] float m_FallingTime = 5f;
        public float FallingTime { get { return m_FallingTime; } set { m_FallingTime = value; } }

        [SerializeField] float m_CheckStabilityInterval = 1f;

        [SerializeField] bool m_CanPlaceOnlyIfStable = true;

        [SerializeField] bool m_ShowDebugs = false;
        [SerializeField] bool m_ShowGizmos = true;

        Rigidbody m_Rigidbody;
        Rigidbody Rigidbody
        {
            get
            {
                if (m_Rigidbody == null)
                {
                    m_Rigidbody = PhysicsExtension.AddRigibody(GetBuildingPart.gameObject, false, true);
                }

                return m_Rigidbody;
            }
        }

        public bool IsFalling { get; private set; }

        #region Events

        /// <summary>
        /// Called when a Building Part is destroyed.
        /// </summary>
        [Serializable] public class FallingBuildingPartEvent : UnityEvent { }
        public FallingBuildingPartEvent OnFallingBuildingPartEvent = new FallingBuildingPartEvent();

        #endregion

        #endregion

        #region Unity Methods

        void Start()
        {
            BuildingManager.Instance.OnDestroyingBuildingPartEvent.AddListener((BuildingPart part) =>
            {
                if (part.State != BuildingPart.StateType.DESTROY)
                {
                    return;
                }

                CheckStability();
            });

            if (m_CheckStabilityInterval != 0)
            {
                InvokeRepeating(nameof(CheckStability), m_CheckStabilityInterval, m_CheckStabilityInterval);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

            if (m_Points == null || m_Points.Length == 0)
            {
                return;
            }

            if (GetBuildingPart == null)
            {
                return;
            }

            for (int i = 0; i < m_Points.Length; i++)
            {
                Gizmos.color = m_Points[i].IsStable(GetBuildingPart) ? Color.green : Color.red;
                Gizmos.DrawWireCube(GetBuildingPart.transform.TransformPoint(m_Points[i].Bounds.center), m_Points[i].Bounds.extents * 2);
            }
        }

        #endregion

        #region Internal Methods

        public bool CheckStability()
        {
            if (m_IsSleeping)
            {
                return true;
            }

            if (m_Points.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < m_Points.Length; i++)
            {
                if (!m_Points[i].IsStable(GetBuildingPart))
                {
                    if (Application.isPlaying)
                    {
                        if (m_ShowDebugs)
                        {
                            Debug.Log("<b>Easy Build System</b> : The Building Part is not stable.");
                        }
                    }

                    ApplyPhysics();

                    return false;
                }
            }

            if (Application.isPlaying)
            {
                if (m_ShowDebugs)
                {
                    Debug.Log("<b>Easy Build System</b> : The Building Part is stable.");
                }
            }

            return true;
        }

        public void ApplyPhysics()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (GetBuildingPart == null)
            {
                return;
            }

            if (GetBuildingPart.State != BuildingPart.StateType.PLACED)
            {
                return;
            }

            if (IsFalling)
            {
                return;
            }

            IsFalling = true;

            OnFallingBuildingPartEvent?.Invoke();

            for (int i = 0; i < GetBuildingPart.Colliders.Count; i++)
            {
                if (GetBuildingPart.Colliders[i] != null)
                {
                    if (GetBuildingPart.Colliders[i].GetComponent<MeshCollider>() != null)
                    {
                        GetBuildingPart.Colliders[i].GetComponent<MeshCollider>().convex = true;
                    }
                }
            }

            for (int i = 0; i < GetBuildingPart.Sockets.Length; i++)
            {
                GetBuildingPart.Sockets[i].IsDisabled = true;
            }

            Rigidbody.useGravity = true;
            Rigidbody.isKinematic = false;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Destroy(GetBuildingPart);
            Destroy(GetBuildingPart.gameObject, m_FallingTime);
        }

        public override bool CheckPlacingCondition()
        {
            if (m_IsSleeping)
            {
                return true;
            }

            if (m_CanPlaceOnlyIfStable)
            {
                return CheckStability();
            }

            return true;
        }

        #endregion
    }
}