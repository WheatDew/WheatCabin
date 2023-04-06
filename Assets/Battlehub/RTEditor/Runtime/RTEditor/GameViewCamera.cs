using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public delegate void GameViewCameraEventHandler(GameViewCamera camera);

    [RequireComponent(typeof(ExposeToEditor))]
    [RequireComponent(typeof(Camera))]
    public class GameViewCamera : MonoBehaviour
    {
        public static event GameViewCameraEventHandler _Awaked;
        public static event GameViewCameraEventHandler _Enabled;
        public static event GameViewCameraEventHandler _Disabled;
        public static event GameViewCameraEventHandler _Destroyed;
        public static event GameViewCameraEventHandler _CameraEnabled;
        public static event GameViewCameraEventHandler _CameraDisabled;

        [SerializeField]
        private Rect m_rect = new Rect(0, 0, 1, 1);
        public Rect Rect
        {
            get { return m_rect; }
            set { m_rect = value; }
        }

        [SerializeField]
        private int m_depth = 0;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        public bool IsAwaked
        {
            get;
            private set;
        }

        public bool IsCameraEnabled
        {
            get { return m_isCameraEnabled; }
            set { m_isCameraEnabled = value; }
        }

        private Camera m_camera;
        public Camera Camera
        {
            get { return m_camera; }
        }

        private void Awake()
        {
            m_rect.x = Mathf.Clamp01(m_rect.x);
            m_rect.y = Mathf.Clamp01(m_rect.y);
            m_rect.width = Mathf.Clamp01(m_rect.width);
            m_rect.height = Mathf.Clamp01(m_rect.height);

            m_camera = GetComponent<Camera>();
            if (m_camera == null)
            {
                Debug.LogError("Camera is null");
                Destroy(gameObject);
                return;
            }

            m_isCameraEnabled = m_camera.enabled;

            IsAwaked = true;

            if (_Awaked != null)
            {
                _Awaked(this);
            }

            if (IsCameraEnabled)
            {
                if (_CameraEnabled != null)
                {
                    _CameraEnabled(this);
                }
            }
        }

        private void OnEnable()
        {
            if (_Enabled != null)
            {
                _Enabled(this);
            }
        }

        private void OnDisable()
        {
            if (_Disabled != null)
            {
                _Disabled(this);
            }
        }

        private void OnDestroy()
        {
            if (_Destroyed != null)
            {
                _Destroyed(this);
            }
        }

        private bool m_isCameraEnabled;
        private void Update()
        {
            if (Camera == null)
            {
                Destroy(this);
                return;
            }

            if (m_isCameraEnabled != m_camera.enabled)
            {
                m_isCameraEnabled = m_camera.enabled;
                if (m_isCameraEnabled)
                {
                    if (_CameraEnabled != null)
                    {
                        _CameraEnabled(this);
                    }
                }
                else
                {
                    if (_CameraDisabled != null)
                    {
                        _CameraDisabled(this);
                    }
                }
            }
        }
    }
}
