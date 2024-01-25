/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocket.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Socket
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Socket
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-socket")]
    [ExecuteInEditMode]
    public class BuildingSocket : MonoBehaviour
    {
        #region Fields

        [Serializable]
        public class SnappingPointSettings
        {
            public enum MatchType { BUILDING_PART_REFERENCE, BUILDING_PART_TYPE }

            [SerializeField] MatchType m_MatchBy;
            public MatchType MatchBy { get { return m_MatchBy; } set { m_MatchBy = value; } }

            [SerializeField] BuildingPart m_BuildingPart;
            public BuildingPart BuildingPart { get { return m_BuildingPart; } set { m_BuildingPart = value; } }

            [SerializeField, BuildingType] public string m_Type;
            public string Type { get { return m_Type; } set { m_Type = value; } }

            [SerializeField] Vector3 m_Position;
            public Vector3 Position { get { return m_Position; } set { m_Position = value; } }

            [SerializeField] Vector3 m_Rotation;
            public Vector3 Rotation { get { return m_Rotation; } set { m_Rotation = value; } }

            [SerializeField] public Vector3 m_Scale = Vector3.one;
            public Vector3 Scale { get { return m_Scale; } set { m_Scale = value; } }
        }

        [SerializeField] List<SnappingPointSettings> m_SnappingPoints = new List<SnappingPointSettings>();
        public List<SnappingPointSettings> SnappingPoints { get { return m_SnappingPoints; } set { m_SnappingPoints = value; } }

        [SerializeField] float m_SocketRadius = 0.25f;
        public float SocketRadius { get { return m_SocketRadius; } set { m_SocketRadius = value; } }

        public bool IsDisabled { get; set; }

        BuildingPart m_BuildingPart;
        public BuildingPart GetBuildingPart
        {
            get
            {
                if (m_BuildingPart == null)
                {
                    m_BuildingPart = GetComponentInParent<BuildingPart>();
                }

                return m_BuildingPart;
            }
        }

        BuildingPart m_Preview;
        public BuildingPart Preview { get { return m_Preview; } set { m_Preview = value; } }

        bool m_SocketBusy;
        public bool SocketBusy { get { return m_SocketBusy; } set { m_SocketBusy = value; } }

        SphereCollider m_Collider;

        bool m_IsQuitting;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterBuildingSocket(this);         
            }

            if (m_Collider == null)
            {
                m_Collider = GetComponent<SphereCollider>();

                if (m_Collider == null)
                {
                    m_Collider = gameObject.AddComponent<SphereCollider>();
                    m_Collider.radius = m_SocketRadius;
                    m_Collider.isTrigger = true;
                    m_Collider.gameObject.layer = LayerMask.NameToLayer("Socket");
                    m_Collider.hideFlags = HideFlags.NotEditable;
                }
            }

            if (m_Collider != null)
            {
                m_Collider.radius = m_SocketRadius;
            }

            ClearPreview();
        }

        void OnValidate()
        {
            if (m_Collider != null)
            {
                m_Collider.radius = m_SocketRadius;
                m_Collider.hideFlags = HideFlags.NotEditable;
            }
        }

        void OnApplicationQuit()
        {
            m_IsQuitting = true;
        }

        void OnDestroy()
        {
            if (m_IsQuitting)
            {
                return;
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuildingSocket(this);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (IsDisabled)
            {
                return;
            }

            Gizmos.color = SocketBusy ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_SocketRadius);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Socket is enabled?
        /// </summary>
        public bool IsEnabled()
        {
            return !IsDisabled;
        }

        /// <summary>
        /// Clear current preview.
        /// </summary>
        public void ClearPreview()
        {
            //for (int i = 0; i < transform.childCount; i++)
            //{
            //    if (transform.GetChild(i) != null)
            //    {
            //        if (transform.GetChild(i).name.Contains("Preview"))
            //        {
            //            if (Application.isPlaying)
            //            {
            //                Destroy(transform.GetChild(i).gameObject);
            //            }
            //            else
            //            {
            //                DestroyImmediate(transform.GetChild(i).gameObject, true);
            //            }
            //        }
            //    }
            //}

            if (m_Preview == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(m_Preview.gameObject);
            }
            else
            {
                DestroyImmediate(m_Preview.gameObject, true);
            }

            m_Preview = null;
        }

        /// <summary>
        /// Preview Building Part.
        /// </summary>
        public void ShowPreview(SnappingPointSettings offsetSettings, bool checkCollisionCondition = false)
        {
            if (offsetSettings == null)
            {
                return;
            }

            BuildingPart offsetBuildingPart = GetOffsetBuildingPart(offsetSettings);

            BuildingPart instancedBuildingPart = Instantiate(offsetBuildingPart, transform.position, transform.rotation, transform);
            instancedBuildingPart.ChangeState(BuildingPart.StateType.PREVIEW);

            instancedBuildingPart.gameObject.name = "Preview";

            Snap(instancedBuildingPart, offsetSettings, Vector3.zero);

            if (checkCollisionCondition)
            {
                if (instancedBuildingPart.TryGetCollisonCondition != null)
                {
                    if (!instancedBuildingPart.TryGetCollisonCondition.CheckPlacingCondition())
                    {
                        Destroy(instancedBuildingPart.gameObject);
                        return;
                    }
                }
            }

            m_Preview = instancedBuildingPart;
        }

        /// <summary>
        /// Get Building Part from snapping point.
        /// </summary>
        public BuildingPart GetOffsetBuildingPart(SnappingPointSettings offsetSettings)
        {
            BuildingPart buildingPart;

            if (offsetSettings.MatchBy == SnappingPointSettings.MatchType.BUILDING_PART_TYPE)
            {
                buildingPart = BuildingManager.Instance.GetBuildingPartByCategory(offsetSettings.Type);

                if (buildingPart == null)
                {
                    Debug.LogWarning("<b>Easy Build System</b> : Building Part with the category <b>" +
                        offsetSettings.Type + "</b> doesn't not exist in the Building Manager.");

                    return null;
                }
            }
            else
            {
                if (offsetSettings.BuildingPart == null)
                {
                    return null;
                }

                buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(offsetSettings.BuildingPart.GetGeneralSettings.Identifier);

                if (buildingPart == null)
                {
                    Debug.LogError("<b>Easy Build System</b> : Building Part with the name <b>" +
                        offsetSettings.BuildingPart.GetGeneralSettings.Name + "</b> doesn't not exist in the Building Manager.");

                    return null;
                }
            }

            return buildingPart;
        }

        /// <summary>
        /// Get the snapping point which can fit.
        /// </summary>
        public SnappingPointSettings GetOffset(BuildingPart part)
        {
            for (int i = 0; i < m_SnappingPoints.Count; i++)
            {
                if (m_SnappingPoints[i].MatchBy == SnappingPointSettings.MatchType.BUILDING_PART_REFERENCE)
                {
                    if (m_SnappingPoints[i].BuildingPart != null)
                    {
                        if (m_SnappingPoints[i].BuildingPart.GetGeneralSettings.Identifier == part.GetGeneralSettings.Identifier)
                        {
                            return m_SnappingPoints[i];
                        }
                    }
                }
                else if (m_SnappingPoints[i].MatchBy == SnappingPointSettings.MatchType.BUILDING_PART_TYPE)
                {
                    if (m_SnappingPoints[i].Type == part.GetGeneralSettings.Type)
                    {
                        return m_SnappingPoints[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Snap a preview on a snapping point settings.
        /// </summary>
        public bool Snap(BuildingPart buildingPart, SnappingPointSettings offsetSettings, Vector3 offsetRotation)
        {
            if (buildingPart == null)
            {
                return false;
            }

            buildingPart.transform.SetPositionAndRotation(transform.TransformPoint(offsetSettings.Position), transform.rotation *
                (buildingPart.GetPreviewSettings.CanRotateOnSocket ? Quaternion.Euler(offsetSettings.Rotation + offsetRotation) :
                Quaternion.Euler(offsetSettings.Rotation)));

            if (offsetSettings.Scale != Vector3.one)
            {
                buildingPart.transform.localScale = offsetSettings.Scale * 1.0001f;
            }
            else
            {
                buildingPart.transform.localScale = transform.parent != null ?
                    transform.parent.localScale * 1.0001f : transform.localScale * 1.0001f;
            }

            return true;
        }

        /// <summary>
        /// Check if this Building Part can fit with the snapping point.
        /// </summary>
        public bool IsFitting(BuildingPart buildingPart)
        {
            if (!IsEnabled())
            {
                return false;
            }

            if (buildingPart == null)
            {
                return false;
            }

            for (int i = 0; i < m_SnappingPoints.Count; i++)
            {
                if (m_SnappingPoints[i] != null)
                {
                    return GetOffset(buildingPart) != null;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the socket is busy.
        /// </summary>
        public bool IsBusy(BuildingPart part)
        {
            BuildingPart[] buildingParts = PhysicsExtension.GetNeighborsType<BuildingPart>(transform.position,
                    Vector3.one * 0.25f, transform.rotation, Physics.AllLayers);

            for (int i = 0; i < buildingParts.Length; i++)
            {
                if (buildingParts[i] != null && buildingParts[i] != GetBuildingPart && 
                    buildingParts[i].GetGeneralSettings.Identifier == part.GetGeneralSettings.Identifier)
                {
                    SocketBusy = true;
                    return true;
                }
            }

            SocketBusy = false;

            return false;
        }

        #endregion
    }
}