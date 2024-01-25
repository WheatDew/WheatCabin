/// <summary>
/// Project : Easy Build System
/// Class : BuildingPart.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

using EasyBuildSystem.Features.Runtime.Bases.Drawers;

using EasyBuildSystem.Features.Runtime.Buildings.Area;
using EasyBuildSystem.Features.Runtime.Buildings.Group;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

using EasyBuildSystem.Features.Runtime.Extensions;

public class BuildingTypeAttribute : PropertyAttribute { }

namespace EasyBuildSystem.Features.Runtime.Buildings.Part
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-part")]
    public class BuildingPart : MonoBehaviour
    {
        #region Fields

        public enum StateType { NONE, PREVIEW, DESTROY, EDIT, PLACED, QUEUE }

        [Serializable]
        public class GeneralSettings
        {
            [SerializeField] string m_Name = "New Building Part";
            public string Name { get { return m_Name; } set { m_Name = value; } }

            [SerializeField, BuildingType] string m_Type;
            public string Type { get { return m_Type; } set { m_Type = value; } }

            [SerializeField] Texture2D m_Thumbnail;
            public Texture2D Thumbnail { get { return m_Thumbnail; } set { m_Thumbnail = value; } }

            [SerializeField] string m_Identifier;
            public string Identifier { get { return m_Identifier; } set { m_Identifier = value; } }
        }
        [SerializeField] GeneralSettings m_GeneralSettings = new GeneralSettings();
        public GeneralSettings GetGeneralSettings { get { return m_GeneralSettings; } }

        [Serializable]
        public class ModelSettings
        {
            [SerializeField] List<GameObject> m_Models = new List<GameObject>();
            public List<GameObject> Models { get { return m_Models; } set { m_Models = value; } }

            [SerializeField] int m_ModelIndex = 0;
            public int ModelIndex { get { return m_ModelIndex; } set { m_ModelIndex = value; } }

            [SerializeField] Bounds m_ModelBounds;
            public Bounds ModelBounds { get { return m_ModelBounds; } set { m_ModelBounds = value; } }

            public GameObject GetModel 
            {
                get 
                { 
                    if (m_Models == null)
                    {
                        m_Models = new List<GameObject>();
                    }

                    if (m_Models.Count == 0)
                    {
                        return null;
                    }

                    if (m_ModelIndex > m_Models.Count - 1)
                    {
                        m_ModelIndex = 0;
                    }

                    return m_Models[m_ModelIndex];
                }
            }
        }
        [SerializeField] ModelSettings m_ModelSettings = new ModelSettings();
        public ModelSettings GetModelSettings { get { return m_ModelSettings; } }

        [Serializable]
        public class PreviewSettings
        {
            [SerializeField] bool m_Indicator = false;
            public bool Indicator { get { return m_Indicator; } }

            [SerializeField] IndicatorSettings m_IndicatorSettings;
            public IndicatorSettings GetIndicatorSettings { get { return m_IndicatorSettings; } }

            [Serializable]
            public class IndicatorSettings
            {
                [SerializeField] GameObject m_Object;
                public GameObject Object { get { return m_Object; } set { m_Object = value; } }

                [SerializeField] Color m_Color = new Color(1, 0.5f, 0f, 1f);
                public Color IndicatorColor { get { return m_Color; } }

                [SerializeField] Vector3 m_OffsetPosition;
                public Vector3 OffsetPosition { get { return m_OffsetPosition; } }

                [SerializeField] Vector3 m_OffsetRotation;
                public Vector3 OffsetRotation { get { return m_OffsetRotation; } }

                [SerializeField] Vector3 m_OffsetScale = new Vector3(1, 1, 1);
                public Vector3 OffsetScale { get { return m_OffsetScale; } }
            }

            [SerializeField] Vector3 m_RotateAxis = new Vector3(0, 45f, 0);
            public Vector3 RotateAxis { get { return m_RotateAxis; } set { m_RotateAxis = value; } }

            [SerializeField] bool m_RotateAccordingAngle;
            public bool RotateAccordingAngle { get { return m_RotateAccordingAngle; } set { m_RotateAccordingAngle = value; } }

            public enum Axis
            {
                Forward,
                Backward,
                Left,
                Right,
                Up,
                Down
            }

            [SerializeField] Axis m_RotateAccordingAxis;
            public Axis RotateAccordingAxis { get { return m_RotateAccordingAxis; } set { m_RotateAccordingAxis = value; } }

            [SerializeField] bool m_CanRotateOnSocket;
            public bool CanRotateOnSocket { get { return m_CanRotateOnSocket; } set { m_CanRotateOnSocket = value; } }

            [SerializeField] Vector3 m_OffsetPosition;
            public Vector3 OffsetPosition { get { return m_OffsetPosition; } set { m_OffsetPosition = value; } }

            [SerializeField] bool m_ClampRotation;
            public bool ClampRotation { get { return m_ClampRotation; } set { m_ClampRotation = value; } }

            [SerializeField, DontDrawIf("m_ClampRotation", true)] Vector3 m_ClampMinRotation;
            public Vector3 ClampMinRotation { get { return m_ClampMinRotation; } set { m_ClampMinRotation = value; } }

            [SerializeField, DontDrawIf("m_ClampRotation", true)] Vector3 m_ClampMaxRotation;
            public Vector3 ClampMaxRotation { get { return m_ClampMaxRotation; } set { m_ClampMaxRotation = value; } }

            [SerializeField] bool m_ClampPosition;
            public bool ClampPosition { get { return m_ClampPosition; } set { m_ClampPosition = value; } }

            [SerializeField, DontDrawIf("m_ClampPosition", true)] Vector3 m_ClampMinPosition;
            public Vector3 ClampMinPosition { get { return m_ClampMinPosition; } set { m_ClampMinPosition = value; } }

            [SerializeField, DontDrawIf("m_ClampPosition", true)] Vector3 m_ClampMaxPosition;
            public Vector3 ClampMaxPosition { get { return m_ClampMaxPosition; } set { m_ClampMaxPosition = value; } }

            [SerializeField] bool m_CanMovingIfPlaceable;
            public bool CanMovingIfPlaceable { get { return m_CanMovingIfPlaceable; } set { m_CanMovingIfPlaceable = value; } }

            [SerializeField] GameObject[] m_DisableGameObjects;
            public GameObject[] DisableGameObjects { get { return m_DisableGameObjects; } set { m_DisableGameObjects = value; } }

            [SerializeField] MonoBehaviour[] m_DisableMonoBehaviours;
            public MonoBehaviour[] DisableMonoBehaviours { get { return m_DisableMonoBehaviours; } set { m_DisableMonoBehaviours = value; } }

            [SerializeField] Material m_Material;
            public Material Material { get { return m_Material; } set { m_Material = value; } }

            [SerializeField] List<Renderer> m_IgnoreRenderers = new List<Renderer>();
            public List<Renderer> IgnoreRenderers { get { return m_IgnoreRenderers; } set { m_IgnoreRenderers = value; } }

            [SerializeField] Color m_PlacingColor = new Color(0, 1, 0, 0.2f);
            public Color PlacingColor { get { return m_PlacingColor; } }

            [SerializeField] Color m_EditingColor = new Color(0, 1, 1, 0.2f);
            public Color EditingColor { get { return m_EditingColor; } }

            [SerializeField] Color m_DestroyingColor = new Color(1, 0, 0, 0.2f);
            public Color DestroyingColor { get { return m_DestroyingColor; } }
        }

        [SerializeField] PreviewSettings m_PreviewSettings = new PreviewSettings();
        public PreviewSettings GetPreviewSettings { get { return m_PreviewSettings; } }

        [Serializable]
        public class SaveSettings
        {
            [SerializeField, DontDrawIf("m_Identifier", true, DontDrawIfAttribute.DisablingType.READ_ONLY)] string m_Identifier;
            public string Identifier { get { return m_Identifier; } set { m_Identifier = value; } }

            [SerializeField, DontDrawIf("m_Name", true, DontDrawIfAttribute.DisablingType.READ_ONLY)] string m_Name;
            public string Name { get { return m_Name; } set { m_Name = value; } }

            [SerializeField, DontDrawIf("m_Position", true, DontDrawIfAttribute.DisablingType.READ_ONLY)] Vector3 m_Position;
            public Vector3 Position { get { return m_Position; } set { m_Position = value; } }

            [SerializeField, DontDrawIf("m_Rotation", true, DontDrawIfAttribute.DisablingType.READ_ONLY)] Vector3 m_Rotation;
            public Vector3 Rotation { get { return m_Rotation; } set { m_Rotation = value; } }

            [SerializeField, DontDrawIf("m_Scale", true, DontDrawIfAttribute.DisablingType.READ_ONLY)] Vector3 m_Scale;
            public Vector3 Scale { get { return m_Scale; } set { m_Scale = value; } }
        }

        [SerializeField] StateType m_State = StateType.PLACED;
        public StateType State { get { return m_State; } set { m_State = value; } }

        public BuildingArea AttachedBuildingArea { get; set; }

        public BuildingGroup AttachedBuildingGroup { get; set; }

        public BuildingSocket AttachedBuildingSocket { get; set; }

        BuildingSocket[] m_Sockets;
        public BuildingSocket[] Sockets
        {
            get
            {
                if (m_Sockets == null || m_Sockets.Length == 0)
                {
                    m_Sockets = GetComponentsInChildren<BuildingSocket>();
                }

                return m_Sockets;
            }
        }

        List<Collider> m_Colliders;
        public List<Collider> Colliders
        {
            get
            {
                if (m_Colliders == null || m_Colliders.Count == 0)
                {
                    m_Colliders = GetComponentsInParent<Collider>(true).ToList();
                    m_Colliders.AddRange(GetComponentsInChildren<Collider>(true).ToList());
                }

                return m_Colliders;
            }

            set
            {
                m_Colliders = null;
            }
        }

        List<Rigidbody> m_Rigidbodies;
        public List<Rigidbody> Rigidbodies
        {
            get
            {
                if (m_Rigidbodies == null || m_Rigidbodies.Count == 0)
                {
                    m_Rigidbodies = GetComponentsInChildren<Rigidbody>(true).ToList();
                }

                return m_Rigidbodies;
            }

            set
            {
                m_Rigidbodies = null;
            }
        }

        List<Renderer> m_Renderers;
        public List<Renderer> Renderers
        {
            get
            {
                if (m_Renderers == null || m_Renderers.Count == 0)
                {
                    m_Renderers = GetComponentsInChildren<Renderer>(true).ToList();
                }

                for (int i = 0; i < m_Renderers.Count; i++)
                {
                    if (m_Renderers[i] != null)
                    {
                        if (m_PreviewSettings.IgnoreRenderers != null &&
                            !m_PreviewSettings.IgnoreRenderers.Contains(m_Renderers[i]))
                        {
                            if (!m_DefaultMaterials.ContainsKey(m_Renderers[i]))
                            {
                                m_DefaultMaterials.Add(m_Renderers[i], m_Renderers[i].sharedMaterials);
                            }
                        }
                    }
                }

                return m_Renderers;
            }

            set
            {
                m_Renderers = value;
            }
        }

        List<BuildingCondition> m_Conditions;
        public List<BuildingCondition> Conditions
        {
            get
            {
                if (m_Conditions == null)
                {
                    m_Conditions = GetComponents<BuildingCondition>().ToList();
                }

                return m_Conditions;
            }
        }

        BuildingBasicsCondition m_BuildingBasicsCondition;
        public BuildingBasicsCondition TryGetBasicsCondition
        {
            get
            {
                if (m_BuildingBasicsCondition == null)
                {
                    m_BuildingBasicsCondition = GetComponent<BuildingBasicsCondition>();
                }

                return m_BuildingBasicsCondition;
            }
        }

        BuildingCollisionCondition m_BuildingCollisionCondition;
        public BuildingCollisionCondition TryGetCollisonCondition
        {
            get
            {
                if (m_BuildingCollisionCondition == null)
                {
                    m_BuildingCollisionCondition = GetComponent<BuildingCollisionCondition>();
                }

                return m_BuildingCollisionCondition;
            }
        }

        BuildingPhysicsCondition m_BuildingPhysicsCondition;
        public BuildingPhysicsCondition TryGetPhysicsCondition
        {
            get
            {
                if (m_BuildingPhysicsCondition == null)
                {
                    m_BuildingPhysicsCondition = GetComponent<BuildingPhysicsCondition>();
                }

                return m_BuildingPhysicsCondition;
            }
        }

        Dictionary<Renderer, Material[]> m_DefaultMaterials = new Dictionary<Renderer, Material[]>();
        public Dictionary<Renderer, Material[]> DefaultMaterials
        {
            get
            {
                return m_DefaultMaterials;
            }
        }

        [SerializeField] GameObject m_InstancedIndicatorObject;
        public GameObject InstancedIndicatorObject { get { return m_InstancedIndicatorObject; } }

        bool m_IsQuitting;

        /// <summary>
        /// Called when the state of the Building Part is changed.
        /// </summary>
        [Serializable]
        public class ChangedStateEvent : UnityEvent<StateType>
        {
            internal void AddListener(ChangedStateEvent onChangedStateEvent)
            {
                throw new NotImplementedException();
            }
        }
        public ChangedStateEvent OnChangedStateEvent = new ChangedStateEvent();

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.RegisterBuildingPart(this);
            }

            m_Colliders = null;
            m_State = StateType.NONE;
        }

        void OnDisable()
        {
            HidePreviewIndicator();
        }

        void Awake()
        {
            if (m_PreviewSettings.Material != null)
            {
                m_PreviewSettings.Material = new Material(m_PreviewSettings.Material);
                List<Renderer> _ = Renderers;
            }

            AttachedBuildingArea = null;
            AttachedBuildingGroup = null;
            AttachedBuildingSocket = null;
        }

        public void OnValidate()
        {
#if UNITY_EDITOR
            if (Renderers != null && Renderers.Count != 0)
            {
                Renderer renderer = Renderers[0];

                if (renderer != null && renderer.name.Contains("Indicator"))
                {
                    GetModelSettings.Models.Add(GetComponentsInChildren<Renderer>()[0].gameObject);
                }
            }
#endif

            if (GetGeneralSettings.Identifier == string.Empty)
            {
                GetGeneralSettings.Identifier = Guid.NewGuid().ToString("N");
            }

            if (!Application.isPlaying)
            {
                GetModelSettings.Models = GetModelSettings.Models.Where(s => s != null).Distinct().ToList();

                for (int i = 0; i < GetModelSettings.Models.Count; i++)
                {
                    if (i == GetModelSettings.ModelIndex)
                    {
                        GetModelSettings.Models[i].SetActive(true);
                    }
                    else
                    {
                        GetModelSettings.Models[i].SetActive(false);
                    }
                }
            } 

            if (m_PreviewSettings.Material == null)
            {
                m_PreviewSettings.Material = Resources.Load<Material>("Materials/Default_Preview");
            }
            else
            {
                if (GraphicsSettings.currentRenderPipeline)
                {
                    m_PreviewSettings.Material.SetColor("_BaseColor", new Color(1, 1, 1, m_PreviewSettings.Material.color.a));
                }
                else
                {
                    m_PreviewSettings.Material.SetColor("_Color", new Color(1, 1, 1, m_PreviewSettings.Material.color.a));
                }
            }

            if (m_PreviewSettings.Indicator)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () =>
                {
                    if (m_PreviewSettings.GetIndicatorSettings.Object == null)
                    {
                        m_PreviewSettings.GetIndicatorSettings.Object = Resources.Load<GameObject>("Prefabs/Default_Preview_Indicator");
                    }

                    if (m_InstancedIndicatorObject != null)
                    {
                        MeshRenderer indicatorRenderer = m_InstancedIndicatorObject.GetComponent<MeshRenderer>();

                        if (indicatorRenderer)
                        {
                            if (GraphicsSettings.currentRenderPipeline)
                            {
                                indicatorRenderer.sharedMaterial.SetColor("_BaseColor", m_PreviewSettings.GetIndicatorSettings.IndicatorColor);
                            }
                            else
                            {
                                indicatorRenderer.sharedMaterial.SetColor("_Color", m_PreviewSettings.GetIndicatorSettings.IndicatorColor);
                            }

                            if (!m_PreviewSettings.IgnoreRenderers.Contains(indicatorRenderer))
                            {
                                m_PreviewSettings.IgnoreRenderers.Add(indicatorRenderer);
                            }
                        }

                        m_InstancedIndicatorObject.transform.localPosition =
                            m_PreviewSettings.GetIndicatorSettings.OffsetPosition;

                        m_InstancedIndicatorObject.transform.localEulerAngles =
                            m_PreviewSettings.GetIndicatorSettings.OffsetRotation;

                        m_InstancedIndicatorObject.transform.localScale =
                            m_PreviewSettings.GetIndicatorSettings.OffsetScale;

                        m_InstancedIndicatorObject.name = "Preview_Indicator";
                    }
                };
#endif
            }
            else
            {
                HidePreviewIndicator();
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

            if (AttachedBuildingArea != null)
            {
                AttachedBuildingArea.UnregisterBuildingPart(this);
            }

            if (AttachedBuildingGroup != null)
            {
                AttachedBuildingGroup.UnregisterBuildingPart(this);
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.UnregisterBuildingPart(this);
                BuildingManager.Instance.OnDestroyingBuildingPartEvent.Invoke(this);
            }
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.cyan;
            GizmosExtension.DrawArrow(transform.position, transform.forward);

            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, Vector3.up, 0.1f);
