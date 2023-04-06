using UnityEngine;
using UnityEngine.UI;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

namespace Battlehub.RTEditor
{
    [AddComponentMenu("") /*,Obsolete*/]
    public class ToolsPanel : RuntimeWindow
    {
        private bool m_handleValueChange = true;

        public Toggle ViewToggle;
        public Toggle MoveToggle;
        public Toggle RotateToggle;
        public Toggle ScaleToggle;
        public Toggle RectToggle;

        public Toggle PivotRotationToggle;
        public Toggle PivotModeToggle;
        public Toggle WireframeToggle;

        public Toggle AutoFocusToggle;
        public Toggle VertexSnappingToggle;
        public Toggle UnitSnappingToggle;

        public Toggle PlayToggle;

        public GameObject SaveSceneDialog;

        public Button BtnNew;
        public Button BtnSave;
        public Button BtnSaveAs;
        public Button BtnUndo;
        public Button BtnRedo;

        private IProjectAsync m_project;
        private IRuntimeEditor m_editor;
        private IWindowManager m_wm;
        
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ToolsPanel;
            base.AwakeOverride();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_project = IOC.Resolve<IProjectAsync>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_wm = IOC.Resolve<IWindowManager>();

            OnRuntimeToolChanged();
            OnPivotRotationChanged();
            OnPivotModeChanged();
            OnBoundingBoxSnappingChanged();
            OnUnitSnappingChanged();
            OnAutoFocusChanged();
            OnPlaymodeStateChanged();

            UpdateUndoRedoButtonsState();

            Editor.Tools.ToolChanged += OnRuntimeToolChanged;
            Editor.Tools.PivotRotationChanged += OnPivotRotationChanged;
            Editor.Tools.PivotModeChanged += OnPivotModeChanged;
            Editor.Tools.AutoFocusChanged += OnAutoFocusChanged;
            Editor.Tools.IsSnappingChanged += OnBoundingBoxSnappingChanged;
            Editor.Tools.UnitSnappingChanged += OnUnitSnappingChanged;
            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;

            if (m_editor != null)
            {
                m_editor.SceneLoaded += OnSceneLoaded;
                m_editor.SceneSaved += OnSceneSaved;
            }

            UpdateLoadSaveButtonsState();

            Editor.Undo.UndoCompleted += OnUndoCompleted;
            Editor.Undo.RedoCompleted += OnRedoCompleted;
            Editor.Undo.StateChanged += OnStateChanged;
            
            if (ViewToggle != null)
            {
                ViewToggle.onValueChanged.AddListener(OnViewToggleValueChanged);
            }
            if (MoveToggle != null)
            {
                MoveToggle.onValueChanged.AddListener(OnMoveToggleValueChanged);
            }
            if (RotateToggle != null)
            {
                RotateToggle.onValueChanged.AddListener(OnRotateToggleValueChanged);
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.onValueChanged.AddListener(OnScaleToggleValueChanged);
            }
            if(RectToggle != null)
            {
                RectToggle.onValueChanged.AddListener(OnRectToggleValueChanged);
            }
            if(PivotRotationToggle != null)
            {
                PivotRotationToggle.onValueChanged.AddListener(OnPivotRotationToggleValueChanged);
            }
            if(PivotModeToggle != null)
            {
                PivotModeToggle.onValueChanged.AddListener(OnPivotModeToggleValueChanged);
            }
            if(WireframeToggle != null)
            {
                WireframeToggle.onValueChanged.AddListener(OnWireframeToggleValueChanged);
            }
            if(UnitSnappingToggle != null)
            {
                UnitSnappingToggle.onValueChanged.AddListener(OnUnitSnappingToggleValueChanged);
            }
            if(VertexSnappingToggle != null)
            {
                VertexSnappingToggle.onValueChanged.AddListener(OnBoundingBoxSnappingToggleValueChanged);
            }
            if(AutoFocusToggle != null)
            {
                AutoFocusToggle.onValueChanged.AddListener(OnAutoFocusToggleValueChanged);
            }
            if(PlayToggle != null)
            {
                PlayToggle.onValueChanged.AddListener(OnPlayToggleValueChanged);
            }

