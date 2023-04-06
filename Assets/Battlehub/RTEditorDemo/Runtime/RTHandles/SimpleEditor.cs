using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles.Demo
{
    public class SimpleEditor : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_viewToggle = null;

        [SerializeField]
        private Toggle m_positionToggle = null;

        [SerializeField]
        private Toggle m_rotationToggle = null;

        [SerializeField]
        private Toggle m_scaleToggle = null;

        [SerializeField]
        private Toggle m_rectToggle = null;

        [SerializeField]
        private Toggle m_pivotModeToggle = null;

        [SerializeField]
        private Toggle m_pivotRotationToggle = null;

        [SerializeField]
        private Button m_undoButton = null;

        [SerializeField]
        private Button m_redoButton = null;

        private IRTE m_editor;
        protected IRTE Editor
        {
            get { return m_editor; }
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            m_editor = IOC.Resolve<IRTE>();

            m_editor.Tools.ToolChanged += OnToolChanged;
            m_editor.Tools.PivotModeChanged += OnPivotModeChanged;
            m_editor.Tools.PivotRotationChanged += OnPivotRotationChanged;

            m_editor.Undo.UndoCompleted += UpdateUndoRedoButtons;
            m_editor.Undo.RedoCompleted += UpdateUndoRedoButtons;
            m_editor.Undo.StateChanged += UpdateUndoRedoButtons;

            SubscribeUIEvents();

            UpdateUndoRedoButtons();
        }

        protected virtual void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnToolChanged;
                m_editor.Tools.PivotModeChanged -= OnPivotModeChanged;
                m_editor.Tools.PivotRotationChanged -= OnPivotRotationChanged;

                m_editor.Undo.UndoCompleted -= UpdateUndoRedoButtons;
                m_editor.Undo.RedoCompleted -= UpdateUndoRedoButtons;
                m_editor.Undo.StateChanged -= UpdateUndoRedoButtons;
            }

            UnsubscribeUIEvents();
        }

        protected virtual void SubscribeUIEvents()
        {
            if (m_viewToggle) m_viewToggle.onValueChanged.AddListener(OnViewToggle);
            if (m_positionToggle) m_positionToggle.onValueChanged.AddListener(OnPositionToggle);
            if (m_rotationToggle) m_rotationToggle.onValueChanged.AddListener(OnRotationToggle);
            if (m_scaleToggle) m_scaleToggle.onValueChanged.AddListener(OnScaleToogle);
            if (m_rectToggle) m_rectToggle.onValueChanged.AddListener(OnRectToggle);

            if (m_pivotModeToggle) m_pivotModeToggle.onValueChanged.AddListener(OnPivotModeToggle);
            if (m_pivotRotationToggle) m_pivotRotationToggle.onValueChanged.AddListener(OnPivotRotationToggle);

            if (m_undoButton) m_undoButton.onClick.AddListener(OnUndoClick);
            if (m_redoButton) m_redoButton.onClick.AddListener(OnRedoClick);
        }

        protected virtual void UnsubscribeUIEvents()
        {
            if (m_viewToggle) m_viewToggle.onValueChanged.RemoveListener(OnViewToggle);
            if (m_positionToggle) m_positionToggle.onValueChanged.RemoveListener(OnPositionToggle);
            if (m_rotationToggle) m_rotationToggle.onValueChanged.RemoveListener(OnRotationToggle);
            if (m_scaleToggle) m_scaleToggle.onValueChanged.RemoveListener(OnScaleToogle);
            if (m_rectToggle) m_rectToggle.onValueChanged.RemoveListener(OnRectToggle);

            if (m_pivotModeToggle) m_pivotModeToggle.onValueChanged.AddListener(OnPivotModeToggle);
            if (m_pivotRotationToggle) m_pivotRotationToggle.onValueChanged.RemoveListener(OnPivotRotationToggle);

            if (m_undoButton) m_undoButton.onClick.RemoveListener(OnUndoClick);
            if (m_redoButton) m_redoButton.onClick.RemoveListener(OnRedoClick);
        }


        private void OnToolChanged()
        {
            UnsubscribeUIEvents();

            RuntimeTool tool = m_editor.Tools.Current;
            switch(tool)
            {
                case RuntimeTool.View:
                    if(m_viewToggle != null) m_viewToggle.isOn = true;
                    break;
                case RuntimeTool.Move:
                    if (m_positionToggle != null) m_positionToggle.isOn = true;
                    break;
                case RuntimeTool.Rotate:
                    if (m_rotationToggle != null) m_rotationToggle.isOn = true;
                    break;
                case RuntimeTool.Scale:
                    if (m_scaleToggle != null) m_scaleToggle.isOn = true;
                    break;
                case RuntimeTool.Rect:
                    if (m_rectToggle != null) m_rectToggle.isOn = true;
                    break;
                case RuntimeTool.None:
                    if (m_viewToggle != null) m_viewToggle.isOn = false;
                    if (m_positionToggle != null) m_positionToggle.isOn = false;
                    if (m_rotationToggle != null) m_rotationToggle.isOn = false;
                    if (m_scaleToggle != null) m_scaleToggle.isOn = false;
                    if (m_rectToggle != null) m_rectToggle.isOn = false;
                    break;
            }

            SubscribeUIEvents();
        }

        private void OnPivotModeChanged()
        {
            UnsubscribeUIEvents();

            m_pivotModeToggle.isOn = m_editor.Tools.PivotMode == RuntimePivotMode.Center;

            Text text = m_pivotModeToggle.GetComponent<Text>();
            if (text != null)
            {
                text.text = m_editor.Tools.PivotMode.ToString() + " (Z)";
            }

            SubscribeUIEvents();
        }

        private void OnPivotRotationChanged()
        {
            UnsubscribeUIEvents();

            m_pivotRotationToggle.isOn = m_editor.Tools.PivotRotation == RuntimePivotRotation.Global;

            Text text = m_pivotRotationToggle.GetComponent<Text>();
            if (text != null)
            {
                text.text = m_editor.Tools.PivotRotation.ToString() + " (X)";
            }

            SubscribeUIEvents();
        }

        private void UpdateUndoRedoButtons()
        {
            if(m_undoButton) m_undoButton.interactable = m_editor.Undo.CanUndo;
            if(m_redoButton) m_redoButton.interactable = m_editor.Undo.CanRedo;
        }

        private void OnViewToggle(bool value)
        {
            m_editor.Tools.Current = RuntimeTool.View;
        }

        private void OnPositionToggle(bool value)
        {
            m_editor.Tools.Current = RuntimeTool.Move;
        }

        private void OnRotationToggle(bool value)
        {
            m_editor.Tools.Current = RuntimeTool.Rotate;
        }

        private void OnScaleToogle(bool value)
        {
            m_editor.Tools.Current = RuntimeTool.Scale;
        }

        private void OnRectToggle(bool value)
        {
            m_editor.Tools.Current = RuntimeTool.Rect;
        }

        private void OnPivotModeToggle(bool value)
        {
            m_editor.Tools.PivotMode = value ? RuntimePivotMode.Center : RuntimePivotMode.Pivot;
        }

        private void OnPivotRotationToggle(bool value)
        {
            m_editor.Tools.PivotRotation = value ? RuntimePivotRotation.Global : RuntimePivotRotation.Local;
        }

        private void OnUndoClick()
        {
            m_editor.Undo.Undo();
        }

        private void OnRedoClick()
        {
            m_editor.Undo.Redo();
        }
    }
}