#endif
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            bool canPlacing = CheckPlacingCondition();

            Gizmos.color = (canPlacing ? Color.cyan : Color.red);
            Gizmos.DrawWireCube(GetModelSettings.ModelBounds.center, GetModelSettings.ModelBounds.size * 1.001f);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Change the state of the Building Part.
        /// </summary>
        public virtual void ChangeState(StateType state)
        {
            if (m_InstancedIndicatorObject != null)
            {
                m_InstancedIndicatorObject.SetActive(state == StateType.PREVIEW);
            }

            if (Application.isPlaying)
            {
                if (m_State == state)
                {
                    return;
                }
            }

            m_State = state;

            if (state == StateType.PLACED)
            {
                RenderExtension.ChangeMaterialRecursively(Renderers.ToArray(), m_DefaultMaterials, m_PreviewSettings.IgnoreRenderers.ToArray());

                HidePreviewIndicator();

                EnableChildrenRigidbodies();
                EnableChildrenColliders();
                EnableChildrenSockets();
            }
            else if (state == StateType.PREVIEW)
            {
                RenderExtension.ChangeMaterialRecursively(Renderers.ToArray(), m_PreviewSettings.Material, m_PreviewSettings.IgnoreRenderers.ToArray());

                ShowPreviewIndicator();

                DisableChildrenRigidbodies();
                DisableChildrenColliders();
                DisableChildrenSockets();
            }
            else if (state == StateType.DESTROY)
            {
                RenderExtension.ChangeMaterialRecursively(Renderers.ToArray(), m_PreviewSettings.Material, m_PreviewSettings.IgnoreRenderers.ToArray());

                HidePreviewIndicator();

                DisableChildrenRigidbodies();
                EnableChildrenColliders();
                DisableChildrenSockets();
            }
            else if (state == StateType.EDIT)
            {
                RenderExtension.ChangeMaterialRecursively(Renderers.ToArray(), m_PreviewSettings.Material, m_PreviewSettings.IgnoreRenderers.ToArray());

                HidePreviewIndicator();

                DisableChildrenRigidbodies();
                EnableChildrenColliders();
                DisableChildrenSockets();
            }
            else if (state == StateType.QUEUE)
            {
                RenderExtension.ChangeMaterialColorRecursively(Renderers.ToArray(), m_PreviewSettings.PlacingColor, m_PreviewSettings.IgnoreRenderers.ToArray());
                RenderExtension.ChangeMaterialRecursively(Renderers.ToArray(), m_PreviewSettings.Material, m_PreviewSettings.IgnoreRenderers.ToArray());

                HidePreviewIndicator();

                DisableChildrenRigidbodies();
                EnableChildrenColliders();
                EnableChildrenSockets();
            }

            for (int i = 0; i < m_PreviewSettings.DisableGameObjects.Length; i++)
            {
                if (m_PreviewSettings.DisableGameObjects[i] != null)
                {
                    m_PreviewSettings.DisableGameObjects[i].SetActive(state != StateType.PREVIEW && state != StateType.QUEUE);
                }
            }

            for (int i = 0; i < m_PreviewSettings.DisableMonoBehaviours.Length; i++)
            {
                if (m_PreviewSettings.DisableMonoBehaviours[i] != null)
                {
                    m_PreviewSettings.DisableMonoBehaviours[i].enabled = state != StateType.PREVIEW && state != StateType.QUEUE;
                }
            }

            OnChangedStateEvent.Invoke(state);
        }

        /// <summary>
        /// Update materials color.
        /// </summary>
        public void UpdatePreviewMaterials(Color color)
        {
            RenderExtension.ChangeMaterialColorRecursively(Renderers.ToArray(), color, GetPreviewSettings.IgnoreRenderers.ToArray());
        }

        /// <summary>
        /// Update model bounds.
        /// </summary>
        public void UpdateModelBounds()
        {
            if (m_ModelSettings.Models == null)
            {
                return;
            }

            m_ModelSettings.ModelBounds = MathExtension.GetBounds(gameObject, m_PreviewSettings.IgnoreRenderers.ToArray());

            Debug.Log("<b>Easy Build System</b> : Building model bounds has been generated.");
        }

        /// <summary>
        /// Check the placing conditions.
        /// </summary>
        public bool CheckPlacingCondition()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckPlacingCondition())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check the destroy conditions.
        /// </summary>
        public bool CheckDestroyCondition()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckDestroyCondition())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check the editing conditions.
        /// </summary>
        public bool CheckEditingCondition()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckEditingCondition())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Enable all children's rigidbody.
        /// </summary>
        public void EnableChildrenRigidbodies()
        {
            for (int i = 0; i < Rigidbodies.Count; i++)
            {
                if (Rigidbodies[i] != null)
                {
                    Rigidbodies[i].isKinematic = false;
                }
            }
        }

        /// <summary>
        /// Disable all children's rigidbody.
        /// </summary>
        public void DisableChildrenRigidbodies()
        {
            for (int i = 0; i < Rigidbodies.Count; i++)
            {
                if (Rigidbodies[i] != null)
                {
                    Rigidbodies[i].isKinematic = transform;
                }
            }
        }

        /// <summary>
        /// Enable all children's collider.
        /// </summary>
        public void EnableChildrenColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                if (Colliders[i] != null)
                {
                    Colliders[i].enabled = true;
                }
            }
        }

        /// <summary>
        /// Disable all children's collider.
        /// </summary>
        public void DisableChildrenColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                if (Colliders[i] != null)
                {
                    Colliders[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// Enable all children's socket.
        /// </summary>
        public void EnableChildrenSockets()
        {
            for (int i = 0; i < Sockets.Length; i++)
            {
                Sockets[i].IsDisabled = false;
            }
        }

        /// <summary>
        /// Disable all children's socket.
        /// </summary>
        public void DisableChildrenSockets()
        {
            for (int i = 0; i < Sockets.Length; i++)
            {
                Sockets[i].IsDisabled = true;
            }
        }

        /// <summary>
        /// Show preview indicator.
        /// </summary>
        public void ShowPreviewIndicator()
        {
            if (m_PreviewSettings.GetIndicatorSettings.Object != null && m_InstancedIndicatorObject == null)
            {
                if (!Application.isPlaying)
                {
                    if (gameObject.scene.IsValid())
                    {
#if UNITY_EDITOR
                        EditorApplication.delayCall += () =>
                        {
                            m_InstancedIndicatorObject =
                                Instantiate(m_PreviewSettings.GetIndicatorSettings.Object);
                            m_InstancedIndicatorObject.transform.SetParent(transform);
                            m_InstancedIndicatorObject.transform.localPosition = Vector3.zero;
                            m_InstancedIndicatorObject.transform.localEulerAngles = Vector3.zero;
                            m_InstancedIndicatorObject.transform.localScale = Vector3.one;
                        };
#else
                        m_InstancedIndicatorObject =
                            Instantiate(m_PreviewSettings.GetIndicatorSettings.Object);
                        m_InstancedIndicatorObject.transform.SetParent(transform);
                        m_InstancedIndicatorObject.transform.localPosition = Vector3.zero;
                        m_InstancedIndicatorObject.transform.localEulerAngles = Vector3.zero;
                        m_InstancedIndicatorObject.transform.localScale = Vector3.one;
#endif
                    }
                }
                else
                {
                    m_InstancedIndicatorObject =
                        Instantiate(m_PreviewSettings.GetIndicatorSettings.Object);
                    m_InstancedIndicatorObject.transform.SetParent(transform);
                    m_InstancedIndicatorObject.transform.localPosition = Vector3.zero;
                    m_InstancedIndicatorObject.transform.localEulerAngles = Vector3.zero;
                    m_InstancedIndicatorObject.transform.localScale = Vector3.one;
                }

                OnValidate();
            }
        }

        /// <summary>
        /// Hide preview indicator.
        /// </summary>
        public void HidePreviewIndicator()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (m_InstancedIndicatorObject != null)
                {
                    m_PreviewSettings.IgnoreRenderers.Remove(m_InstancedIndicatorObject.GetComponent<MeshRenderer>());

                    DestroyImmediate(m_InstancedIndicatorObject, true);
                }
            };
#endif
        }

        /// <summary>
        /// Get the save data of the Building Part.
        /// </summary>
        public SaveSettings GetSaveData()
        {
            return new SaveSettings()
            {
                Identifier = m_GeneralSettings.Identifier,
                Name = m_GeneralSettings.Name,
                Position = transform.position,
                Rotation = transform.eulerAngles,
                Scale = transform.localScale
            };
        }

        #endregion
    }
}