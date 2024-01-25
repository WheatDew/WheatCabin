/// <summary>
/// Project : Easy Build System
/// Class : BuildingArea.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Area
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EasyBuildSystem.Features.Runtime.Buildings.Area
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-area")]
    public class BuildingArea : MonoBehaviour
    {
        #region Fields

        public enum ShapeType { SPHERE, BOUNDS }

        [SerializeField] ShapeType m_Shape = ShapeType.SPHERE;
        public ShapeType Shape { get { return m_Shape; } }

        [SerializeField] float m_Radius = 5f;
        public float Radius { get { return m_Radius; } }

        [SerializeField] Bounds m_Bounds = new Bounds(Vector3.zero, Vector3.one);
        public Bounds Bounds { get { return m_Bounds; } }

        [SerializeField] bool m_CanPlacingAnyBuildingParts = true;
        public bool CanPlacingAnyBuildingParts { get { return m_CanPlacingAnyBuildingParts; } }

        [SerializeField] List<BuildingPart> m_CanPlacingSpecificBuildingParts = new List<BuildingPart>();

        [SerializeField] bool m_CanEditingAnyBuildingParts = true;
        public bool CanEditingAnyBuildingParts { get { return m_CanEditingAnyBuildingParts; } }

        [SerializeField] List<BuildingPart> m_CanEditingSpecificBuildingParts = new List<BuildingPart>();

        [SerializeField] bool m_CanDestroyingAnyBuildingParts = true;
        public bool CanDestroyingAnyBuildingParts { get { return m_CanDestroyingAnyBuildingParts; } }

        [SerializeField] List<BuildingPart> m_CanDestroyingSpecificBuildingParts = new List<BuildingPart>();

        [SerializeField] List<BuildingPart> m_RegisteredBuildingParts = new List<BuildingPart>();

        #region Events

        /// <summary>
        /// Called when a Building Part is placed in the area.
        /// </summary>
        [Serializable] public class RegisterBuildingPartEvent : UnityEvent<BuildingPart> { }
        public RegisterBuildingPartEvent OnRegisterBuildingPartEvent = new RegisterBuildingPartEvent();

        /// <summary>
        /// Called when a Building Part is destroyed in the area.
        /// </summary>
        [Serializable] public class UnregisterBuildingPartEvent : UnityEvent { }
        public UnregisterBuildingPartEvent OnUnregisterBuildingPartEvent = new UnregisterBuildingPartEvent();

        #endregion

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterBuildingArea(this);
            }
        }

        void OnDisable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuildingArea(this);
            }
        }

        void OnDestroy()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuildingArea(this);
            }
        }

        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (m_Shape == ShapeType.SPHERE)
            {
                Handles.color = !m_CanPlacingAnyBuildingParts ? Color.red / 4f : Color.cyan / 4f;
                Handles.DrawSolidArc(transform.position, transform.up, transform.right, 360f, m_Radius);
                Handles.color = !m_CanPlacingAnyBuildingParts ? Color.red : Color.cyan;
                Handles.DrawWireDisc(transform.position, transform.up, m_Radius);
            }
            else
            {
                Bounds bounds = MathExtension.GetWorldBounds(transform, m_Bounds);
                Gizmos.color = !m_CanPlacingAnyBuildingParts ? Color.red / 4f: Color.cyan / 4f;
                Gizmos.DrawCube(bounds.center, m_Bounds.size);
                Handles.color = !m_CanPlacingAnyBuildingParts ? Color.red : Color.cyan;
                Handles.DrawWireCube(bounds.center, m_Bounds.size);
            }
#endif
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Check if the Building Part can be placed in this area.
        /// </summary>
        public bool CanPlacingBuildingPart(BuildingPart buildingPart)
        {
            if (m_CanPlacingSpecificBuildingParts.Count == 0)
            {
                return false;
            }

            return m_CanPlacingSpecificBuildingParts.Find(entry => entry.GetGeneralSettings.Identifier == buildingPart.GetGeneralSettings.Identifier);
        }

        /// <summary>
        /// Check if the Building Part can be destroyed in this area.
        /// </summary>
        public bool CanDestroyBuildingPart(BuildingPart buildingPart)
        {
            if (m_CanDestroyingSpecificBuildingParts.Count == 0)
            {
                return false;
            }

            return m_CanDestroyingSpecificBuildingParts.Find(entry => 
                entry.GetGeneralSettings.Identifier == buildingPart.GetGeneralSettings.Identifier);
        }

        /// <summary>
        /// Check if the Building Part can be edited in this area.
        /// </summary>
        public bool CanEditingBuildingPart(BuildingPart buildingPart)
        {
            if (m_CanEditingSpecificBuildingParts.Count == 0)
            {
                return false;
            }

            return m_CanEditingSpecificBuildingParts.Find(entry =>
                entry.GetGeneralSettings.Identifier == buildingPart.GetGeneralSettings.Identifier);
        }

        /// <summary>
        /// Register the Building Part.
        /// </summary>
        public void RegisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            buildingPart.AttachedBuildingArea = this;

            m_RegisteredBuildingParts.Add(buildingPart);

            OnRegisterBuildingPartEvent?.Invoke(buildingPart);
        }

        /// <summary>
        /// Unregister the Building Part. 
        /// </summary>
        public void UnregisterBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            buildingPart.AttachedBuildingArea = null;

            m_RegisteredBuildingParts.Remove(buildingPart);

            OnUnregisterBuildingPartEvent?.Invoke();
        }

        #endregion
    }
}