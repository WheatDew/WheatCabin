/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacer.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Bases;
using EasyBuildSystem.Features.Runtime.Bases.Drawers;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

using Random = UnityEngine.Random;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-placer")]
    public class BuildingPlacer : Singleton<BuildingPlacer>
    {
        #region Fields

        public enum BuildMode { NONE, PLACE, DESTROY, EDIT }

        BuildMode m_LastBuildMode;
        public BuildMode LastBuildMode { get { return m_LastBuildMode; } }

        BuildMode m_BuildMode;
        public BuildMode GetBuildMode { get { return m_BuildMode; } }

        [SerializeField] BaseInputHandler m_InputHandler;
        public BaseInputHandler GetInputHandler
        {
            get
            {
                if (m_InputHandler == null)
                {
                    m_InputHandler = GetComponent<BaseInputHandler>();
                }

                return m_InputHandler;
            }

            set { m_InputHandler = value; }
        }

        [Serializable]
        public class RaycastSettings
        {
            public enum RaycastType
            {
                FIRST_PERSON_VIEW,
                THIRD_PERSON_VIEW,
                TOP_DOWN_VIEW
#if EBS_XR
                ,FROM_XR_INTERACTOR
#endif
            }

            [SerializeField] Camera m_Camera;
            public Camera Camera { get { return m_Camera; } set { m_Camera = value; } }

            [SerializeField] RaycastType m_ViewType;
            public RaycastType ViewType { get { return m_ViewType; } set { m_ViewType = value; } }

#if EBS_XR
            [SerializeField, DontDrawIf("m_ViewType", RaycastType.FROM_XR_INTERACTOR)] UnityEngine.XR.Interaction.Toolkit.XRRayInteractor m_RaycastFromXRInteractor;
            public UnityEngine.XR.Interaction.Toolkit.XRRayInteractor RaycastFromXRInteractor { get { return m_RaycastFromXRInteractor; } }
#endif

            [SerializeField, DontDrawIf("m_ViewType", RaycastType.THIRD_PERSON_VIEW)] Transform m_FromTransform;
            public Transform FromTransform { get { return m_FromTransform; } set { m_FromTransform = value; } }

            [SerializeField] LayerMask m_LayerMask = 1 << 0;
            public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

            [SerializeField] bool m_Through = false;
            public bool Through { get { return m_Through; } set { m_Through = value; } }

            [SerializeField] Vector3 m_OffsetPosition = new Vector3(0, 0, 0);
            public Vector3 OffsetPosition { get { return m_OffsetPosition; } }

            [SerializeField] float m_Distance = 10f;
            public float Distance { get { return m_Distance; } set { m_Distance = value; } }

            [SerializeField] float m_MaxDistance = 0f;
            public float MaxDistance { get { return m_MaxDistance; } }

            public virtual Ray GetRay()
            {
                if (m_Camera == null)
                {
                    Debug.LogWarning("<b>Easy Build System</b> : BuildingPlacer component require a camera to work properly!");
                    return new Ray();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (Event.current == null)
                    {
                        return new Ray();
                    }

                    Vector3 mousePosition = Event.current.mousePosition;

                    float perPixel = UnityEditor.EditorGUIUtility.pixelsPerPoint;
                    mousePosition.y = m_Camera.pixelHeight - mousePosition.y * perPixel;
                    mousePosition.x *= perPixel;

                    return Camera.current.ScreenPointToRay(mousePosition);
                }
#endif

                if (m_ViewType == RaycastType.TOP_DOWN_VIEW)
                {
#if EBS_INPUT_SYSTEM_SUPPORT
                    return m_Camera.ScreenPointToRay(new Vector3(UnityEngine.InputSystem.Mouse.current.position.ReadValue().x, 
                        UnityEngine.InputSystem.Mouse.current.position.ReadValue().y, 0f) + m_OffsetPosition);
#else
                    return m_Camera.ScreenPointToRay(Input.mousePosition + m_OffsetPosition);
#endif
                }
                else if (m_ViewType == RaycastType.FIRST_PERSON_VIEW)
                {
                    return new Ray(m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f) + m_OffsetPosition), m_Camera.transform.forward);
                }
                else if (m_ViewType == RaycastType.THIRD_PERSON_VIEW)
                {
                    if (m_FromTransform != null)
                    {
                        return new Ray(m_FromTransform.position + m_FromTransform.TransformDirection(m_OffsetPosition),
                            m_Camera.transform.forward);
                    }
                }
#if EBS_XR
                else if (m_ViewType == RaycastType.FROM_XR_INTERACTOR)
                {
                    return new Ray(m_RaycastFromXRInteractor.rayOriginTransform.position, m_RaycastFromXRInteractor.rayOriginTransform.forward);
                }
#endif

                return new Ray();
            }

            Transform _Caster;
            public virtual Transform GetCaster
            {
                get
                {
                    if (!Application.isPlaying)
                    {
                        if (Camera.current != null)
                        {
                            _Caster = Camera.current.transform;
                        }
                    }

                    if (m_ViewType == RaycastType.THIRD_PERSON_VIEW)
                    {
                        _Caster = m_FromTransform;
                    }
                    else
                    {
                        _Caster = Camera.main.transform;
                    }

                    return _Caster;
                }
            }

            public float GetRaycastDistance
            {
                get
                {
                    return MaxDistance == 0 ? Distance : MaxDistance;
                }
            }
        }
        [SerializeField] RaycastSettings m_RaycastSettings = new RaycastSettings();
        public RaycastSettings GetRaycastSettings { get { return m_RaycastSettings; } }

        [Serializable]
        public class SnappingSettings
        {
            public enum DetectionType { OVERLAP, RAYCAST }

            [SerializeField] DetectionType m_Type;
            public DetectionType Type { get { return m_Type; } set { m_Type = value; } }

            [SerializeField] LayerMask m_LayerMask = 1 << 8;
            public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

            [SerializeField, DontDrawIf("m_Type", DetectionType.OVERLAP)] float m_MaxAngle = 20f;
            public float MaxAngles { get { return m_MaxAngle; } set { m_MaxAngle = value; } }
        }
        [SerializeField] SnappingSettings m_SnappingSettings = new SnappingSettings();
        public SnappingSettings GetSnappingSettings { get { return m_SnappingSettings; } }

        [Serializable]
        public class PreviewSettings
        {
            public enum MovementType { NORMAL, SMOOTH, GRID }

            [SerializeField] MovementType m_Type;
            public MovementType Type { get { return m_Type; } set { m_Type = value; } }

            [SerializeField, DontDrawIf("m_Type", MovementType.GRID)] float m_MovementGridSize = 1f;
            public float MovementGridSize { get { return m_MovementGridSize; } }

            [SerializeField, DontDrawIf("m_Type", MovementType.GRID)] float m_MovementGridOffset;
            public float MovementGridOffset { get { return m_MovementGridOffset; } }

            [SerializeField, DontDrawIf("m_Type", MovementType.SMOOTH)] float m_MovementSmoothTime = 5f;
            public float MovementSmoothTime { get { return m_MovementSmoothTime; } }

            [SerializeField] bool m_LockRotation;
            public bool LockRotation { get { return m_LockRotation; } }

            [SerializeField] bool m_ResetRotation;
            public bool ResetRotation { get { return m_ResetRotation; } }
        }
        [SerializeField] PreviewSettings m_PreviewSettings = new PreviewSettings();
        public PreviewSettings GetPreviewSettings { get { return m_PreviewSettings; } }

        [Serializable]
        public class AudioSettings
        {
            [SerializeField] AudioSource m_AudioSource;
            public AudioSource AudioSource { get { return m_AudioSource; } }

            [SerializeField] AudioClip[] m_PlacingAudioClips;
            public AudioClip[] PlacingAudioClips { get { return m_PlacingAudioClips; } }

            [SerializeField] AudioClip[] m_DestroyAudioClips;
            public AudioClip[] DestroyAudioClips { get { return m_DestroyAudioClips; } }

            [SerializeField] AudioClip[] m_EditingAudioClips;
            public AudioClip[] EditingAudioClips { get { return m_EditingAudioClips; } }
        }
        [SerializeField] AudioSettings m_AudioSettings = new AudioSettings();
        public AudioSettings GetAudioSettings { get { return m_AudioSettings; } }

        [SerializeField] BuildingPart m_SelectedBuildingPart;
        public BuildingPart GetSelectedBuildingPart { get { return m_SelectedBuildingPart; } }

        public bool CanPlacing { get { return CheckPlacingCondition(); } }

        public bool CanDestroy { get { return CheckDestroyCondition(); } }

        public bool CanEditing { get { return CheckEditingCondition(); } }

        BuildingPart m_LastPreview;

        BuildingPart m_CurrentPreview;
        public BuildingPart GetCurrentPreview { get { return m_CurrentPreview; } }

        BuildingSocket m_CurrentSocket;

        Vector3 m_CurrentPreviewRotation;
        Vector3 m_CurrentPreviewDefaultScale;
        Vector3 m_LastAllowedPosition;

        readonly RaycastHit[] m_RaycastResults = new RaycastHit[5];

        #region Events

        /// <summary>
        /// Called when the build mode is changed.
        /// </summary>
        [Serializable] public class ChangedBuildModeEvent : UnityEvent<BuildMode> { }
        public ChangedBuildModeEvent OnChangedBuildModeEvent = new ChangedBuildModeEvent();

        /// <summary>
        /// Called when the Building Part selection is changed.
        /// </summary>
        [Serializable] public class ChangedBuildingPartEvent : UnityEvent<BuildingPart> { }
        public ChangedBuildingPartEvent OnChangedBuildingPartEvent = new ChangedBuildingPartEvent();

        #endregion

        #endregion

        #region Unity Methods

        public virtual void Start()
        {
            m_SnappingSettings.LayerMask |= (1 << LayerMask.NameToLayer("Socket"));

            if (BuildingManager.Instance != null)
            {
                SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[0]);
            }
        }

        public virtual void Update()
        {
            if (m_RaycastSettings.Camera == null)
            {
                m_RaycastSettings.Camera = Camera.current;
            }

            if (m_BuildMode == BuildMode.PLACE)
            {
                HandlePlacingMode();
            }
            else if (m_BuildMode == BuildMode.DESTROY)
            {
                HandleDestroyMode();
            }
            else if (m_BuildMode == BuildMode.EDIT)
            {
                HandleEditingMode();
            }

            if (m_BuildMode != BuildMode.NONE)
            {
                HandlePreviewMaterials();
            }
        }

        void Reset()
        {
            if (m_RaycastSettings.Camera == null)
            {
                m_RaycastSettings.Camera = Camera.main;
            }

            m_SnappingSettings.LayerMask |= (1 << LayerMask.NameToLayer("Socket"));
        }

        void OnDestroy()
        {
            if (m_InputHandler != null)
            {
                if (!Application.isPlaying)
                {
                    DestroyImmediate(m_InputHandler);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Ray ray = m_RaycastSettings.GetRay();
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(ray.origin, ray.direction * m_RaycastSettings.Distance);
        }

        #endregion

        #region Internal Methods

        #region Placing Mode

        /// <summary>
        /// Handle placing mode.
        /// </summary>
        void HandlePlacingMode()
        {
            if (!HasPreview())
            {
                CreatePreview(GetSelectedBuildingPart);
            }
            else
            {
                if (FindClosestSocket())
                {
                    HandleSnapping();
                }
                else
                {
                    HandleFree();
                }
            }
        }

        /// <summary>
        /// Handle the building's snap placement.
        /// </summary>
        void HandleSnapping()
        {
            BuildingSocket.SnappingPointSettings socketOffset = m_CurrentSocket.GetOffset(m_CurrentPreview);

            if (socketOffset != null)
            {
                m_CurrentSocket.Snap(m_CurrentPreview, socketOffset, m_CurrentPreviewRotation);

                m_CurrentPreview.AttachedBuildingSocket = m_CurrentSocket;

                if (!CanPlacing)
                {
                    HandleFree();
                }
            }
        }

        /// <summary>
        /// Manage the free placement of the building.
        /// </summary>
        void HandleFree()
        {
            m_CurrentPreview.AttachedBuildingSocket = null;

            if (m_PreviewSettings.LockRotation)
            {
                m_CurrentPreview.transform.rotation = Quaternion.Euler(new Vector3(0, m_RaycastSettings.GetCaster.eulerAngles.y, 0)) *
                    GetSelectedBuildingPart.transform.localRotation * Quaternion.Euler(m_CurrentPreviewRotation);
            }
            else
            {
                m_CurrentPreview.transform.rotation = Quaternion.Euler(m_CurrentPreviewRotation);
            }

            m_CurrentPreview.transform.localScale = m_CurrentPreviewDefaultScale; // * 1.005f

            if (Physics.Raycast(m_RaycastSettings.GetRay(), out RaycastHit hit,
                m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask))
            {
                if (hit.collider != null)
                {
                    Vector3 nextPosition = GetLookPosition(hit.point);

                    if (m_CurrentPreview.GetPreviewSettings.CanMovingIfPlaceable)
                    {
                        m_CurrentPreview.transform.position = nextPosition;

                        if (CanPlacing)
                        {
                            m_LastAllowedPosition = m_CurrentPreview.transform.position;
                        }
                        else
                        {
                            m_CurrentPreview.transform.position = m_LastAllowedPosition;
                        }
                    }
                    else
                    {
                        m_CurrentPreview.transform.position = nextPosition;
                    }

                    if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAngle)
                    {
                        if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Forward)
                        {
                            m_CurrentPreview.transform.forward = hit.normal;
                        }
                        else if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Backward)
                        {
                            m_CurrentPreview.transform.forward = -hit.normal;
                        }
                        else if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Left)
                        {
                            m_CurrentPreview.transform.right = -hit.normal;
                        }
                        else if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Right)
                        {
                            m_CurrentPreview.transform.right = hit.normal;
                        }
                        else if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Up)
                        {
                            m_CurrentPreview.transform.up = hit.normal;
                        }
                        else if (m_CurrentPreview.GetPreviewSettings.RotateAccordingAxis == BuildingPart.PreviewSettings.Axis.Down)
                        {
                            m_CurrentPreview.transform.up = -hit.normal;
                        }
                    }
                    if (m_CurrentPreview.GetPreviewSettings.ClampRotation)
                    {
                        m_CurrentPreview.transform.eulerAngles = MathExtension.Clamp(m_CurrentPreview.transform.eulerAngles,
                            m_CurrentPreview.GetPreviewSettings.ClampMinRotation, m_CurrentPreview.GetPreviewSettings.ClampMaxRotation);
                    }

                    return;
                }
            }

            Vector3 lookDistance = GetLookDistance();

            m_CurrentPreview.transform.position = lookDistance + m_CurrentPreview.GetPreviewSettings.OffsetPosition;

            if (m_PreviewSettings.Type == PreviewSettings.MovementType.GRID)
            {
                m_CurrentPreview.transform.position = MathExtension.Grid(m_PreviewSettings.MovementGridSize, m_PreviewSettings.MovementGridOffset,
                    lookDistance);
            }
            else if (m_PreviewSettings.Type == PreviewSettings.MovementType.SMOOTH)
            {
                m_CurrentPreview.transform.position = Vector3.Lerp(m_CurrentPreview.transform.position,
                    lookDistance, m_PreviewSettings.MovementSmoothTime * Time.deltaTime);
            }
        }

        /// <summary>
        /// Get the look position.
        /// </summary>
        Vector3 GetLookPosition(Vector3 point)
        {
            Vector3 targetPoint = point + m_CurrentPreview.GetPreviewSettings.OffsetPosition;
            Vector3 nextPoint = targetPoint;

            if (m_CurrentPreview.GetPreviewSettings.ClampPosition)
            {
                nextPoint = MathExtension.Clamp(nextPoint, m_CurrentPreview.GetPreviewSettings.ClampMinPosition,
                    m_CurrentPreview.GetPreviewSettings.ClampMaxPosition);
            }

            if (m_PreviewSettings.Type == PreviewSettings.MovementType.SMOOTH)
            {
                nextPoint = Vector3.Lerp(m_CurrentPreview.transform.position, nextPoint, m_PreviewSettings.MovementSmoothTime * Time.deltaTime);
            }
            else if (m_PreviewSettings.Type == PreviewSettings.MovementType.GRID)
            {
                nextPoint = MathExtension.Grid(m_PreviewSettings.MovementGridSize, m_PreviewSettings.MovementGridOffset, nextPoint);
            }

            return nextPoint;
        }

        /// <summary>
        /// Get the look distance.
        /// </summary>
        Vector3 GetLookDistance()
        {
            Vector3 lookDistance = m_RaycastSettings.GetCaster.position + m_RaycastSettings.GetCaster.forward * m_RaycastSettings.GetRaycastDistance;

            int hits = Physics.RaycastNonAlloc(m_CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                    Vector3.down, m_RaycastResults, Mathf.Infinity, m_RaycastSettings.LayerMask);

            for (int i = 0; i < hits; i++)
            {
                lookDistance.y = m_RaycastResults[i].point.y;
            }

            return lookDistance;
        }

        /// <summary>
        /// Check the conditions for placement.
        /// </summary>
        public bool CheckPlacingCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(m_RaycastSettings.GetCaster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckPlacingCondition())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Place the Building Part.
        /// </summary>
        public virtual bool PlacingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanPlacing)
            {
                return false;
            }

            BuildingManager.Instance.PlaceBuildingPart(GetSelectedBuildingPart,
                m_CurrentPreview.transform.position,
                m_CurrentPreview.transform.eulerAngles,
                m_CurrentPreview.transform.localScale);

            if (m_LastBuildMode == BuildMode.EDIT)
            {
                ChangeBuildMode(BuildMode.EDIT, true);
            }
            else
            {
                CancelPreview();
            }

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.PlacingAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.PlacingAudioClips[Random.Range(0,
                        m_AudioSettings.PlacingAudioClips.Length)]);
                }
            }

            return true;
        }

        #endregion

        #region Destroy Mode

        /// <summary>
        /// Handle destroy mode.
        /// </summary>
        void HandleDestroyMode()
        {
            m_CurrentPreview = GetTargetBuildingPart();

            if (m_CurrentPreview != null)
            {
                if (m_LastPreview != null)
                {
                    if (m_LastPreview != m_CurrentPreview)
                    {
                        m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                        m_LastPreview = null;
                    }
                }

                m_LastPreview = m_CurrentPreview;
                m_CurrentPreview.ChangeState(BuildingPart.StateType.DESTROY);
            }
            else
            {
                if (m_LastPreview != null)
                {
                    m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                    m_LastPreview = null;
                }
            }
        }

        /// <summary>
        /// Check the conditions for destruction.
        /// </summary>
        public bool CheckDestroyCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(m_RaycastSettings.GetCaster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckDestroyCondition())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Destroy the Building Part.
        /// </summary>
        public virtual bool DestroyBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanDestroy)
            {
                return false;
            }

            BuildingManager.Instance.DestroyBuildingPart(m_CurrentPreview);

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.DestroyAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.DestroyAudioClips[Random.Range(0,
                        m_AudioSettings.DestroyAudioClips.Length)]);
                }
            }

            return true;
        }

        #endregion

        #region Editing Mode

        /// <summary>
        /// Handle the editing mode.
        /// </summary>
        void HandleEditingMode()
        {
            m_CurrentPreview = GetTargetBuildingPart();

            if (m_CurrentPreview != null)
            {
                if (m_LastPreview != null)
                {
                    if (m_LastPreview != m_CurrentPreview)
                    {
                        m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                        m_LastPreview = null;
                    }
                }

                m_LastPreview = m_CurrentPreview;
                m_CurrentPreview.ChangeState(BuildingPart.StateType.EDIT);
            }
            else
            {
                if (m_LastPreview != null)
                {
                    m_LastPreview.ChangeState(BuildingPart.StateType.PLACED);
                    m_LastPreview = null;
                }
            }
        }

        /// <summary>
        /// Check the conditions for edition.
        /// </summary>
        public bool CheckEditingCondition()
        {
            if (m_CurrentPreview == null)
            {
                return false;
            }

            if (m_RaycastSettings.MaxDistance != 0)
            {
                if (Vector3.Distance(m_RaycastSettings.GetCaster.position, m_CurrentPreview.transform.position) > m_RaycastSettings.Distance)
                {
                    return false;
                }
            }

            if (!m_CurrentPreview.CheckEditingCondition())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Edit the Building Part.
        /// </summary>
        public virtual bool EditingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanEditing)
            {
                return false;
            }

            SelectBuildingPart(m_CurrentPreview);

            if (!m_CurrentPreview.GetPreviewSettings.RotateAccordingAngle)
            {
                m_CurrentPreviewRotation = m_CurrentPreview.transform.eulerAngles;
            }

            m_CurrentPreviewDefaultScale = m_CurrentPreview.transform.localScale;

            m_CurrentPreview.ChangeState(BuildingPart.StateType.PREVIEW);

            ChangeBuildMode(BuildMode.PLACE, false);

            if (m_AudioSettings.AudioSource != null)
            {
                if (m_AudioSettings.EditingAudioClips.Length != 0)
                {
                    m_AudioSettings.AudioSource.PlayOneShot(m_AudioSettings.EditingAudioClips[Random.Range(0,
                        m_AudioSettings.EditingAudioClips.Length)]);
                }
            }

            return true;
        }

        #endregion

        #region Preview

        /// <summary>
        /// Handle preview material.
        /// </summary>
        void HandlePreviewMaterials()
        {
            if (!HasPreview())
            {
                return;
            }

            if (m_BuildMode == BuildMode.PLACE)
            {
                m_CurrentPreview.UpdatePreviewMaterials(CanPlacing ? m_CurrentPreview.GetPreviewSettings.PlacingColor : m_CurrentPreview.GetPreviewSettings.DestroyingColor);
            }
            else if (m_BuildMode == BuildMode.DESTROY)
            {
                m_CurrentPreview.UpdatePreviewMaterials(m_CurrentPreview.GetPreviewSettings.DestroyingColor);
            }
            else if (m_BuildMode == BuildMode.EDIT)
            {
                m_CurrentPreview.UpdatePreviewMaterials(m_CurrentPreview.GetPreviewSettings.EditingColor);
            }
        }

        /// <summary>
        /// Rotate the preview.
        /// </summary>
        public void RotatePreview(bool reverse = false)
        {
            if (!HasPreview())
            {
                return;
            }

            if (reverse)
            {
                m_CurrentPreviewRotation -= m_CurrentPreview.GetPreviewSettings.RotateAxis;
            }
            else
            {
                m_CurrentPreviewRotation += m_CurrentPreview.GetPreviewSettings.RotateAxis;
            }
        }

        /// <summary>
        /// Create a preview Building Part.
        /// </summary>
        public BuildingPart CreatePreview(BuildingPart buildingPart)
        {
            if (buildingPart == null)
            {
                return null;
            }

            Vector3 instantiatePoint = Vector3.zero;

            if (Physics.Raycast(m_RaycastSettings.GetRay(), out RaycastHit hit,
                m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask))
            {
                instantiatePoint = hit.point;
            }

            m_CurrentPreview = Instantiate(buildingPart);
            m_CurrentPreview.ChangeState(BuildingPart.StateType.PREVIEW);
            m_CurrentPreview.transform.position = instantiatePoint;

            m_CurrentPreviewDefaultScale = m_CurrentPreview.transform.localScale;

            return m_CurrentPreview;
        }

        /// <summary>
        /// Cancel the current preview.
        /// </summary>
        public void CancelPreview()
        {
            if (m_CurrentPreview == null)
            {
                return;
            }

            RenderExtension.ChangeMaterialColorRecursively(m_CurrentPreview.Renderers.ToArray(), new Color(1, 1, 1, 0.25f),
                    m_CurrentPreview.GetPreviewSettings.IgnoreRenderers.ToArray());

            if (m_CurrentPreview.State == BuildingPart.StateType.PREVIEW)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_CurrentPreview.gameObject);
                }
                else
                {
                    DestroyImmediate(m_CurrentPreview.gameObject);
                }
            }
            else if (m_CurrentPreview.State == BuildingPart.StateType.DESTROY)
            {
                m_CurrentPreview.ChangeState(BuildingPart.StateType.PLACED);
            }
            else if (m_CurrentPreview.State == BuildingPart.StateType.EDIT)
            {
                m_CurrentPreview.ChangeState(BuildingPart.StateType.PLACED);
            }

            if (m_PreviewSettings.ResetRotation)
            {
                m_CurrentPreviewRotation = Vector3.zero;
            }

            m_CurrentPreview = null;
        }

        /// <summary>
        /// Check if has a preview.
        /// </summary>
        public bool HasPreview()
        {
            return m_CurrentPreview != null;
        }

        #endregion

        #region Socket

        /// <summary>
        /// Find the closest socket.
        /// </summary>
        public bool FindClosestSocket()
        {
            if (m_CurrentPreview.TryGetBasicsCondition.IgnoreSocket)
            {
                return false;
            }

            m_CurrentSocket = null;

            Ray ray = m_RaycastSettings.GetRay();

            if (m_SnappingSettings.Type == SnappingSettings.DetectionType.RAYCAST)
            {
                RaycastHit[] hits = new RaycastHit[BuildingManager.Instance.RegisteredBuildingSockets.Count];

                int colliderCount = Physics.RaycastNonAlloc(ray, hits, m_RaycastSettings.Distance, m_SnappingSettings.LayerMask);

                for (int i = 0; i < colliderCount; i++)
                {
                    if (hits[i].collider.TryGetComponent(out BuildingSocket buildingSocket))
                    {
                        m_CurrentSocket = buildingSocket;
                    }
                }
            }
            else
            {
                Collider[] hitColliders = new Collider[BuildingManager.Instance.RegisteredBuildingSockets.Count];

                float closestAngle = Mathf.Infinity;

                int numColliders = Physics.OverlapSphereNonAlloc(m_RaycastSettings.GetCaster.position,
                    m_RaycastSettings.Distance, hitColliders, m_SnappingSettings.LayerMask);

                for (int i = 0; i < numColliders; i++)
                {
                    if (!m_RaycastSettings.Through)
                    {
                        Physics.Linecast(m_RaycastSettings.GetRay().origin, hitColliders[i].ClosestPointOnBounds(m_RaycastSettings.GetRay().origin),
                            out RaycastHit hit, m_RaycastSettings.LayerMask);

                        if (hit.collider != null)
                        {
                            if (hit.collider.GetComponentInParent<BuildingPart>() != null)
                            {
                                continue;
                            }
                        }
                    }

                    if (hitColliders[i].TryGetComponent(out BuildingSocket buildingSocket))
                    {
                        if (buildingSocket != null)
                        {
                            if (buildingSocket.IsEnabled() && buildingSocket.IsFitting(m_CurrentPreview))
                            {
                                if (CloseFromPosition(buildingSocket.transform.position))
                                {
                                    float angle = Vector3.Angle(ray.direction,
                                        buildingSocket.transform.position - ray.origin);

                                    if (angle < closestAngle && angle < m_SnappingSettings.MaxAngles)
                                    {
                                        if (!buildingSocket.IsBusy(m_CurrentPreview))
                                        {
                                            closestAngle = angle;
                                            m_CurrentSocket = buildingSocket;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return m_CurrentSocket;
        }

        bool CloseFromPosition(Vector3 position)
        {
            if (m_RaycastSettings.ViewType == RaycastSettings.RaycastType.TOP_DOWN_VIEW)
            {
                return true;
            }

            return ((position - m_RaycastSettings.GetCaster.position).sqrMagnitude < Mathf.Pow(m_RaycastSettings.Distance, 2));
        }

        #endregion

        /// <summary>
        /// Get the target Building Part.
        /// </summary>
        public BuildingPart GetTargetBuildingPart()
        {
            Physics.Raycast(m_RaycastSettings.GetRay(), out RaycastHit hit,
                m_RaycastSettings.GetRaycastDistance, m_RaycastSettings.LayerMask);

            if (hit.collider != null)
            {
                BuildingPart targetBuildingPart = hit.collider.GetComponentInParent<BuildingPart>();

                if (targetBuildingPart != null)
                {
                    if (targetBuildingPart.State != BuildingPart.StateType.PREVIEW)
                    {
                        return hit.collider.GetComponentInParent<BuildingPart>();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Change the build mode.
        /// </summary>
        public void ChangeBuildMode(BuildMode mode, bool clearPreview = true)
        {
            
            if (clearPreview)
            {
                CancelPreview();
            }

            m_LastBuildMode = m_BuildMode;
            m_BuildMode = mode;

            OnChangedBuildModeEvent?.Invoke(mode);
        }

        /// <summary>
        /// Select a Building Part.
        /// </summary>
        public void SelectBuildingPart(BuildingPart part)
        {
            if (part == null)
            {
                return;
            }

            BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(part.GetGeneralSettings.Identifier);
            m_SelectedBuildingPart = buildingPart;

            OnChangedBuildingPartEvent.Invoke(m_SelectedBuildingPart);
        }

        #endregion
    }
}