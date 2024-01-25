/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Collision Condition",
        "Checks if the Building Part does not collide with undesirable colliders during preview mode.\n\n" +
        "You can find more information on the Building Collision Condition component in the documentation.", 2)]
    public class BuildingCollisionCondition : BuildingCondition
    {
        #region Fields

        [SerializeField] LayerMask m_LayerMask = 1 << 0;

        [SerializeField] [Range(0f, 10f)] float m_Tolerance = 0.9f;

        [SerializeField] bool m_IgnoreBuildingSurface;

        [SerializeField] int m_BuildingSurfaceFlags;
        public int CollisionBuildingSurfaceFlags { get { return m_BuildingSurfaceFlags; } set { m_BuildingSurfaceFlags = value; } }

        [SerializeField] bool m_ShowDebugs = false;
        
        [SerializeField] bool m_ShowGizmos = true;

        List<string> m_CollisionSurfaces = new List<string>();

        #endregion

        #region Unity Methods

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.Selection.gameObjects.Length > 6)
            {
                return;
            }
#endif

            if (GetBuildingPart == null)
            {
                return;
            }

            Gizmos.matrix = GetBuildingPart.transform.localToWorldMatrix;

            bool canPlacing = CheckPlacingCondition();

            Gizmos.color = (canPlacing ? Color.cyan : Color.red) / 2f;
            Gizmos.DrawCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);

            Gizmos.color = (canPlacing ? Color.cyan : Color.red);
            Gizmos.DrawWireCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);
        }

        #endregion

        #region Internal Methods

        public override bool CheckPlacingCondition()
        {
#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
#else
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
#endif
            {
                return true;
            }
#endif

            Bounds worldCollisionBounds = GetBuildingPart.transform.GetWorldBounds(GetBuildingPart.GetModelSettings.ModelBounds);

            Collider[] colliders = PhysicsExtension.GetNeighborsType<Collider>(worldCollisionBounds.center,
                    worldCollisionBounds.extents * m_Tolerance, GetBuildingPart.transform.rotation, m_LayerMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    if (!GetBuildingPart.Colliders.Contains(colliders[i]))
                    {
                        //if (m_BuildingSurfaceFlags != 0 && BuildingManager.Instance.AllSurfaces.Count != 0)
                        //{                            
                            BuildingCollisionSurface buildingCollisionSurface = colliders[i].GetComponent<BuildingCollisionSurface>();

                            if (buildingCollisionSurface != null)
                            {
                                if (!ContainsSurface(buildingCollisionSurface.SurfaceIdentifier))
                                {
                                    if (m_ShowDebugs)
                                    {
                                        Debug.LogWarning("<b>Easy Build System</b> : Colliding with " +
                                        colliders[i].name);
                                    }

                                    return false;
                                }
                            }
                            else
                            {
                                if (m_ShowDebugs)
                                {
                                    Debug.LogWarning("<b>Easy Build System</b> : Colliding with " +
                                    colliders[i].name);
                                }

                                return false;
                            }
                        //}
                        //else
                        //{
                        //    return m_IgnoreBuildingSurface;
                        //}
                    }
                }
            }

            return true;
        }

        bool ContainsSurface(string surfaceIdentifier)
        {
            if (m_IgnoreBuildingSurface)
            {
                return true;
            }

            m_CollisionSurfaces = BuildingManager.Instance.AllSurfaces;

            for (int i = 0; i < m_CollisionSurfaces.Count; i++)
            {
                int layer = 1 << i;

                if ((m_BuildingSurfaceFlags & layer) != 0)
                {
                    if (surfaceIdentifier == m_CollisionSurfaces[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}