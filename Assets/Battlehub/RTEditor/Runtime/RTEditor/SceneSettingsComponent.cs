using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface ISceneSettingsComponent
    {
        event PropertyChangedEventHandler<bool> IsUserDefinedChanged;
        event PropertyChangedEventHandler<bool> Is2DModeChanged;

        /// <summary>
        /// Allow user to change settings using interface (Scene Settings Panel)
        /// </summary>
        bool IsUserDefined
        {
            get;
            set;
        }

        /// <summary>
        /// Is scene in 2D mode
        /// </summary>
        bool Is2DMode
        {
            get;
            set;
        }

        //Other settings here: Grid settings, Scene camera settings
    }

    [DefaultExecutionOrder(-40)]
    public class SceneSettingsComponent : RTEComponent, ISceneSettingsComponent
    {
        public event PropertyChangedEventHandler<bool> IsUserDefinedChanged;
        public event PropertyChangedEventHandler<bool> Is2DModeChanged;

        private bool m_isUserDefined = false;
        public bool IsUserDefined 
        {
            get { return m_isUserDefined; }
            set
            {
                if(m_isUserDefined != value)
                {
                    bool oldValue = m_isUserDefined;
                    m_isUserDefined = value;
                    IsUserDefinedChanged?.Invoke(this, oldValue, value);
                }
                
            }
        }

        private RuntimeSceneComponentSettings m_3DSettings;
        private RuntimeSceneComponentSettings m_2DSettings;
        private bool m_is2DMode = false;
        public bool Is2DMode
        {
            get { return m_is2DMode; }
            set
            {
                if(m_is2DMode != value)
                {
                    bool oldValue = m_is2DMode;
                    m_is2DMode = value;
                    ApplyToRuntimeSceneComponent();
                    Is2DModeChanged?.Invoke(this, oldValue, value);
                }
            }
        }

        private void ApplyToRuntimeSceneComponent()
        {
            IRuntimeSceneComponent sceneComponent = Window.IOCContainer.Resolve<IRuntimeSceneComponent>();
            if (Is2DMode)
            {
                m_3DSettings.ReadFrom(sceneComponent);
                m_2DSettings.WriteTo(sceneComponent);
            }
            else
            {
                m_2DSettings.ReadFrom(sceneComponent);
                m_3DSettings.WriteTo(sceneComponent);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Window.IOCContainer.RegisterFallback<ISceneSettingsComponent>(this);
        }

        protected override void Start()
        {
            base.Start();
            IRuntimeSceneComponent sceneComponent = Window.IOCContainer.Resolve<IRuntimeSceneComponent>();
            m_3DSettings = new RuntimeSceneComponentSettings(sceneComponent);
            m_2DSettings = new RuntimeSceneComponentSettings
            {
                ChangeOrthographicSizeOnly = true,
                IsSceneGizmoEnabled = false,
                Pivot = Vector3.zero,
                CameraPosition = Vector3.forward * -500,
                IsOrthographic = true,
                OrthographicSize = 5,
                CanRotate = false,
                CanFreeMove = false,
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Window.IOCContainer.UnregisterFallback<ISceneSettingsComponent>(this);
        }

        private class RuntimeSceneComponentSettings
        {
            public bool IsSceneGizmoEnabled = true;
            public bool IsOrthographic = false;
            public bool ChangeOrthographicSizeOnly = false;
            public bool CanRotate = true;
            public bool CanFreeMove = true;

            public float OrthographicSize = 5;
            public Vector3 Pivot = Vector3.zero;
            public Vector3 CameraPosition = Vector3.one * 10;

            public RuntimeSceneComponentSettings()
            {
            }

            public RuntimeSceneComponentSettings(IRuntimeSceneComponent sceneComponent)
            {
                ReadFrom(sceneComponent);
            }

            public void ReadFrom(IRuntimeSceneComponent sceneComponent)
            {
                ChangeOrthographicSizeOnly = sceneComponent.ChangeOrthographicSizeOnly;
                IsSceneGizmoEnabled = sceneComponent.IsSceneGizmoEnabled;
                Pivot = sceneComponent.Pivot;
                CameraPosition = sceneComponent.CameraPosition;
                IsOrthographic = sceneComponent.IsOrthographic;
                OrthographicSize = sceneComponent.OrthographicSize;
                CanRotate = sceneComponent.CanRotate;
                CanFreeMove = sceneComponent.CanFreeMove;
            }

            public void WriteTo(IRuntimeSceneComponent sceneComponent)
            {
                sceneComponent.ChangeOrthographicSizeOnly = ChangeOrthographicSizeOnly;
                sceneComponent.IsSceneGizmoEnabled = IsSceneGizmoEnabled;
                sceneComponent.Pivot = Pivot;
                sceneComponent.CameraPosition = CameraPosition;
                sceneComponent.IsOrthographic = IsOrthographic;
                sceneComponent.OrthographicSize = OrthographicSize;
                sceneComponent.CanRotate = CanRotate;
                sceneComponent.CanFreeMove = CanFreeMove;
            }
        }
    }
}

