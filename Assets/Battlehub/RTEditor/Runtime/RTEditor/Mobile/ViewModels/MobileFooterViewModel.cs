using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Models;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileFooterViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_isRootSelection;
        [Binding]
        public bool IsRootSelection
        {
            get { return m_isRootSelection; }
            set
            {
                if(m_isRootSelection != value)
                {
                    m_isRootSelection = value;
                    RaisePropertyChanged(nameof(IsRootSelection));

                    m_rte.Tools.SelectionMode = m_isRootSelection ? SelectionMode.Root : SelectionMode.Part;
                }
            }
        }

        private bool m_isLocalPivotMode;

        [Binding]
        public bool IsLocalPivotMode
        {
            get { return m_isLocalPivotMode; }
            set
            {
                if (m_isLocalPivotMode != value)
                {
                    m_isLocalPivotMode = value;
                    RaisePropertyChanged(nameof(IsLocalPivotMode));

                    m_rte.Tools.PivotMode = m_isLocalPivotMode ? RuntimePivotMode.Pivot : RuntimePivotMode.Center;
                }
            }
        }

        private bool m_isGlobalPivotRotation;

        [Binding]
        public bool IsGlobalPivotRotation
        {
            get { return m_isGlobalPivotRotation; }
            set
            {
                if (m_isGlobalPivotRotation != value)
                {
                    m_isGlobalPivotRotation = value;
                    RaisePropertyChanged(nameof(IsGlobalPivotRotation));

                    m_rte.Tools.PivotRotation = m_isGlobalPivotRotation ? RuntimePivotRotation.Global : RuntimePivotRotation.Local;
                }
            }
        }

        private bool m_isPositionHandle;

        [Binding]
        public bool IsPositionHandle
        {
            get { return m_isPositionHandle; }
            set
            {
                if (m_isPositionHandle != value)
                {
                    m_isPositionHandle = value;
                    RaisePropertyChanged(nameof(IsPositionHandle));

                    if(m_isPositionHandle)
                    {
                        m_rte.Tools.Current = RuntimeTool.Move;
                    }
                    else
                    {
                        ResetCurrentTool();
                    }
                }
            }
        }

        private bool m_isRotationHandle;

        [Binding]
        public bool IsRotationHandle
        {
            get { return m_isRotationHandle; }
            set
            {
                if (m_isRotationHandle != value)
                {
                    m_isRotationHandle = value;
                    RaisePropertyChanged(nameof(IsRotationHandle));

                    if(m_isRotationHandle)
                    {
                        m_rte.Tools.Current = RuntimeTool.Rotate;
                    }
                    else
                    {
                        ResetCurrentTool();
                    }
                    
                }
            }
        }

        private bool m_isScaleHandle;

        [Binding]
        public bool IsScaleHandle
        {
            get { return m_isScaleHandle; }
            set
            {
                if (m_isScaleHandle != value)
                {
                    m_isScaleHandle = value;
                    RaisePropertyChanged(nameof(IsScaleHandle));

                    if (m_isScaleHandle)
                    {
                        m_rte.Tools.Current = RuntimeTool.Scale;
                    }
                    else
                    {
                        ResetCurrentTool();
                    }
                }
            }
        }

        private bool m_isRectTool;

        [Binding]
        public bool IsRectTool
        {
            get { return m_isRectTool; }
            set
            {
                if (m_isRectTool != value)
                {
                    m_isRectTool = value;
                    RaisePropertyChanged(nameof(IsRectTool));

                    if (m_isRectTool)
                    {
                        m_rte.Tools.Current = RuntimeTool.Rect;
                    }
                    else
                    {
                        ResetCurrentTool();
                    }
                }
            }
        }

        private void ResetCurrentTool()
        {
            try
            {
                m_handleToolChanged = false;
                m_rte.Tools.Current = RuntimeTool.View;
            }
            finally
            {
                m_handleToolChanged = true;
            }
        }

        private bool m_isInspectorVisible;

        [Binding]
        public bool IsInspectorVisible
        {
            get { return m_isInspectorVisible; }
            set
            {
                if (m_isInspectorVisible != value)
                {
                    m_isInspectorVisible = value;
                    RaisePropertyChanged(nameof(IsInspectorVisible));

                    if(m_mobileEditorModel != null)
                    {
                        m_mobileEditorModel.IsInspectorOpened = value;
                    }
                }
            }
        }

        private bool m_canOpenInspector;

        [Binding]
        public bool CanOpenInspector
        {
            get { return m_canOpenInspector; }
            set
            {
                if(m_canOpenInspector != value)
                {
                    m_canOpenInspector = value;
                    RaisePropertyChanged(nameof(CanOpenInspector));
                }
            }
        }

        private bool m_handleToolChanged = true;
        private IRTE m_rte;
        private IMobileEditorModel m_mobileEditorModel;

        private void Start()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_rte.Tools.ToolChanged += OnToolChanged;
            m_rte.Tools.PivotRotationChanged += OnPivotRotationChanged;
            m_rte.Tools.PivotModeChanged += OnPivotModeChanged;
            m_rte.Tools.SelectionModeChanged += OnSelectionModeChanged;
            
            m_mobileEditorModel = IOC.Resolve<IMobileEditorModel>();
            CanOpenInspector = m_mobileEditorModel != null;
            if (m_mobileEditorModel != null)
            {
                IsInspectorVisible = m_mobileEditorModel.IsInspectorOpened;
                m_mobileEditorModel.IsInspectorOpenedChanged += OnIsInspectorVisibleChanged;
            }
            
            IsRootSelection = m_rte.Tools.SelectionMode == SelectionMode.Root;
            IsGlobalPivotRotation = m_rte.Tools.PivotRotation == RuntimePivotRotation.Global;
            IsLocalPivotMode = m_rte.Tools.PivotMode == RuntimePivotMode.Pivot;
            IsPositionHandle = m_rte.Tools.Current == RuntimeTool.Move;
            IsRotationHandle = m_rte.Tools.Current == RuntimeTool.Rotate;
            IsScaleHandle = m_rte.Tools.Current == RuntimeTool.Scale;
            IsRectTool = m_rte.Tools.Current == RuntimeTool.Rect;
        }

        private void OnDestroy()
        {
            if(m_rte != null)
            {
                m_rte.Tools.SelectionModeChanged -= OnSelectionModeChanged;
                m_rte.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                m_rte.Tools.PivotModeChanged -= OnPivotModeChanged;
                m_rte.Tools.ToolChanged -= OnToolChanged;
                
                m_rte = null;
            }

            if(m_mobileEditorModel != null)
            {
                m_mobileEditorModel.IsInspectorOpenedChanged -= OnIsInspectorVisibleChanged;
                m_mobileEditorModel = null;
            }
        }

        private void OnPivotRotationChanged()
        {
            IsGlobalPivotRotation = m_rte.Tools.PivotRotation == RuntimePivotRotation.Global;
        }

        private void OnPivotModeChanged()
        {
            IsLocalPivotMode = m_rte.Tools.PivotMode == RuntimePivotMode.Pivot;
        }

        private void OnSelectionModeChanged()
        {
            IsRootSelection = m_rte.Tools.SelectionMode == SelectionMode.Root;
        }

        private void OnToolChanged()
        {
            if (!m_handleToolChanged)
            {
                return;
            }

            m_isPositionHandle = m_rte.Tools.Current == RuntimeTool.Move;
            m_isRotationHandle = m_rte.Tools.Current == RuntimeTool.Rotate;
            m_isScaleHandle = m_rte.Tools.Current == RuntimeTool.Scale;
            m_isRectTool = m_rte.Tools.Current == RuntimeTool.Rect;

            RaisePropertyChanged(nameof(IsPositionHandle));
            RaisePropertyChanged(nameof(IsRotationHandle));
            RaisePropertyChanged(nameof(IsScaleHandle));
            RaisePropertyChanged(nameof(IsRectTool));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnIsInspectorVisibleChanged(object sender, ValueChangedArgs<bool> e)
        {
            IsInspectorVisible = e.NewValue;
        }
    }

}

