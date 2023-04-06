using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class GameView : RuntimeCameraWindow
    {
        [SerializeField]
        private GameObject m_noCamerasRenderingTxt = null;
        private List<GameViewCamera> m_gameCameras;
        private List<RenderTextureCamera> m_renderTextureCameras;

        [SerializeField]
        private RectTransform m_renderTextureOutput = null;

        //When default layout button clicked Awake for new GameView invoked before OnDestroy for previous => camera disabled unintentionally
        //This static variable (m_gameView) needed to fix this issue. Probably DestroyImmediate also could work...
        private static GameView m_gameView;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Game;

            m_gameCameras = Editor.Object.Get(false).Select(obj => obj.GetComponent<GameViewCamera>()).Where(obj => obj != null && obj.IsAwaked).ToList();

            if (m_gameCameras.Count > 0)
            {
                m_camera = m_gameCameras[0].Camera;
            }

            if (m_renderTextureOutput != null && RenderPipelineInfo.UseRenderTextures)
            {
                m_renderTextureCameras = new List<RenderTextureCamera>();
                for (int i = 0; i < m_gameCameras.Count; ++i)
                {
                    GameViewCamera gameViewCamera = m_gameCameras[i];
                    CreateRenderTextureCamera(gameViewCamera);
                }
            }

            UpdateVisualState();

            GameViewCamera._Awaked += OnCameraAwaked;
            GameViewCamera._Destroyed += OnCameraDestroyed;
            GameViewCamera._Enabled += OnCameraEnabled;
            GameViewCamera._Disabled += OnCameraDisabled;
            GameViewCamera._CameraEnabled += OnCameraComponentEnabled;
            GameViewCamera._CameraDisabled += OnCameraComponentDisabled;

            m_gameView = this;

            base.AwakeOverride();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            GameViewCamera._Awaked -= OnCameraAwaked;
            GameViewCamera._Destroyed -= OnCameraDestroyed;
            GameViewCamera._Enabled -= OnCameraEnabled;
            GameViewCamera._Disabled -= OnCameraDisabled;
            GameViewCamera._CameraEnabled -= OnCameraComponentEnabled;
            GameViewCamera._CameraDisabled -= OnCameraComponentDisabled;

            if (m_gameView == this)
            {
                for (int i = 0; i < m_gameCameras.Count; ++i)
                {
                    GameViewCamera gameCamera = m_gameCameras[i];
                    if (gameCamera != null)
                    {
                        gameCamera.Camera.depth = gameCamera.Depth;
                        gameCamera.Camera.rect = gameCamera.Rect;
                    }
                }
                m_gameView = null;

                if (m_renderTextureCameras != null)
                {
                    for (int i = 0; i < m_renderTextureCameras.Count; ++i)
                    {
                        RenderTextureCamera renderTextureCamera = m_renderTextureCameras[i];
                        if (renderTextureCamera != null)
                        {
                            Destroy(renderTextureCamera);
                        }
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                gameCamera.enabled = true;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_gameView == this)
            {
                for (int i = 0; i < m_gameCameras.Count; ++i)
                {
                    GameViewCamera gameCamera = m_gameCameras[i];
                    if (gameCamera.Camera != null)
                    {
                        if (Editor.IsOpened)
                        {
                            gameCamera.enabled = false;
                        }
                    }
                }
            }
        }

        public override void SetCameraDepth(int depth)
        {
            base.SetCameraDepth(depth);
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                gameCamera.Camera.depth = depth + gameCamera.Depth;
            }
        }

        private void OnCameraAwaked(GameViewCamera gameCamera)
        {
            gameCamera.Camera.depth = CameraDepth + gameCamera.Depth;
            m_gameCameras.Add(gameCamera);
            if (m_renderTextureCameras != null)
            {
                CreateRenderTextureCamera(gameCamera);
            }

            if (Camera == null)
            {
                Camera = gameCamera.Camera;
            }

            UpdateVisualState();
            if (Editor.IsOpened && gameCamera.Camera != null)
            {
                SetCullingMask(gameCamera.Camera);
                HandleResize();
            }
        }

        private void OnCameraEnabled(GameViewCamera gameCamera)
        {
            UpdateVisualState();
        }

        private void OnCameraDisabled(GameViewCamera gameCamera)
        {
            UpdateVisualState();
        }

        private void OnCameraComponentEnabled(GameViewCamera gameCamera)
        {
            UpdateVisualState();
        }

        private void OnCameraComponentDisabled(GameViewCamera gameCamera)
        {
            UpdateVisualState();
        }

        private void OnCameraDestroyed(GameViewCamera camera)
        {
            int index = m_gameCameras.IndexOf(camera);
            m_gameCameras.RemoveAt(index);
            if (m_renderTextureCameras != null)
            {
                m_renderTextureCameras.RemoveAt(index);
            }

            if (m_gameCameras.Count > 0)
            {
                Camera = m_gameCameras[0].Camera;
            }
            else
            {
                Camera = null;
            }
            UpdateVisualState();
        }

        protected override void SetCullingMask()
        {
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                SetCullingMask(m_gameCameras[i].Camera);
            }
        }

        protected override void ResetCullingMask()
        {
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                ResetCullingMask(m_gameCameras[i].Camera);
            }
        }

        protected override void ResizeCamera(Rect pixelRect)
        {
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                Rect r = gameCamera.Rect;
                gameCamera.Camera.pixelRect = new Rect(pixelRect.x + r.x * pixelRect.width, pixelRect.y + r.y * pixelRect.height, r.width * pixelRect.width, r.height * pixelRect.height);
            }
        }

        private void UpdateVisualState()
        {
            m_noCamerasRenderingTxt.SetActive(m_gameCameras.Count == 0 || m_gameCameras.All(c => !c.gameObject.activeSelf || !c.Camera || !c.Camera.enabled));
        }

        private void CreateRenderTextureCamera(GameViewCamera gameViewCamera)
        {
            GameObject gameViewCameraGo = gameViewCamera.gameObject;
            bool wasActive = gameViewCameraGo.activeSelf;
            gameViewCameraGo.SetActive(false);

            RenderTextureCamera renderTextureCamera = gameViewCameraGo.GetComponent<RenderTextureCamera>();
            if (renderTextureCamera != null)
            {
                Destroy(renderTextureCamera);
            }

            renderTextureCamera = gameViewCameraGo.AddComponent<RenderTextureCamera>();
            renderTextureCamera.OutputRoot = m_renderTextureOutput;
            m_renderTextureCameras.Add(renderTextureCamera);

            gameViewCameraGo.SetActive(wasActive);
        }

        protected override void SetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask &= settings.RaycastMask;
            RenderPipelineInfo.XRFix(camera);
        }

        protected override void ResetCullingMask(Camera camera)
        {
            CameraLayerSettings settings = Editor.CameraLayerSettings;
            camera.cullingMask |= ~settings.RaycastMask;
        }

        protected override void RegisterGraphicsCamera()
        {
        }
        protected override void UnregisterGraphicsCamera()
        {
        }
    }

}

