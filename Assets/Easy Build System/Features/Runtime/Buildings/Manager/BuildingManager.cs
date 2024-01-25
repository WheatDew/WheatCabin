/// <summary>
/// Project : Easy Build System
/// Class : BuildingManager.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Bases;
using EasyBuildSystem.Features.Runtime.Bases.Drawers;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Area;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Group;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager
{
    [ExecuteInEditMode]
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-manager")]
    [DefaultExecutionOrder(-999)]
    public class BuildingManager : Singleton<BuildingManager>
    {
        #region Fields

        [SerializeField] List<BuildingPart> m_BuildingPartReferences = new List<BuildingPart>();
        public List<BuildingPart> BuildingPartReferences { get { return m_BuildingPartReferences; } set { m_BuildingPartReferences = value; } }

        [SerializeField] List<string> m_AllSurfaces = new List<string>();
        public List<string> AllSurfaces { get { return m_AllSurfaces; } set { m_AllSurfaces = value; } }

        [Serializable]
        public class AreaOfInterestSettings
        {
            [SerializeField] bool m_AreaOfInterest;
            public bool AreaOfInterest { get { return m_AreaOfInterest; } }

            [SerializeField, DontDrawIf("m_AreaOfInterest", true)] bool m_AffectBuildingAreas = true;
            public bool AffectBuildingAreas { get { return m_AffectBuildingAreas; } }

            [SerializeField, DontDrawIf("m_AreaOfInterest", true)] bool m_AffectBuildingSockets = true;
            public bool AffectBuildingSockets { get { return m_AffectBuildingSockets; } }

            [SerializeField, DontDrawIf("m_AreaOfInterest", true)] float m_RefreshInterval = 0.5f;
            public float RefreshInterval { get { return m_RefreshInterval; } }
        }
        [SerializeField] AreaOfInterestSettings m_AreaOfInterestSettings = new AreaOfInterestSettings();
        public AreaOfInterestSettings GetAreaOfInterestSettings { get { return m_AreaOfInterestSettings; } }

        [Serializable]
        public class BuildingBatchingSettings
        {
            [SerializeField] bool m_UseBuildingBatching = true;
            public bool UseDynamicBatching { get { return m_UseBuildingBatching; } }
        }
        [SerializeField] BuildingBatchingSettings m_BuildingBatchingSettings = new BuildingBatchingSettings();
        public BuildingBatchingSettings GetBuildingBatchingSettings { get { return m_BuildingBatchingSettings; } }

        List<BuildingArea> m_RegisteredBuildingAreas = new List<BuildingArea>();
        public List<BuildingArea> RegisteredBuildingAreas { get { return m_RegisteredBuildingAreas; } private set { m_RegisteredBuildingAreas = value; } }

        List<BuildingPart> m_RegisteredBuildingParts = new List<BuildingPart>();
        public List<BuildingPart> RegisteredBuildingParts { get { return m_RegisteredBuildingParts; } private set { m_RegisteredBuildingParts = value; } }

        List<BuildingSocket> m_RegisteredBuildingSockets = new List<BuildingSocket>();
        public List<BuildingSocket> RegisteredBuildingSockets { get { return m_RegisteredBuildingSockets; } private set { m_RegisteredBuildingSockets = value; } }

        List<BuildingGroup> m_RegisteredBuildingGroups = new List<BuildingGroup>();
        public List<BuildingGroup> RegisteredBuildingGroups { get { return m_RegisteredBuildingGroups; } private set { m_RegisteredBuildingGroups = value; } }

        #region Events

        /// <summary>
        /// Called when a Building Part is placed.
        /// </summary>
        [Serializable] public class PlacingBuildingPartEvent : UnityEvent<BuildingPart> { }
        public PlacingBuildingPartEvent OnPlacingBuildingPartEvent = new PlacingBuildingPartEvent();

        /// <summary>
        /// Called when a Building Part is destroyed.
        /// </summary>
        [Serializable] public class DestroyingBuildingPartEvent : UnityEvent<BuildingPart> { }
        public DestroyingBuildingPartEvent OnDestroyingBuildingPartEvent = new DestroyingBuildingPartEvent();

        /// <summary>
        /// Called when a Building Area is registered.
        /// </summary>
        [Serializable] public class RegisterBuildingAreaEvent : UnityEvent<BuildingArea> { }
        public RegisterBuildingAreaEvent OnRegisterBuildingAreaEvent = new RegisterBuildingAreaEvent();

        /// <summary>
        /// Called when a Building Area is unregistered.
        /// </summary>
        [Serializable] public class UnregisterBuildinAreaEvent : UnityEvent { }
        public UnregisterBuildinAreaEvent OnUnregisterBuildingAreaEvent = new UnregisterBuildinAreaEvent();

        /// <summary>
        /// Called when a Building Part is registered.
        /// </summary>
        [Serializable] public class RegisterBuildingPartEvent : UnityEvent<BuildingPart> { }
        public RegisterBuildingPartEvent OnRegisterBuildingPartEvent = new RegisterBuildingPartEvent();

        /// <summary>
        /// Called when a Building Part is unregistered.
        /// </summary>
        [Serializable] public class UnregisterBuildingPartEvent : UnityEvent { }
        public UnregisterBuildingPartEvent OnUnregisterBuildingPartEvent = new UnregisterBuildingPartEvent();

        /// <summary>
        /// Called when a Building Socket is registered.
        /// </summary>
        [Serializable] public class RegisterBuildingSocketEvent : UnityEvent<BuildingSocket> { }
        public RegisterBuildingSocketEvent OnRegisterBuildingSocketEvent = new RegisterBuildingSocketEvent();

        /// <summary>
        /// Called when a Building Socket is unregistered.
        /// </summary>
        [Serializable] public class UnregisterBuildingSocketEvent : UnityEvent { }
        public UnregisterBuildingSocketEvent OnUnregisterBuildingSocketEvent = new UnregisterBuildingSocketEvent();

        /// <summary>
        /// Called when a Building Group is registered.
        /// </summary>
        [Serializable] public class RegisterBuildingGroupEvent : UnityEvent<BuildingGroup> { }
        public RegisterBuildingGroupEvent OnRegisterBuildingGroupEvent = new RegisterBuildingGroupEvent();

        /// <summary>
        /// Called when a Building Group is unregistered.
        /// </summary>
        [Serializable] public class UnregisterBuildingGroupEvent : UnityEvent { }
        public UnregisterBuildingGroupEvent OnUnregisterBuildingGroupEvent = new UnregisterBuildingGroupEvent();

        #endregion

        #endregion

        #region Unity Methods

        void Awake()
        {
            if (m_AreaOfInterestSettings.AreaOfInterest)
            {
                InvokeRepeating(nameof(UpdateAreaOfInterest),
                    m_AreaOfInterestSettings.RefreshInterval, m_AreaOfInterestSettings.RefreshInterval);
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_BuildingBatchingSettings.UseDynamicBatching)
            {
                UpdateBuildingBatching();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Update Area Of Interest.
        /// </summary>
        void UpdateAreaOfInterest()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            if (m_AreaOfInterestSettings.AffectBuildingAreas)
            {
                for (int i = 0; i < RegisteredBuildingAreas.Count; i++)
                {
                    if (RegisteredBuildingAreas[i] != null)
                    {
                        RegisteredBuildingAreas[i].gameObject.SetActive((Vector3.Distance(BuildingPlacer.Instance.transform.position,
                            RegisteredBuildingAreas[i].transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance));
                    }
                }
            }

            if (m_AreaOfInterestSettings.AffectBuildingSockets)
            {
                for (int i = 0; i < RegisteredBuildingSockets.Count; i++)
                {
                    if (RegisteredBuildingSockets[i] != null)
                    {
                        RegisteredBuildingSockets[i].gameObject.SetActive((Vector3.Distance(BuildingPlacer.Instance.transform.position,
                            RegisteredBuildingSockets[i].transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance));
                    }
                }
            }
        }

        /// <summary>
        /// Update the Building Batching.
        /// </summary>
        void UpdateBuildingBatching()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            for (int i = 0; i < m_RegisteredBuildingGroups.Count; i++)
            {
                if (m_RegisteredBuildingGroups[i] != null)
                {
                    if (Vector3.Distance(m_RegisteredBuildingGroups[i].transform.position,
                        BuildingPlacer.Instance.transform.position) <= BuildingPlacer.Instance.GetRaycastSettings.Distance * 1.5f)
                    {
                        m_RegisteredBuildingGroups[i].UnbatchBuildingGroup();
                    }
                    else
                    {
                        m_RegisteredBuildingGroups[i].BatchBuildingGroup();
                    }
                }
            }
        }

        #region Building Area

        /// <summary>
        /// Get the closest Building Area.
        /// </summary>
        public BuildingArea GetClosestBuildingArea(Vector3 position)
        {
            for (int i = 0; i < m_RegisteredBuildingAreas.Count; i++)
            {
                if (m_RegisteredBuildingAreas[i] != null)
                {
                    if (m_RegisteredBuildingAreas[i].gameObject.activeSelf == true)
                    {
                        if (m_RegisteredBuildingAreas[i].Shape == BuildingArea.ShapeType.BOUNDS)
                        {
                            if (m_RegisteredBuildingAreas[i].transform.GetWorldBounds(m_RegisteredBuildingAreas[i].Bounds).Contains(position))
                            {
                                return m_RegisteredBuildingAreas[i];
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(position, m_RegisteredBuildingAreas[i].transform.position) <= m_RegisteredBuildingAreas[i].Radius)
                            {
                                return m_RegisteredBuildingAreas[i];
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Register the Building Area.
        /// </summary>
        public void RegisterBuildingArea(BuildingArea area)
        {
            if (area == null)
            {
                return;
            }

            if (RegisteredBuildingAreas.Contains(area))
            {
                return;
            }

            RegisteredBuildingAreas.Add(area);
            OnRegisterBuildingAreaEvent.Invoke(area);
        }

        /// <summary>
        /// Unregister the Building Area.
        /// </summary>
        public void UnregisterBuildingArea(BuildingArea area)
        {
            if (area == null)
            {
                return;
            }

            RegisteredBuildingAreas.Remove(area);

            OnUnregisterBuildingAreaEvent.Invoke();
        }

        #endregion

        #region Building Part

        /// <summary>
        /// Place a new Building Part.
        /// </summary>
        public BuildingPart PlaceBuildingPart(BuildingPart buildingPart, Vector3 position, Vector3 rotation, Vector3 scale, bool createNewGroup = true)
        {
            if (buildingPart == null)
            {
                return null;
            }

            BuildingPart instancedBuildingPart = Instantiate(buildingPart.gameObject,
                position, Quaternion.Euler(rotation)).GetComponent<BuildingPart>();

            instancedBuildingPart.transform.localScale = scale;

            instancedBuildingPart.ChangeState(BuildingPart.StateType.PLACED);

            BuildingArea closestArea = GetClosestBuildingArea(position);

            if (closestArea != null)
            {
                closestArea.RegisterBuildingPart(instancedBuildingPart);
            }

            if (createNewGroup != false)
            {
                BuildingGroup closestGroup = GetClosestBuildingGroup(instancedBuildingPart);

                if (closestGroup != null)
                {
                    closestGroup.RegisterBuildingPart(instancedBuildingPart);
                }
                else
                {
                    BuildingGroup instancedGroup = CreateBuildingGroup(instancedBuildingPart);
                    instancedGroup.RegisterBuildingPart(instancedBuildingPart);
                }
            }

            OnPlacingBuildingPartEvent.Invoke(instancedBuildingPart);

            return instancedBuildingPart;
        }

        /// <summary>
        /// Destroy a Building Part.
        /// </summary>
        public void DestroyBuildingPart(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(buildingPart.gameObject);
            }
            else
            {
                DestroyImmediate(buildingPart.gameObject);
            }
        }

        /// <summary>
        /// Get a Building Part by identifier.
        /// </summary>
        public BuildingPart GetBuildingPartByIdentifier(string identifier)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Identifier == identifier)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a Building Part by name.
        /// </summary>
        public BuildingPart GetBuildingPartByName(string name)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Name == name)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a Building Part by type.
        /// </summary>
        public BuildingPart GetBuildingPartByCategory(string type)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (m_BuildingPartReferences[i].GetGeneralSettings.Type == type)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a Building Part by index.
        /// </summary>
        public BuildingPart GetBuildingPartByIndex(int index)
        {
            for (int i = 0; i < m_BuildingPartReferences.Count; i++)
            {
                if (m_BuildingPartReferences[i] != null)
                {
                    if (i == index)
                    {
                        return m_BuildingPartReferences[i];
                    }
                }
            }

            Debug.LogWarning("<b>Easy Build System</b> : Could not find the Building Part with index: " + index + "");

            return null;
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

            if (RegisteredBuildingParts.Contains(buildingPart))
            {
                return;
            }

            RegisteredBuildingParts.Add(buildingPart);
            OnRegisterBuildingPartEvent.Invoke(buildingPart);
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

            RegisteredBuildingParts.Remove(buildingPart);
            OnUnregisterBuildingPartEvent.Invoke();
        }

        #endregion

        #region Building Socket

        /// <summary>
        /// Register the Building Socket.
        /// </summary>
        public void RegisterBuildingSocket(BuildingSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            if (RegisteredBuildingSockets.Contains(socket))
            {
                return;
            }

            RegisteredBuildingSockets.Add(socket);
            OnRegisterBuildingSocketEvent.Invoke(socket);
        }

        /// <summary>
        /// Unegister the Building Socket.
        /// </summary>
        public void UnregisterBuildingSocket(BuildingSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            RegisteredBuildingSockets.Remove(socket);
            OnUnregisterBuildingSocketEvent.Invoke();
        }

        #endregion

        #region Building Group 

        /// <summary>
        /// Create a new Building Group.
        /// </summary>
        public BuildingGroup CreateBuildingGroup(BuildingPart buildingPart)
        {
            BuildingGroup instancedGroup =
                new GameObject("Building Group (" + buildingPart.GetInstanceID() + ")").AddComponent<BuildingGroup>();

            instancedGroup.Identifier = buildingPart.GetInstanceID().ToString();

            instancedGroup.transform.position = buildingPart.transform.position;

            return instancedGroup;
        }

        /// <summary>
        /// Get the closest Building Group.
        /// </summary>
        public BuildingGroup GetClosestBuildingGroup(BuildingPart buildingPart)
        {
            for (int i = 0; i < RegisteredBuildingGroups.Count; i++)
            {
                if (RegisteredBuildingGroups[i] != null)
                {
                    if (Vector3.Distance(buildingPart.transform.position, 
                        RegisteredBuildingGroups[i].transform.position) < 128)
                    {
                        return RegisteredBuildingGroups[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Register the Building Group.
        /// </summary>
        public void RegisterBuildingGroup(BuildingGroup group)
        {
            if (group == null)
            {
                return;
            }

            if (RegisteredBuildingGroups.Contains(group))
            {
                return;
            }

            RegisteredBuildingGroups.Add(group);
            OnRegisterBuildingGroupEvent.Invoke(group);
        }

        /// <summary>
        /// Unregister the Building Group.
        /// </summary>
        public void UnregisterBuildingGroup(BuildingGroup group)
        {
            if (group == null)
            {
                return;
            }

            RegisteredBuildingGroups.Remove(group);
            OnUnregisterBuildingGroupEvent.Invoke();
        }

        #endregion

        #endregion
    }
}