            if(BtnSave != null)
            {
                BtnSave.onClick.AddListener(OnSaveClick);
            }

            if(BtnSaveAs != null)
            {
                BtnSaveAs.onClick.AddListener(OnSaveAsClick);
            }

            if(BtnNew != null)
            {
                BtnNew.onClick.AddListener(OnNewClick);
            }
            if(BtnUndo != null)
            {
                BtnUndo.onClick.AddListener(OnUndoClick);
            }
            if(BtnRedo != null)
            {
                BtnRedo.onClick.AddListener(OnRedoClick);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        
            if(Editor != null)
            {
                Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
                Editor.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                Editor.Tools.PivotModeChanged -= OnPivotModeChanged;
                Editor.Tools.AutoFocusChanged -= OnAutoFocusChanged;
                Editor.Tools.UnitSnappingChanged -= OnUnitSnappingChanged;
                Editor.Tools.IsSnappingChanged -= OnBoundingBoxSnappingChanged;
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                Editor.Undo.UndoCompleted -= OnUndoCompleted;
                Editor.Undo.RedoCompleted -= OnRedoCompleted;
                Editor.Undo.StateChanged -= OnStateChanged;
            }

            if (m_editor != null)
            {
                m_editor.SceneLoaded -= OnSceneLoaded;
                m_editor.SceneSaved -= OnSceneSaved;
            }

            if (ViewToggle != null)
            {
                ViewToggle.onValueChanged.RemoveListener(OnViewToggleValueChanged);
            }
            if (MoveToggle != null)
            {
                MoveToggle.onValueChanged.RemoveListener(OnMoveToggleValueChanged);
            }
            if (RotateToggle != null)
            {
                RotateToggle.onValueChanged.RemoveListener(OnRotateToggleValueChanged);
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.onValueChanged.RemoveListener(OnScaleToggleValueChanged);
            }
            if (RectToggle != null)
            {
                RectToggle.onValueChanged.RemoveListener(OnRectToggleValueChanged);
            }
            if (PivotRotationToggle != null)
            {
                PivotRotationToggle.onValueChanged.RemoveListener(OnPivotRotationToggleValueChanged);
            }
            if (PivotModeToggle != null)
            {
                PivotModeToggle.onValueChanged.RemoveListener(OnPivotModeToggleValueChanged);
            }
            if (WireframeToggle != null)
            {
                WireframeToggle.onValueChanged.RemoveListener(OnWireframeToggleValueChanged);
            }
            if (UnitSnappingToggle != null)
            {
                UnitSnappingToggle.onValueChanged.RemoveListener(OnUnitSnappingToggleValueChanged);
            }
            if (VertexSnappingToggle != null)
            {
                VertexSnappingToggle.onValueChanged.RemoveListener(OnBoundingBoxSnappingToggleValueChanged);
            }
            if (AutoFocusToggle != null)
            {
                AutoFocusToggle.onValueChanged.RemoveListener(OnAutoFocusToggleValueChanged);
            }
            if (PlayToggle != null)
            {
                PlayToggle.onValueChanged.RemoveListener(OnPlayToggleValueChanged);
            }

            if (BtnSave != null)
            {
                BtnSave.onClick.RemoveListener(OnSaveClick);
            }

            if (BtnSaveAs != null)
            {
                BtnSaveAs.onClick.RemoveListener(OnSaveAsClick);
            }

            if (BtnNew != null)
            {
                BtnNew.onClick.RemoveListener(OnNewClick);
            }
            if (BtnUndo != null)
            {
                BtnUndo.onClick.RemoveListener(OnUndoClick);
            }
            if (BtnRedo != null)
            {
                BtnRedo.onClick.RemoveListener(OnRedoClick);
            }
        }

