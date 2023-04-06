using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class MobileSceneInput : RuntimeSelectionInputBase
    {
        public float RotateSensitivity = 0.25f;
        public float ZoomSensitivity = 0.005f;
        public float MoveSensitivity = 0.1f;
 
        [SerializeField]
        private MobileSceneControls m_sceneControls = null;
       
        private bool m_isActive = false;
        private MobileSceneControls.Mode m_mode;
        private bool m_zoom = false;
        private bool m_pan = false;
        private bool m_cameraTransformChanged = false;
        private bool m_isDragging = false;

        private Vector3 m_defaultCameraPosition;
        private Vector3 m_defaultPivot;
        private Vector3 m_betweenTouches;
        private Vector3 m_prevCameraPosition;
        private Vector3 m_prevPivot;

        protected RuntimeSceneComponent SceneComponent
        {
            get { return (RuntimeSceneComponent)m_component; }
        }

        protected override void Start()
        {
            base.Start();
            m_defaultCameraPosition = SceneComponent.CameraPosition;
            m_defaultPivot = SceneComponent.Pivot;

            SceneComponent.BoxSelection.Selection += OnBoxSelection;
            SceneComponent.Editor.ActiveWindowChanged += Editor_ActiveWindowChanged;
            if (m_sceneControls == null)
            {
                m_sceneControls = Resources.Load<MobileSceneControls>("RTH_MobileSceneControls");
                if (m_sceneControls != null)
                {
                    RuntimeCameraWindow scene = (RuntimeCameraWindow)SceneComponent.Window;
                    m_sceneControls = Instantiate(m_sceneControls, scene.ViewRoot, false);
                }
            }

            if (m_sceneControls != null)
            {
                m_sceneControls.Focus += OnFocus;
                m_sceneControls.ModeChanged += OnModeChanged;
                m_sceneControls.ResetPosition += OnResetPosition;

                m_mode = m_sceneControls.CurrentMode;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (SceneComponent != null)
            {
                if(SceneComponent.Editor != null)
                {
                    SceneComponent.Editor.ActiveWindowChanged -= Editor_ActiveWindowChanged;
                }
                SceneComponent.BoxSelection.Selection -= OnBoxSelection;
            }

            if (m_sceneControls != null)
            {
                m_sceneControls.Focus -= OnFocus;
                m_sceneControls.ModeChanged -= OnModeChanged;
                m_sceneControls.ResetPosition -= OnResetPosition;
            }
        }

        private void Editor_ActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (SceneComponent != null)
            {
                if (m_isActive)
                {
                    m_zoom = false;
                }
                m_isActive = SceneComponent.IsWindowActive;
            }
        }

        private void OnFocus()
        {
            SceneComponent.Focus(FocusMode.Default);
        }

        private void OnBoxSelection(object sender, BoxSelectionArgs e)
        {
            if(m_sceneControls != null)
            {
                m_sceneControls.Cancel();
            }
        }

        private void OnModeChanged(MobileSceneControls.Mode mode)
        {
            m_mode = mode;
            SceneComponent.IsBoxSelectionEnabled = m_mode == MobileSceneControls.Mode.BoxSelection;
        }

        private void OnResetPosition()
        {
            SceneComponent.SecondaryPivot = m_defaultPivot;
            SceneComponent.Pivot = m_defaultPivot;
            SceneComponent.CameraPosition = m_defaultCameraPosition;
        }

        protected override void SelectGO()
        {
            RuntimeTools tools = m_component.Editor.Tools;
            IRuntimeSelection selection = m_component.Selection;

            if (tools.ActiveTool != null && tools.ActiveTool != m_component.BoxSelection)
            {
                return;
            }

            if (!selection.Enabled)
            {
                return;
            }

            OnSelectGO();
        }

        protected override void BeginSelectAction()
        {
            if(!SceneComponent.Window.IsPointerOver)
            {
                return;
            }
            
            base.BeginSelectAction();
        }


        protected virtual bool BeginDrag()
        {
            if (!SceneComponent.Window.IsPointerOver)
            {
                return false;
            }

            ITouchInput input = SceneComponent.Editor.TouchInput;
            return input.TouchCount > 0 && input.GetTouch(0).phase == TouchPhase.Began;
        }

        protected virtual bool EndDrag()
        {
            ITouchInput input = SceneComponent.Editor.TouchInput;
            if(input.TouchCount == 0)
            {
                return true;
            }

            TouchPhase phase = input.GetTouch(0).phase;
            return phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
        }

        protected virtual bool MoveAction()
        {
            if (!SceneComponent.Window.IsPointerOver || !m_isDragging)
            {
                return false;
            }

            ITouchInput input = SceneComponent.Editor.TouchInput;
            return input.TouchCount == 1 && input.GetTouch(0).tapCount == 2 && !SceneComponent.IsOrthographic;
        }

        protected virtual bool PanAction()
        {
            if (!SceneComponent.Window.IsPointerOver || !m_isDragging)
            {
                return false;
            }

            IRTE editor = SceneComponent.Editor;
            ITouchInput input = editor.TouchInput;
            return input.TouchCount == 1 && (m_mode == MobileSceneControls.Mode.Pan || SceneComponent.IsOrthographic) && !SceneComponent.IsBoxSelectionEnabled;
        }

        protected virtual bool RotateAction()
        {
            if (!SceneComponent.Window.IsPointerOver || !m_isDragging)
            {
                return false;
            }

            ITouchInput input = SceneComponent.Editor.TouchInput;
            return m_mode == MobileSceneControls.Mode.View && input.TouchCount == 1 && !SceneComponent.IsOrthographic;
        }

        protected virtual bool OrbitAction()
        {
            if (!SceneComponent.Window.IsPointerOver || !m_isDragging)
            {
                return false;
            }

            ITouchInput input = SceneComponent.Editor.TouchInput;
            return m_mode == MobileSceneControls.Mode.Orbit && input.TouchCount == 1 && !SceneComponent.IsOrthographic;
        }

        protected virtual bool ZoomAction()
        {
            if (!SceneComponent.Window.IsPointerOver || !m_isDragging)
            {
                return false;
            }

            ITouchInput input = SceneComponent.Editor.TouchInput;
            return input.TouchCount == 2;
        }

        protected virtual Vector2 RotateAxes()
        {
            ITouchInput input = SceneComponent.Editor.TouchInput;
            return input.GetTouch(0).deltaPosition;
        }

        protected virtual float ZoomAxis()
        {
            ITouchInput input = SceneComponent.Editor.TouchInput;
            Vector3 t0 = input.GetTouch(0).position;
            Vector3 t1 = input.GetTouch(1).position;
            Vector3 betweenTouches = t0 - t1;

            float delta = betweenTouches.magnitude - m_betweenTouches.magnitude;
            m_betweenTouches = betweenTouches;
            return delta;
        }

        protected override void LateUpdate()
        {
            if(m_sceneControls != null)
            {
                m_sceneControls.IsOrthographicMode = SceneComponent.IsOrthographic;
            }

            RuntimeSceneComponent sceneComponent = SceneComponent;
            if (!sceneComponent.IsWindowActive)
            {
                m_isDragging = false;
                return;
            }

            RuntimeWindow window = sceneComponent.Window;
            ITouchInput input = sceneComponent.Editor.TouchInput;
            RuntimeTools tools = sceneComponent.Editor.Tools;
            if (tools.ActiveTool != null)
            {
                m_isDragging = false;
                return;
            }

            if(m_isDragging)
            {
                if(EndDrag())
                {
                    m_isDragging = false;
                }
            }
            else
            {
                if(BeginDrag())
                {
                    m_isDragging = true;
                }
            }

            bool zoom = ZoomAction();
            bool beginZoom = zoom != m_zoom && (m_zoom = zoom);
            bool pan = PanAction();
            bool beginPan = pan != m_pan && (m_pan = pan);
            bool rotate = RotateAction();
            bool orbit = OrbitAction();
            bool moveAction = MoveAction();            
            bool isViewing = tools.IsViewing;

            tools.IsViewing = pan || rotate || orbit || zoom;
            if (tools.IsViewing && tools.IsViewing != isViewing)
            {
                ResetCameraTransformChanged();
            }

            if (!m_cameraTransformChanged)
            { 
                Vector3 v0 = m_prevPivot - m_prevCameraPosition;
                Vector3 v1 = sceneComponent.Pivot - sceneComponent.CameraPosition;
                if (!Mathf.Approximately(v0.magnitude, v1.magnitude) || Vector3.Dot(v0.normalized, v1.normalized) < 0.999f)
                {
                    m_cameraTransformChanged = true;
                }

                if(pan)
                {
                    if(!MathHelper.Approximately(m_prevCameraPosition, sceneComponent.CameraPosition))
                    {
                        m_cameraTransformChanged = true;
                    }
                }
            }

            BeginSelectAction();
            if (!m_cameraTransformChanged && SelectAction())
            {
                SelectGO();
            }          

            if(zoom)
            {
                if (beginZoom)
                {
                    m_betweenTouches = input.GetTouch(0).position - input.GetTouch(1).position;
                }

                float deltaZ = ZoomAxis() * ZoomSensitivity;
                if (m_mode == MobileSceneControls.Mode.Orbit)
                {
                    sceneComponent.Orbit(0, 0, deltaZ);
                }
                else
                {
                    sceneComponent.Zoom(deltaZ, Quaternion.FromToRotation(Vector3.forward, 
                        window.Camera.transform.InverseTransformVector(window.Pointer.Ray.direction).normalized));
                }
                sceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);
            }
            else if(pan)
            {
                if(beginPan)
                {
                    sceneComponent.BeginPan(window.Pointer.ScreenPoint);
                }
                sceneComponent.Pan(window.Pointer.ScreenPoint);
            }
            else if(rotate)
            {
                Vector3 offset = Vector3.zero;
                if (moveAction)
                {
                    offset = Vector3.up * MoveSensitivity;
                }

                Vector2 deltaXY = RotateAxes() * RotateSensitivity;
                sceneComponent.FreeMove(deltaXY, offset, 0);
            }
            else if(orbit)
            {
                Vector2 deltaXY = RotateAxes() * RotateSensitivity;
                sceneComponent.Orbit(deltaXY.x, deltaXY.y, 0);
                sceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);
            }
            else
            {
                sceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);
            }
        }

        private void ResetCameraTransformChanged()
        {
            m_prevPivot = SceneComponent.Pivot;
            m_prevCameraPosition = SceneComponent.CameraPosition;
            m_cameraTransformChanged = false;
        }
    }

}
