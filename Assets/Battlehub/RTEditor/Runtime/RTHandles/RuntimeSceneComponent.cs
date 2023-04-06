using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public interface IRuntimeSceneComponent : IRuntimeSelectionComponent
    {
        RectTransform SceneGizmoTransform
        {
            get;
        }

        bool IsSceneGizmoEnabled
        {
            get;
            set;
        }

        SceneGizmo SceneGizmo
        {
            get;
        }

        [Obsolete("Use CanRotate instead")]
        bool CanOrbit
        {
            get;
            set;
        }

        bool CanRotate
        {
            get;
            set;
        }

        bool CanZoom
        {
            get;
            set;
        }

        float FreeRotationSmoothSpeed
        {
            get;
            set;
        }

        bool RotationInvertX
        {
            get;
            set;
        }

        bool RotationInvertY
        {
            get;
            set;
        }

        float FreeMovementSmoothSpeed
        {
            get;
            set;
        }

        [Obsolete]
        float ZoomSpeed
        {
            get;
            set;
        }

        bool ConstantZoomSpeed
        {
            get;
            set;
        }

        float ZoomSensitivity
        {
            get;
            set;
        }

        float MoveSensitivity
        {
            get;
            set;
        }

        float RotationSensitivity
        {
            get;
            set;
        }

        bool ChangeOrthographicSizeOnly
        {
            get;
            set;
        }

        bool CanPan
        {
            get;
            set;
        }

        bool CanFreeMove
        {
            get;
            set;
        }

        float BoundingSphereRadius
        {
            get;
            set;
        }

        float MinOrthoSize
        {
            get;
            set;
        }

        float MaxOrthoSize
        {
            get;
            set;
        }

        GameObject GameObject
        {
            get;
        }
    }

    public class RuntimeSceneComponent : RuntimeSelectionComponent, IRuntimeSceneComponent
    {
        public Texture2D ViewTexture;
        public Texture2D MoveTexture;
        public Texture2D FreeMoveTexture;

        [SerializeField]
        private RectTransform m_sceneGizmoTransform = null;
        //[SerializeField]
        private bool m_isSceneGizmoEnabled = true;
        [SerializeField]
        private bool m_canPan = true;
        [SerializeField]
        private bool m_canZoom = true;
        [SerializeField]
        private bool m_changeOrthographicSizeOnly = false;
        [SerializeField]
        private bool m_canRotate = true;
        [SerializeField]
        private bool m_canFreeMove = true;
        [SerializeField]
        private float m_orbitDistance = 5.0f;
        [SerializeField]
        private float m_boundingSphereRadius = 10000.0f;
        [SerializeField]
        private float m_minOrthoSize = 0.01f;
        [SerializeField]
        private float m_maxOrthoSize = 10000.0f;
        [SerializeField]
        private bool m_constantZoomSpeed = false;
        [SerializeField]
        private float m_zoomSensitivity = 1.0f;
        [SerializeField]
        private float m_moveSensitivity = 1.0f;
        [SerializeField]
        private float m_rotationSensitivity = 1.0f;
        [SerializeField]
        private float m_freeMovementSmoothSpeed = 10.0f;
        [SerializeField]
        private float m_freeRotationSmoothSpeed = 10.0f;
        [SerializeField]
        private bool m_rotationInvertX = false;
        [SerializeField]
        private bool m_rotationInvertY = false;

        private Quaternion m_targetRotation;
        private Vector3 m_targetPosition;
        private Quaternion m_prevCamRotation;
        private Vector3 m_prevCamPosition;
        private bool m_isSceneGizmoOrientationChanging;

        public bool IsSceneGizmoEnabled
        {
            get { return m_isSceneGizmoEnabled && m_sceneGizmo != null; }
            set
            {
                m_isSceneGizmoEnabled = value;
                if (m_sceneGizmo != null)
                {
                    m_sceneGizmo.gameObject.SetActive(value);
                }
            }
        }

        public RectTransform SceneGizmoTransform
        {
            get { return m_sceneGizmoTransform; }
        }

        public bool CanOrbit
        {
            get { return CanRotate; }
            set { CanRotate = value; }
        }

        public bool CanRotate
        {
            get { return m_canRotate; }
            set { m_canRotate = value; }
        }

        public bool CanZoom
        {
            get { return m_canZoom; }
            set { m_canZoom = value; }
        }

        public bool ChangeOrthographicSizeOnly
        {
            get { return m_changeOrthographicSizeOnly; }
            set { m_changeOrthographicSizeOnly = value; }
        }

        public bool CanPan
        {
            get { return m_canPan; }
            set { m_canPan = value; }
        }

        public bool CanFreeMove
        {
            get { return m_canFreeMove; }
            set { m_canFreeMove = value; }
        }

        public float FreeMovementSmoothSpeed
        {
            get { return m_freeMovementSmoothSpeed; }
            set { m_freeMovementSmoothSpeed = value; }
        }

        public float FreeRotationSmoothSpeed
        {
            get { return m_freeRotationSmoothSpeed; }
            set { m_freeRotationSmoothSpeed = value; }
        }

        public bool RotationInvertX
        {
            get { return m_rotationInvertX; }
            set { m_rotationInvertX = value; }
        }

        public bool RotationInvertY
        {
            get { return m_rotationInvertY; }
            set { m_rotationInvertY = value; }
        }

        [Obsolete]
        public float ZoomSpeed
        {
            get { return ZoomSensitivity; }
            set { ZoomSensitivity = value; }
        }

        public bool ConstantZoomSpeed
        {
            get { return m_constantZoomSpeed; }
            set { m_constantZoomSpeed = value; }
        }

        public float ZoomSensitivity
        {
            get { return m_zoomSensitivity; }
            set { m_zoomSensitivity = value; }
        }

        public float MoveSensitivity
        {
            get { return m_moveSensitivity; }
            set { m_moveSensitivity = value; }
        }

        public float RotationSensitivity
        {
            get { return m_rotationSensitivity; }
            set { m_rotationSensitivity = value; }
        }

        public float BoundingSphereRadius
        {
            get { return m_boundingSphereRadius; }
            set { m_boundingSphereRadius = value; }
        }

        public float MinOrthoSize
        {
            get { return m_minOrthoSize; }
            set { m_maxOrthoSize = value; }
        }

        public float MaxOrthoSize
        {
            get { return m_maxOrthoSize; }
            set { m_maxOrthoSize = value; }
        }

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        public override Vector3 Pivot
        {
            get { return base.Pivot; }
            set
            {
                base.Pivot = value;
                m_orbitDistance = (Pivot - m_targetPosition).magnitude;
            }
        }

        public override Vector3 CameraPosition
        {
            get { return base.CameraPosition; }
            set
            {
                base.CameraPosition = value;
                Transform camTransform = Window.Camera.transform;
                m_targetPosition = camTransform.position;
                m_targetRotation = camTransform.rotation;
                m_orbitDistance = (Pivot - m_targetPosition).magnitude;
            }
        }

        public override void SetCameraPositionAndPivot(Vector3 position, Vector3 pivot)
        {
            base.SetCameraPositionAndPivot(position, pivot);

            Transform camTransform = Window.Camera.transform;
            m_targetPosition = camTransform.position;
            m_targetRotation = camTransform.rotation;
            m_orbitDistance = (Pivot - m_targetPosition).magnitude;
        }

        public override bool IsOrthographic
        {
            get { return base.IsOrthographic; }
            set
            {
                if (m_sceneGizmo != null)
                {
                    if (m_sceneGizmo.IsOrthographic != value)
                    {
                        m_sceneGizmo.IsOrthographic = value;
                    }
                }
                else
                {
                    base.IsOrthographic = value;
                }
            }
        }

        private Plane m_dragPlane;
        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }

        private Vector3 m_lastMousePosition;
        protected Vector3 LastMousePosition
        {
            get { return m_lastMousePosition; }
            set { m_lastMousePosition = value; }
        }

        private bool m_lockInput;
        protected bool IsInputLocked
        {
            get { return m_lockInput; }
        }

        [SerializeField]
        private SceneGizmo m_sceneGizmo;
        public SceneGizmo SceneGizmo
        {
            get { return m_sceneGizmo; }
        }

        private IAnimationInfo m_focusAnimation;
        private Transform m_autoFocusTransform;

        protected override void Awake()
        {
            base.Awake();

            Window.IOCContainer.RegisterFallback<IRuntimeSceneComponent>(this);

            if (Run.Instance == null)
            {
                GameObject runGO = new GameObject("Run");
                runGO.transform.SetParent(transform, false);
                runGO.name = "Run";
                runGO.AddComponent<Run>();
            }

            if (ViewTexture == null)
            {
                ViewTexture = Resources.Load<Texture2D>("RTH_Eye");
            }
            if (MoveTexture == null)
            {
                MoveTexture = Resources.Load<Texture2D>("RTH_Hand");
            }
            if (FreeMoveTexture == null)
            {
                FreeMoveTexture = Resources.Load<Texture2D>("RTH_FreeMove");
            }

            if (m_sceneGizmo == null)
            {
                m_sceneGizmo = GetComponentInChildren<SceneGizmo>(true);
            }

            if (m_sceneGizmo != null)
            {
                if (m_sceneGizmo.Window == null)
                {
                    m_sceneGizmo.Window = Window;
                }
                m_sceneGizmo.OrientationChanging.AddListener(OnSceneGizmoOrientationChanging);
                m_sceneGizmo.OrientationChanged.AddListener(OnSceneGizmoOrientationChanged);
                m_sceneGizmo.ProjectionChanged.AddListener(OnSceneGizmoProjectionChanged);
                m_sceneGizmo.Pivot = PivotTransform;
                if (!IsSceneGizmoEnabled)
                {
                    m_sceneGizmo.gameObject.SetActive(false);
                }
            }

            Transform camTransform = Window.Camera.transform;
            camTransform.LookAt(Pivot);

            camTransform.position = cameraPosition;
            camTransform.rotation = cameraRotation;

            m_targetRotation = camTransform.rotation;
            m_targetPosition = camTransform.position;
            cameraTransform = Window.Camera.transform;
            m_orbitDistance = (Pivot - m_targetPosition).magnitude;

        }
        [HideInInspector] public Transform cameraTransform;
        [HideInInspector] public Vector3 cameraPosition;
        [HideInInspector] public Quaternion cameraRotation;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Window.IOCContainer.UnregisterFallback<IRuntimeSceneComponent>(this);

            if (m_sceneGizmo != null)
            {
                m_sceneGizmo.OrientationChanging.RemoveListener(OnSceneGizmoOrientationChanging);
                m_sceneGizmo.OrientationChanged.RemoveListener(OnSceneGizmoOrientationChanged);
                m_sceneGizmo.ProjectionChanged.RemoveListener(OnSceneGizmoProjectionChanged);
            }
        }

        protected override void Start()
        {
            if (GetComponent<RuntimeSelectionInputBase>() == null)
            {
                gameObject.AddComponent<RuntimeSceneInput>();
            }

            base.Start();
        }

        protected virtual void Update()
        {
            if (Editor.Tools.AutoFocus)
            {
                do
                {
                    if (Editor.Tools.ActiveTool != null)
                    {
                        break;
                    }

                    if (m_autoFocusTransform == null)
                    {
                        break;
                    }

                    if (m_autoFocusTransform.position == Pivot)
                    {
                        break;
                    }

                    if (m_focusAnimation != null && m_focusAnimation.InProgress)
                    {
                        break;
                    }

                    if (m_lockInput)
                    {
                        break;
                    }

                    Vector3 offset = (m_autoFocusTransform.position - SecondaryPivot);
                    Window.Camera.transform.position += offset;
                    PivotTransform.position += offset;
                    SecondaryPivotTransform.position += offset;
                }
                while (false);
            }

            if (Grid != null)
            {
                if (IsOrthographic)
                {
                    Transform camTransform = Window.Camera.transform;
                    UpdateGridRotation();
                    if (m_prevCamPosition != camTransform.position || m_prevCamRotation != camTransform.rotation)
                    {
                        m_prevCamPosition = camTransform.position;
                        m_prevCamRotation = camTransform.rotation;
                    }
                    else
                    {
                        if (!m_isSceneGizmoOrientationChanging)
                        {
                            Grid.Alpha += Time.deltaTime * 5;
                        }
                    }
                }
                else
                {
                    if (IsGridCloseToCamera())
                    {
                        Grid.Alpha -= Time.deltaTime * 25;
                    }
                    else
                    {
                        Grid.Alpha += Time.deltaTime * 5;
                    }
                }
            }
        }

        private bool IsGridCloseToCamera()
        {
            Transform camTransform = Window.Camera.transform;
            return Mathf.Abs(camTransform.position.y - Grid.transform.position.y) < 0.1f;
        }

        private void UpdateGridRotation()
        {
            Vector3 camForward = Window.Camera.transform.forward;
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(Vector3.right, camForward)), 1))
            {
                Grid.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(Vector3.forward, camForward)), 1))
            {
                Grid.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            else
            {
                Grid.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        public void UpdateCursorState(bool isPointerOverEditorArea, bool pan, bool rotate, bool freeMove)
        {
            if (!isPointerOverEditorArea)
            {
                Window.Editor.CursorHelper.ResetCursor(this);
                return;
            }

            if (freeMove && CanFreeMove)
            {
                Editor.CursorHelper.SetCursor(this, FreeMoveTexture, Vector2.one * 0.5f, CursorMode.Auto);
            }
            else if (pan && CanPan)
            {
                if (rotate && Editor.Tools.Current == RuntimeTool.View)
                {
                    Editor.CursorHelper.SetCursor(this, ViewTexture, Vector2.one * 0.5f, CursorMode.Auto);
                }
                else
                {
                    Editor.CursorHelper.SetCursor(this, MoveTexture, Vector2.one * 0.5f, CursorMode.Auto);
                }
            }
            else if (rotate && CanRotate)
            {
                Editor.CursorHelper.SetCursor(this, ViewTexture, Vector2.one * 0.5f, CursorMode.Auto);
            }
            else
            {
                Editor.CursorHelper.ResetCursor(this);
            }
        }

        [Obsolete]
        public override void Focus()
        {
            Focus(FocusMode.Selected);
        }

        public override void Focus(FocusMode focusMode = FocusMode.Selected)
        {
            if (m_lockInput)
            {
                return;
            }

            m_autoFocusTransform = null;

            Transform[] transforms;
            if (focusMode == FocusMode.Selected || focusMode == FocusMode.Default)
            {
                if (Selection.activeTransform == null)
                {
                    return;
                }

                if ((Selection.activeTransform.gameObject.hideFlags & HideFlags.DontSave) != 0 || Selection.activeGameObject.IsPrefab())
                {
                    return;
                }

                m_autoFocusTransform = Selection.activeTransform;
                transforms = Selection.gameObjects.Select(go => go.transform).ToArray();
            }
            else
            {
                transforms = Editor.Object.Get(true).SelectMany(e => e.GetComponentsInChildren<Transform>()).Where(r => r.gameObject.activeInHierarchy).ToArray();
            }

            Bounds bounds = TransformUtility.CalculateBounds(transforms);
            if (bounds.extents == Vector3.zero)
            {
                bounds.extents = Vector3.one * 0.5f;
            }
            float objSize = Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z) * 2.0f;
            Focus(bounds.center, objSize);
            if (focusMode == FocusMode.Selected || focusMode == FocusMode.Default)
            {
                if (Selection.activeTransform != null)
                {
                    SecondaryPivotTransform.position = Selection.activeTransform.position;
                }
            }
            else
            {
                SecondaryPivotTransform.position = bounds.center;
            }
        }

        public override void Focus(Vector3 objPosition, float objSize)
        {
            PivotTransform.position = objPosition;
            SecondaryPivotTransform.position = objPosition;

            float distance;
            if (ChangeOrthographicSizeOnly && IsOrthographic)
            {
                distance = m_orbitDistance;
            }
            else
            {
                float fov = Window.Camera.fieldOfView * Mathf.Deg2Rad;
                distance = Mathf.Abs(objSize / Mathf.Sin(fov / 2.0f));
            }

            Focus(distance, objSize);
        }

        private void Focus(float distance, float objSize)
        {
            const float duration = 0.1f;

            m_focusAnimation = new Vector3AnimationInfo(Window.Camera.transform.position, PivotTransform.position - distance * Window.Camera.transform.forward, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    if (Window.Camera)
                    {
                        Window.Camera.transform.position = value;
                        m_targetPosition = value;
                    }
                });
            Run.Instance.Animation(m_focusAnimation);
            Run.Instance.Animation(new FloatAnimationInfo(m_orbitDistance, distance, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    m_orbitDistance = value;
                }));

            Run.Instance.Animation(new FloatAnimationInfo(Window.Camera.orthographicSize, objSize, duration, Vector3AnimationInfo.EaseOutCubic,
                (target, value, t, completed) =>
                {
                    if (Window.Camera)
                    {
                        Window.Camera.orthographicSize = value;
                    }
                }));
        }

        public virtual void Zoom(float deltaZ, Quaternion rotation)
        {
            Zoom(deltaZ, rotation, 0.0001f);
        }

        public virtual void Zoom(float deltaZ, Quaternion rotation, float epsilonSq)
        {
            if (m_lockInput)
            {
                return;
            }

            if (!CanZoom)
            {
                deltaZ = 0;
            }

            Camera camera = Window.Camera;

            if (camera.orthographic)
            {
                camera.orthographicSize -= deltaZ * camera.orthographicSize;
                if (camera.orthographicSize < MinOrthoSize)
                {
                    camera.orthographicSize = MinOrthoSize;
                }
                if (camera.orthographicSize > MaxOrthoSize)
                {
                    camera.orthographicSize = MaxOrthoSize;
                }

                if (ChangeOrthographicSizeOnly)
                {
                    return;
                }
            }

            Vector3 fwd = rotation * Vector3.forward * deltaZ;
            if (m_constantZoomSpeed)
            {
                fwd *= ZoomSensitivity * 10.0f;
            }
            else
            {
                fwd *= Mathf.Max(ZoomSensitivity * 10.0f, Mathf.Abs(m_orbitDistance));
            }

            Transform cameraTransform = Window.Camera.transform;
            m_orbitDistance = m_orbitDistance - fwd.z;
            fwd.z = 0;

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -m_orbitDistance);
            m_targetPosition = cameraTransform.TransformVector(fwd) + cameraTransform.rotation * negDistance + PivotTransform.position;

            if (ClampPosition(Pivot, ref m_targetPosition))
            {
                RecalculateOrbitDistance(m_targetPosition);
            }

            if (!MathHelper.Approximately(m_targetPosition, cameraTransform.position, epsilonSq))
            {
                cameraTransform.position = m_targetPosition;
            }
        }

        public virtual void Orbit(float deltaX, float deltaY, float deltaZ)
        {
            if (m_lockInput || !CanRotate)
            {
                return;
            }
            if (deltaX == 0 && deltaY == 0 && deltaZ == 0)
            {
                return;
            }

            if (m_rotationInvertY)
            {
                deltaY = -deltaY;
            }

            if (m_rotationInvertX)
            {
                deltaX = -deltaX;
            }

            deltaX *= RotationSensitivity;
            deltaY *= RotationSensitivity;
            deltaZ *= RotationSensitivity;

            Transform cameraTransform = Window.Camera.transform;

            m_targetRotation = Quaternion.Inverse(
                Quaternion.Euler(deltaY, 0, 0) *
                Quaternion.Inverse(m_targetRotation) *
                Quaternion.Euler(0, -deltaX, 0));

            cameraTransform.rotation = m_targetRotation;

            Zoom(deltaZ, Quaternion.identity, 0);
        }

        public virtual void BeginPan(Vector3 mousePosition)
        {
            if (m_lockInput || !CanPan)
            {
                return;
            }
            m_lastMousePosition = mousePosition;

            RaycastHit hitInfo;
            if (Physics.Raycast(Window.Pointer, out hitInfo))
            {
                m_dragPlane = new Plane(-Window.Camera.transform.forward, hitInfo.point);
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float d;
                Ray ray = Window.Pointer;

                Vector3 camFwd = Window.Camera.transform.forward;

                float magMax = Mathf.Max(100, (Pivot - CameraPosition).magnitude);
                float magMin = Mathf.Max(10, (Pivot - CameraPosition).magnitude);
                if (groundPlane.Raycast(ray, out d) && d < magMax)
                {
                    m_dragPlane = groundPlane;
                    m_dragPlane = new Plane(-camFwd, ray.GetPoint(d));
                }
                else
                {
                    m_dragPlane = new Plane(-camFwd, Window.Camera.transform.position + camFwd * magMin);
                }

            }
        }

        public virtual void Pan(Vector3 mousePosition)
        {
            if (m_lockInput || !CanPan)
            {
                return;
            }

            if (GetPointOnDragPlane(mousePosition, out Vector3 pointOnDragPlane) &&
                GetPointOnDragPlane(m_lastMousePosition, out Vector3 prevPointOnDragPlane))
            {
                Transform camTransform = Window.Camera.transform;

                Vector3 delta = pointOnDragPlane - prevPointOnDragPlane;

                m_lastMousePosition = mousePosition;

                Vector3 newCameraPosition = camTransform.position - delta;
                ClampPosition(camTransform.position, ref newCameraPosition);
                camTransform.position = newCameraPosition;

                Vector3 newPivotPosition = PivotTransform.position - delta;
                ClampPosition(PivotTransform.position, ref newPivotPosition);
                PivotTransform.position = newPivotPosition;

                Vector3 newSecondaryPivotPosition = SecondaryPivotTransform.position - delta;
                ClampPosition(SecondaryPivotTransform.position, ref newSecondaryPivotPosition);
                SecondaryPivotTransform.position = newSecondaryPivotPosition;

                m_targetPosition = camTransform.position;

                RecalculateOrbitDistance(m_targetPosition);
            }
        }

        public virtual void FreeMove(Vector2 rotate, Vector3 move, float forward)
        {
            rotate *= RotationSensitivity;
            forward *= ZoomSensitivity;
            move *= MoveSensitivity;

            if (m_lockInput || !CanFreeMove)
            {
                return;
            }

            Transform camTransform = Window.Camera.transform;
            if (m_rotationInvertY)
            {
                rotate.y = -rotate.y;
            }

            if (m_rotationInvertX)
            {
                rotate.x = -rotate.x;
            }

            m_targetRotation = Quaternion.Inverse(
                Quaternion.Euler(rotate.y, 0, 0) *
                Quaternion.Inverse(m_targetRotation) *
                Quaternion.Euler(0, -rotate.x, 0));

            if (m_freeRotationSmoothSpeed <= 0)
            {
                camTransform.rotation = m_targetRotation;
            }
            else
            {
                if (!MathHelper.Approximately(camTransform.rotation, m_targetRotation))
                {
                    camTransform.rotation = Quaternion.Slerp(
                        camTransform.rotation,
                        m_targetRotation,
                        m_freeRotationSmoothSpeed * Time.deltaTime);
                }
            }

            Vector3 zoomOffset = Vector3.zero;
            if (Window.Camera.orthographic)
            {
                if (Mathf.Approximately(move.y, 0))
                {
                    move.y = forward;
                }
                else
                {
                    move.y /= 10.0f;
                }

                if (!Mathf.Approximately(move.y, 0))
                {
                    Vector3 position = camTransform.position;
                    Zoom(move.y, Quaternion.identity);
                    zoomOffset = camTransform.position - position;
                    camTransform.position = position;
                    move.y = 0;
                }
            }
            else
            {
                if (Mathf.Approximately(move.y, 0))
                {
                    move.y = forward * 50;
                }
            }

            m_targetPosition = m_targetPosition + zoomOffset +
                camTransform.forward * move.y + camTransform.right * move.x + camTransform.up * move.z;

            if (ClampPosition(Pivot, ref m_targetPosition))
            {
                RecalculateOrbitDistance(m_targetPosition);
            }

            if (m_freeMovementSmoothSpeed <= 0)
            {
                camTransform.position = m_targetPosition;
            }
            else
            {
                if (!MathHelper.Approximately(m_targetPosition, camTransform.position))
                {
                    Vector3 newPosition = Vector3.Lerp(
                        camTransform.position,
                        m_targetPosition,
                        m_freeMovementSmoothSpeed * Time.deltaTime);

                    camTransform.position = newPosition;
                }
            }

            Vector3 newPivot = camTransform.position + camTransform.forward * m_orbitDistance;

            bool recalculateOrbitDistance = ClampPosition(camTransform.position, ref newPivot);
            SecondaryPivotTransform.position += newPivot - Pivot;
            PivotTransform.position = newPivot;

            if (recalculateOrbitDistance)
            {
                RecalculateOrbitDistance(camTransform.position);
            }
        }

        private void OnSceneGizmoOrientationChanging()
        {
            m_lockInput = true;
            m_isSceneGizmoOrientationChanging = true;

            if (IsOrthographic && Grid != null)
            {
                Grid.Alpha = 0;
            }
        }

        private void OnSceneGizmoOrientationChanged()
        {
            m_lockInput = false;
            m_isSceneGizmoOrientationChanging = false;

            Pivot = Window.Camera.transform.position + Window.Camera.transform.forward * m_orbitDistance;
            SecondaryPivot = Pivot;

            m_targetRotation = Window.Camera.transform.rotation;
            m_targetPosition = Window.Camera.transform.position;
        }

        private void OnSceneGizmoProjectionChanged()
        {
            float fov = Window.Camera.fieldOfView * Mathf.Deg2Rad;
            float distance = (Window.Camera.transform.position - Pivot).magnitude;
            float objSize = distance * Mathf.Sin(fov / 2);
            Window.Camera.orthographicSize = objSize;

            if (!IsOrthographic && Grid != null)
            {
                Grid.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (IsGridCloseToCamera())
                {
                    Grid.Alpha = 0;
                }
            }
        }


        [Obsolete]
        public Vector3 CenterPoint(Vector3[] vectors)
        {
            return TransformUtility.CenterPoint(vectors);
        }

        private bool GetPointOnDragPlane(Vector3 mouse, out Vector3 point)
        {
            Ray ray = Window.Camera.ScreenPointToRay(mouse);
            float distance;
            if (m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private bool ClampPosition(Vector3 origin, ref Vector3 position)
        {
            if (position.magnitude <= m_boundingSphereRadius)
            {
                return false;
            }
            Vector3 toOrigin = origin - position;
            if (toOrigin == Vector3.zero)
            {
                position = position.normalized * m_boundingSphereRadius;
            }
            else
            {
                Vector3 dir = toOrigin.normalized;
                float distance = MathHelper.RaySphereIntertsect(origin, dir, Vector3.zero, m_boundingSphereRadius);
                if (distance == -1.0f)
                {
                    position = position.normalized * m_boundingSphereRadius;
                }
                else
                {
                    position = origin + dir * distance;
                }
            }

            return true;
        }

        private void RecalculateOrbitDistance(Vector3 position)
        {
            m_orbitDistance = (Pivot - position).magnitude;
        }
    }
}