        protected virtual void OnViewToggleValueChanged(bool value)
        {
            if(!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                Editor.Tools.Current = RuntimeTool.View;
                m_handleValueChange = false;
                RotateToggle.isOn = false;
                ScaleToggle.isOn = false;
                MoveToggle.isOn = false;
                RectToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if (Editor.Tools.Current == RuntimeTool.View)
                {
                    ViewToggle.isOn = true;
                }
            }
        }
        private void OnMoveToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                Editor.Tools.Current = RuntimeTool.Move;
                m_handleValueChange = false;
                RotateToggle.isOn = false;
                ScaleToggle.isOn = false;
                ViewToggle.isOn = false;
                RectToggle.isOn = false;
                m_handleValueChange = true;

            }
            else
            {
                if (Editor.Tools.Current == RuntimeTool.Move)
                {
                    MoveToggle.isOn = true;
                }
            }
        }

        protected virtual void OnRotateToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                Editor.Tools.Current = RuntimeTool.Rotate;
                m_handleValueChange = false;
                ViewToggle.isOn = false;
                ScaleToggle.isOn = false;
                MoveToggle.isOn = false;
                RectToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if (Editor.Tools.Current == RuntimeTool.Rotate)
                {
                    RotateToggle.isOn = true;
                }
            }

        }

        protected virtual void OnScaleToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                Editor.Tools.Current = RuntimeTool.Scale;
                m_handleValueChange = false;
                ViewToggle.isOn = false;
                RotateToggle.isOn = false;
                MoveToggle.isOn = false;
                RectToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if(Editor.Tools.Current == RuntimeTool.Scale)
                {
                    ScaleToggle.isOn = true;
                }
            }
        }


        protected virtual void OnRectToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                Editor.Tools.Current = RuntimeTool.Rect;
                m_handleValueChange = false;
                ViewToggle.isOn = false;
                RotateToggle.isOn = false;
                MoveToggle.isOn = false;
                ScaleToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if (Editor.Tools.Current == RuntimeTool.Rect)
                {
                    RectToggle.isOn = true;
                }
            }
        }


        protected virtual void OnPivotRotationToggleValueChanged(bool value)
        {
            if(value)
            {
                Editor.Tools.PivotRotation = RuntimePivotRotation.Global;
            }
            else
            {
                Editor.Tools.PivotRotation = RuntimePivotRotation.Local;
            }
        }


        protected virtual void OnPivotModeToggleValueChanged(bool value)
        {
            if (value)
            {
                Editor.Tools.PivotMode = RuntimePivotMode.Center;
            }
            else
            {
                Editor.Tools.PivotMode = RuntimePivotMode.Pivot;
            }
        }

        protected virtual void OnWireframeToggleValueChanged(bool value)
        {
            //NOT IMPLEMENTED
        }

        protected virtual void OnAutoFocusToggleValueChanged(bool value)
        {
            Editor.Tools.AutoFocus = value;
        }

        protected virtual void OnUnitSnappingToggleValueChanged(bool value)
        {
            Editor.Tools.UnitSnapping = value;
        }

        protected virtual void OnBoundingBoxSnappingToggleValueChanged(bool value)
        {
            Editor.Tools.IsSnapping = value;
        }

        protected virtual void OnPlayToggleValueChanged(bool value)
        {
            Editor.IsPlaying = value;
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            if(PlayToggle != null)
            {
                PlayToggle.isOn = Editor.IsPlaying;
            }

            UpdateLoadSaveButtonsState();
        }

        protected virtual void OnPivotRotationChanged()
        {
            if(PivotRotationToggle != null)
            {
                if (Editor.Tools.PivotRotation == RuntimePivotRotation.Global)
                {
                    PivotRotationToggle.isOn = true;
                }
                else
                {
                    PivotRotationToggle.isOn = false;
                }
            }
        }

        protected virtual void OnPivotModeChanged()
        {
            if (PivotModeToggle != null)
            {
                if (Editor.Tools.PivotMode == RuntimePivotMode.Center)
                {
                    PivotModeToggle.isOn = true;
                }
                else
                {
                    PivotModeToggle.isOn = false;
                }
            }
        }

        protected virtual void OnRuntimeToolChanged()
        {
            if(!m_handleValueChange)
            {
                return;
            }
            if (ViewToggle != null)
            {
                ViewToggle.isOn = Editor.Tools.Current == RuntimeTool.View;
            }
            if (MoveToggle != null)
            {
                MoveToggle.isOn = Editor.Tools.Current == RuntimeTool.Move;
            }
            if (RotateToggle != null)
            {
                RotateToggle.isOn = Editor.Tools.Current == RuntimeTool.Rotate;
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.isOn = Editor.Tools.Current == RuntimeTool.Scale;
            }
            if(RectToggle != null)
            {
                RectToggle.isOn = Editor.Tools.Current == RuntimeTool.Rect;
            }
        }

        protected virtual void OnAutoFocusChanged()
        {
            if(AutoFocusToggle != null)
            {
                AutoFocusToggle.isOn = Editor.Tools.AutoFocus;
            }
        }

        protected virtual void OnUnitSnappingChanged()
        {
            if(UnitSnappingToggle != null)
            {
                UnitSnappingToggle.isOn = Editor.Tools.UnitSnapping;
            }
        }

        protected virtual void OnBoundingBoxSnappingChanged()
        {
            if(VertexSnappingToggle != null)
            {
                VertexSnappingToggle.isOn = Editor.Tools.IsSnapping;
            }
        }

        protected virtual void UpdateLoadSaveButtonsState()
        {
            if(BtnSave != null)
            {
                BtnSave.interactable = m_project != null && (Editor.Undo.CanRedo || Editor.Undo.CanUndo) && !Editor.IsPlaying;
            }

            if (BtnSaveAs != null)
            {
                BtnSaveAs.interactable = m_project != null && (Editor.Undo.CanRedo || Editor.Undo.CanUndo) && !Editor.IsPlaying;
            }

            if (BtnNew != null)
            {
                BtnNew.interactable = m_project != null && !Editor.IsPlaying; 
            }
        }

        protected virtual void UpdateUndoRedoButtonsState()
        {
            if (BtnUndo != null)
            {
                BtnUndo.interactable = Editor.Undo.CanUndo;
            }

            if (BtnRedo != null)
            {
                BtnRedo.interactable = Editor.Undo.CanRedo;
            }
        }

        protected virtual void OnSaveClick()
        {
            if (Editor.IsPlaying)
            {
                m_wm.MessageBox("Save Scene", "Scene can not be saved in playmode");
                return;
            }

            m_editor.SaveScene();
        }

        protected virtual void OnSaveAsClick()
        {
            m_editor.SaveSceneAs();
        }

        protected virtual void OnNewClick()
        {
            if (Editor.IsPlaying)
            {
                m_wm.MessageBox("Create Scene", "Scene can not be create in playmode");
                return;
            }

            if (m_project == null)
            {
                Debug.LogError("Project Manager is null");
                return;
            }


            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.NewScene(true);
        }

        protected virtual void OnUndoClick()
        {
            Editor.Undo.Undo();
        }

        protected virtual void OnRedoClick()
        {
            Editor.Undo.Redo();
        }

        protected virtual void OnStateChanged()
        {
            UpdateUndoRedoButtonsState();
            UpdateLoadSaveButtonsState();
        }

        protected virtual void OnRedoCompleted()
        {
            UpdateUndoRedoButtonsState();
            UpdateLoadSaveButtonsState();
        }

        protected virtual void OnUndoCompleted()
        {
            UpdateUndoRedoButtonsState();
            UpdateLoadSaveButtonsState();
        }

        protected virtual void OnSceneSaved()
        {
            UpdateUndoRedoButtonsState();
            UpdateLoadSaveButtonsState();
        }

        protected virtual void OnSceneLoaded()
        {
            UpdateUndoRedoButtonsState();
            UpdateLoadSaveButtonsState();
        }
    }
}
