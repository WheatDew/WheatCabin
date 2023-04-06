using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class ProBuilderToolbar : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_objectToggle = null;

        [SerializeField]
        private Toggle m_vetexToggle = null;

        [SerializeField]
        private Toggle m_edgeToggle = null;

        [SerializeField]
        private Toggle m_faceToggle = null;

        [SerializeField]
        private Toggle m_pivotModeToggle = null;

        [SerializeField]
        private Toggle m_pivotRotationToggle = null;

        [SerializeField]
        private ToggleGroup m_toggleGroup = null;

        private IProBuilderTool m_tool;
        private IRTE m_rte;

        protected virtual void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();

            if(m_objectToggle != null)
            {
                m_objectToggle.onValueChanged.AddListener(OnObject);
            }
            if(m_vetexToggle != null)
            {
                m_vetexToggle.onValueChanged.AddListener(OnVertex);
            }
            if(m_edgeToggle != null)
            {
                m_edgeToggle.onValueChanged.AddListener(OnEdge);
            }
            if(m_faceToggle != null)
            {
                m_faceToggle.onValueChanged.AddListener(OnFace);
            }
            if(m_pivotModeToggle != null)
            {
                m_pivotModeToggle.onValueChanged.AddListener(OnPivotMode);
            }
            if(m_pivotRotationToggle != null)
            {
                m_pivotRotationToggle.onValueChanged.AddListener(OnPivotRotation);
            }

            m_rte.Tools.PivotRotationChanged += OnPivotRotationChanged;
            m_rte.Tools.PivotModeChanged += OnPivotModeChanged;
        }

        protected virtual void Start()
        {
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_tool.ModeChanged += OnModeChanged;
        }

        protected virtual void OnDestroy()
        {
            if (m_tool != null)
            {
                m_tool.ModeChanged -= OnModeChanged;
            }

            if (m_objectToggle != null)
            {
                m_objectToggle.onValueChanged.RemoveListener(OnObject);
            }
            if (m_vetexToggle != null)
            {
                m_vetexToggle.onValueChanged.RemoveListener(OnVertex);
            }
            if (m_edgeToggle != null)
            {
                m_edgeToggle.onValueChanged.RemoveListener(OnEdge);
            }
            if (m_faceToggle != null)
            {
                m_faceToggle.onValueChanged.RemoveListener(OnFace);
            }
            if (m_pivotModeToggle != null)
            {
                m_pivotModeToggle.onValueChanged.RemoveListener(OnPivotMode);
            }
            if (m_pivotRotationToggle != null)
            {
                m_pivotRotationToggle.onValueChanged.RemoveListener(OnPivotRotation);
            }

            if (m_rte != null)
            {
                m_rte.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                m_rte.Tools.PivotModeChanged -= OnPivotModeChanged;
            }
        }

        private void OnObject(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Object;
            }
        }

        private void OnVertex(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Vertex;
            }
        }

        private void OnEdge(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Edge;
            }
        }

        private void OnFace(bool value)
        {
            if(value)
            {
                m_tool.Mode = ProBuilderToolMode.Face;
            }
        }

        private void OnPivotRotation(bool value)
        {
            IRTE rte = IOC.Resolve<IRTE>();
            rte.Tools.PivotRotation = value ? RuntimePivotRotation.Global : RuntimePivotRotation.Local;
        }

        private void OnPivotMode(bool value)
        {
            IRTE rte = IOC.Resolve<IRTE>();
            rte.Tools.PivotMode = value ? RuntimePivotMode.Center : RuntimePivotMode.Pivot;
        }

        private void OnPivotRotationChanged()
        {
            if (m_pivotRotationToggle != null)
            {
                m_pivotRotationToggle.isOn = m_rte.Tools.PivotRotation == RuntimePivotRotation.Global;
            }
        }

        private void OnPivotModeChanged()
        {
            if(m_pivotModeToggle != null)
            {
                m_pivotModeToggle.isOn = m_rte.Tools.PivotMode == RuntimePivotMode.Center;
            }
        }

        private void OnModeChanged(ProBuilderToolMode oldMode)
        {
            if (m_toggleGroup != null)
            {
                m_toggleGroup.allowSwitchOff = false;
            }
            switch (m_tool.Mode)
            {
                case ProBuilderToolMode.Object:
                    m_objectToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Vertex:
                    m_vetexToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Edge:
                    m_edgeToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Face:
                    m_faceToggle.isOn = true;
                    break;
                case ProBuilderToolMode.Custom:
                    if(m_toggleGroup != null)
                    {
                        m_toggleGroup.allowSwitchOff = true;
                    }
                    m_objectToggle.isOn = false;
                    m_vetexToggle.isOn = false;
                    m_edgeToggle.isOn = false;
                    m_faceToggle.isOn = false;
                    break;
            }
        }
    }

}

