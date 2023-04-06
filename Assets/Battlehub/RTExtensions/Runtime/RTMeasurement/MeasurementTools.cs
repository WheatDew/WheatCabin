using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using TMPro;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public enum MeasurementToolType
    {
        None,
        Distance,
        Angle
    }

    public delegate void MeasurementToolsEventHandler();

    public interface IMeasurementTools
    {
        event MeasurementToolsEventHandler ToolChanged;

        MeasurementToolType Current
        {
            get;
            set;
        }
    }

    public class MeasurementTools : SceneComponentExtension, IMeasurementTools
    {
        public event MeasurementToolsEventHandler ToolChanged;

        private MeasurementToolType m_current;
        public MeasurementToolType Current
        {
            get { return m_current; }
            set
            {
                if(m_current != value)
                {
                    m_current = value;
                    m_editor.Tools.ToolChanged -= OnToolChanged;
                    if (m_current != MeasurementToolType.None)
                    {
                        m_editor.Tools.Custom = m_current;
                    }
                    else
                    {
                        m_editor.Tools.Custom = null;
                    }
                    m_editor.Tools.ToolChanged += OnToolChanged;

                    //if(m_output != null)
                    //{
                    //    m_output.gameObject.SetActive(m_current != MeasurementToolType.None);
                    //}
                    
                    m_distanceTool.gameObject.SetActive(m_current == MeasurementToolType.Distance);
                    m_angleTool.gameObject.SetActive(m_current == MeasurementToolType.Angle);

                    if (m_current != MeasurementToolType.None)
                    {
                        DisallowSelection();
                    }
                    else
                    {
                        AllowSelection();
                    }

                    if (ToolChanged != null)
                    {
                        ToolChanged();
                    }
                }
            }
        }

        private IRTE m_editor;

        [SerializeField]
        private MeasureDistanceTool m_distanceTool = null;

        [SerializeField]
        private MeasureAngleTool m_angleTool = null;

        //[SerializeField]
        //private TextMeshProUGUI m_output = null;

        private ISelectionComponentState m_selectionComponentState;
        private ISettingsComponent m_settingsComponent;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            IOC.RegisterFallback<IMeasurementTools>(this);

            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Tools.ToolChanged += OnToolChanged;

            m_settingsComponent = IOC.Resolve<ISettingsComponent>();
            if(m_settingsComponent != null)
            {
                m_settingsComponent.SystemOfMeasurementChanged += OnSystemOfMeasurementChanged;
            }

            if(m_distanceTool == null)
            {
                GameObject go = new GameObject("MeasureDistanceTool");
                go.transform.SetParent(transform, false);
                go.SetActive(false);

                m_distanceTool = go.AddComponent<MeasureDistanceTool>();
                //if(m_output != null)
                //{
                //    m_distanceTool.Output = m_output;
                //}
            }

            if(m_angleTool == null)
            {
                GameObject go = new GameObject("MeasureAngleTool");
                go.transform.SetParent(transform, false);
                go.SetActive(false);

                m_angleTool = go.AddComponent<MeasureAngleTool>();
                //if (m_output != null)
                //{
                //    m_angleTool.Output = m_output;
                //}
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            IOC.UnregisterFallback<IMeasurementTools>(this);
            if(m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnToolChanged;
            }

            if (m_settingsComponent != null)
            {
                m_settingsComponent.SystemOfMeasurementChanged -= OnSystemOfMeasurementChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<IMeasurementTools>(this);
            if (m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnToolChanged;
            }
            if (m_settingsComponent != null)
            {
                m_settingsComponent.SystemOfMeasurementChanged -= OnSystemOfMeasurementChanged;
            }
        }

        protected override void OnSceneActivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneActivated(sceneComponent);
            m_selectionComponentState = sceneComponent.Window.IOCContainer.Resolve<ISelectionComponentState>();

            if(Current != MeasurementToolType.None)
            {
                DisallowSelection();
            }
        }

        protected override void OnSceneDeactivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneDeactivated(sceneComponent);
            if (m_selectionComponentState != null)
            {
                AllowSelection();
            }
        }

        private void DisallowSelection()
        {
            if(m_selectionComponentState != null)
            {
                m_selectionComponentState.IsBoxSelectionEnabled(this, false);
                m_selectionComponentState.CanSelect(this, false);
                m_selectionComponentState.CanSelectAll(this, false);
            }
          
        }

        private void AllowSelection()
        {
            if (m_selectionComponentState != null)
            {
                m_selectionComponentState.IsBoxSelectionEnabled(this, true);
                m_selectionComponentState.CanSelect(this, true);
                m_selectionComponentState.CanSelectAll(this, false);
            }
        }

        private void OnToolChanged()
        {
            Current = MeasurementToolType.None;
        }

        private void OnSystemOfMeasurementChanged()
        {
            m_distanceTool.Metric = m_settingsComponent.SystemOfMeasurement == SystemOfMeasurement.Metric;
        }
    }

}
