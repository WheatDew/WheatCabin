using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public enum RenderTextureUsage
    {
        UsePipelineSettings,
        Off,
        On
    }

    public class RuntimeCameraWindow : RuntimeWindow
    {
        public event Action CameraResized;

        [SerializeField]
        private RenderTextureUsage m_renderTextureUsage = RenderTextureUsage.UsePipelineSettings;
        public RenderTextureUsage RenderTextureUsage
        {
            get { return m_renderTextureUsage; }
            set { m_renderTextureUsage = value; }
        }

        [SerializeField]
        protected Camera m_camera;
        public override Camera Camera
        {
            get { return m_camera; }
            set
            {
                if (m_camera == value)
                {
                    return;
                }

                if (m_camera != null)
                {
                    ResetCullingMask();
                    UnregisterGraphicsCamera();
                }

                m_camera = value;

                if (m_camera != null)
                {
                    SetCullingMask();
                    if (WindowType == RuntimeWindowType.Scene)
                    {
                        RegisterGraphicsCamera();
                    }

                    RenderPipelineInfo.XRFix(Camera);

                    m_camera.depth = m_cameraDepth;
                }
            }
        }

        private int m_cameraDepth;
        public int CameraDepth
        {
            get { return m_cameraDepth; }
        }

        public virtual void SetCameraDepth(int depth)
        {
            m_cameraDepth = depth;
            if (m_camera != null)
            {
                m_camera.depth = m_cameraDepth;
            }
        }


        private Vector3 m_position;
        private Rect m_rect;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            if (RenderPipelineInfo.Type != RPType.Standard)
            {
                RTEGraphicsLayer graphicsLayer = GetComponent<RTEGraphicsLayer>();
                DestroyImmediate(graphicsLayer);
            }
            
            if (Camera != null)
            {
                Image windowBackground = GetComponent<Image>();
                if (windowBackground != null)
                {
                    Color color = windowBackground.color;
                    color.a = 0;
                    windowBackground.color = color;
                }

                if (RenderTextureUsage == RenderTextureUsage.Off || RenderTextureUsage == RenderTextureUsage.UsePipelineSettings && !RenderPipelineInfo.UseRenderTextures)
                {
                    RenderTextureCamera renderTextureCamera = Camera.GetComponent<RenderTextureCamera>();
                    if (renderTextureCamera != null)
                    {
                        DestroyImmediate(renderTextureCamera);
                    }
                }

                RenderPipelineInfo.XRFix(Camera);
            }

            if (m_camera != null)
            {
                SetCullingMask();
                RegisterGraphicsCamera();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_camera != null)
            {
                ResetCullingMask();
                UnregisterGraphicsCamera();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TryResize();
        }

        protected virtual void Update()
        {
            UpdateOverride();
        }

        protected override void UpdateOverride()
        {
            TryResize();
        }

        protected virtual void RegisterGraphicsCamera()
        {
            IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
            if (graphics != null)
            {
                graphics.RegisterCamera(m_camera);
            }
        }

        protected virtual void UnregisterGraphicsCamera()
        {
            IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
            if (graphics != null)
            {
                graphics.UnregisterCamera(m_camera);
            }
        }

        public IRTECamera GetGraphicsCamera()
        {
            IRTECamera rteCamera;
            IRTEGraphicsLayer graphicsLayer = IOCContainer.Resolve<IRTEGraphicsLayer>();
            if (graphicsLayer != null)
            {
                rteCamera = graphicsLayer.Camera;
            }
            else
            {
                IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
                rteCamera = graphics.GetOrCreateCamera(Camera, CameraEvent.AfterImageEffectsOpaque);
            }

            return rteCamera;
        }

        private void TryResize()
        {
            if (m_camera != null && ViewRoot != null)
            {
                if (ViewRoot.rect != m_rect || ViewRoot.position != m_position)
                {
                    HandleResize();

                    m_rect = ViewRoot.rect;
                    m_position = ViewRoot.position;
                }
            }
        }

        public override void HandleResize()
        {
            if (m_camera == null)
            {
                return;
            }

            Canvas canvas = Canvas;
            if (ViewRoot != null && canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Vector3[] corners = new Vector3[4];
                    ViewRoot.GetWorldCorners(corners);
                    ResizeCamera(new Rect(corners[0], new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y)));
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    if (canvas.worldCamera != Camera)
                    {
                        Vector3[] corners = new Vector3[4];
                        ViewRoot.GetWorldCorners(corners);

                        corners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
                        corners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                        corners[2] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
                        corners[3] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);

                        Vector2 size = new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y);
                        ResizeCamera(new Rect(corners[0], size));
                    }
                }
            }
        }

        protected virtual void ResizeCamera(Rect pixelRect)
        {
            m_camera.pixelRect = pixelRect;
            if (CameraResized != null)
            {
                CameraResized();
            }
        }

        protected virtual void SetCullingMask()
        {
            SetCullingMask(m_camera);
        }

        protected virtual void ResetCullingMask()
        {
            ResetCullingMask(m_camera);
        }

        protected virtual void SetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask &= (settings.RaycastMask | 1 << settings.AllScenesLayer);
        }

        protected virtual void ResetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask |= ~(settings.RaycastMask | 1 << settings.AllScenesLayer);
        }

    }
}
