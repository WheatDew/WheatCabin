using Battlehub.RTCommon;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [DefaultExecutionOrder(-1)]
    [Binding]
    public class ToolsViewModel : ViewModel
    {
        private bool m_handleValueChange;

        [Binding]
        public virtual bool IsView
        {
            get { return Editor.Tools.Current == RuntimeTool.View; }
            set
            {
                if (value)
                {
                    Editor.Tools.Current = RuntimeTool.View;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsMove
        {
            get { return Editor.Tools.Current == RuntimeTool.Move; }
            set
            {
                if (value)
                {
                    Editor.Tools.Current = RuntimeTool.Move;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsRotate
        {
            get { return Editor.Tools.Current == RuntimeTool.Rotate; }
            set
            {
                if (value)
                {
                    Editor.Tools.Current = RuntimeTool.Rotate;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsScale
        {
            get { return Editor.Tools.Current == RuntimeTool.Scale; }
            set
            {
                if(value)
                {
                    Editor.Tools.Current = RuntimeTool.Scale;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsRect
        {
            get { return Editor.Tools.Current == RuntimeTool.Rect; }
            set
            {
                if(value)
                {
                    Editor.Tools.Current = RuntimeTool.Rect;
                }

                UpdateTogglesState();
            }
        }

        [Binding]
        public virtual bool IsPivotRotation
        {
            get { return Editor.Tools.PivotRotation == RuntimePivotRotation.Global; }
            set 
            {
                Editor.Tools.PivotRotation = value ? RuntimePivotRotation.Global : RuntimePivotRotation.Local;
                RaisePropertyChanged(nameof(IsPivotRotation));
            }
        }

        [Binding]
        public virtual bool IsPivotMode
        {
            get { return Editor.Tools.PivotMode == RuntimePivotMode.Center; }
            set 
            { 
                Editor.Tools.PivotMode = value ? RuntimePivotMode.Center : RuntimePivotMode.Pivot;
                RaisePropertyChanged(nameof(IsPivotMode));
            }
        }

        [Binding]
        public virtual bool IsPlay
        {
            get { return Editor.IsPlaying; }
            set 
            {
                Editor.IsPlaying = value;
                RaisePropertyChanged(nameof(IsPlay));
            }
        }

        
        [Binding]
        public virtual bool IsUndoInteractable
        {
            get { return Editor.Undo.CanUndo; }
        }

        [Binding]
        public virtual bool IsRedoInteractable
        {
            get { return Editor.Undo.CanRedo; }
        }


        [Binding]
        public virtual void OnUndo()
        {
            Editor.Undo.Undo();
        }

        [Binding]
        public virtual void OnRedo()
        {
            Editor.Undo.Redo();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Editor.Tools.ToolChanged += OnRuntimeToolChanged;
            Editor.Tools.PivotRotationChanged += OnPivotRotationChanged;
            Editor.Tools.PivotModeChanged += OnPivotModeChanged;

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
            Editor.Undo.UndoCompleted += OnUndoCompleted;
            Editor.Undo.RedoCompleted += OnRedoCompleted;
            Editor.Undo.StateChanged += OnStateChanged;

            Editor.SceneLoaded += OnSceneLoaded;
            Editor.SceneSaved += OnSceneSaved;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Editor != null)
            {
                Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
                Editor.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                Editor.Tools.PivotModeChanged -= OnPivotModeChanged;
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                Editor.Undo.UndoCompleted -= OnUndoCompleted;
                Editor.Undo.RedoCompleted -= OnRedoCompleted;
                Editor.Undo.StateChanged -= OnStateChanged;
                Editor.SceneLoaded -= OnSceneLoaded;
                Editor.SceneSaved -= OnSceneSaved;
            }
        }

        protected void UpdateTogglesState()
        {
            RaisePropertyChanged(nameof(IsView));
            RaisePropertyChanged(nameof(IsMove));
            RaisePropertyChanged(nameof(IsRotate));
            RaisePropertyChanged(nameof(IsScale));
            RaisePropertyChanged(nameof(IsRect));
        }

        protected virtual void UpdateUndoRedoButtonsState()
        {
            RaisePropertyChanged(nameof(IsUndoInteractable));
            RaisePropertyChanged(nameof(IsRedoInteractable));
        }

        protected virtual void OnPivotRotationChanged()
        {
            RaisePropertyChanged(nameof(IsPivotRotation));
        }

        protected virtual void OnPivotModeChanged()
        {
            RaisePropertyChanged(nameof(IsPivotMode));
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            RaisePropertyChanged(nameof(IsPlay));   
        }

        protected virtual void OnRuntimeToolChanged()
        {
            UpdateTogglesState();
        }

        protected virtual void OnStateChanged()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnRedoCompleted()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnUndoCompleted()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnSceneSaved()
        {
            UpdateUndoRedoButtonsState();
        }

        protected virtual void OnSceneLoaded()
        {
            UpdateUndoRedoButtonsState();
        }

    }
}
