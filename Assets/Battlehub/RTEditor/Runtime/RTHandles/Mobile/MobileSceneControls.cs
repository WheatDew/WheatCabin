using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
    public class MobileSceneControls : MonoBehaviour
    {
        public enum Mode
        {
            Orbit,
            View,
            Pan,
            BoxSelection,
        }

        public event Action Focus;
        public event Action ResetPosition;
        public event Action<Mode> ModeChanged;

        [SerializeField]
        private Button m_boxSelectionButton = null;

        [SerializeField]
        private Button m_cancelButton = null;

        [SerializeField]
        private Button m_focusButton = null;

        [SerializeField]
        private Button m_resetPositionButton = null;

        [SerializeField]
        private Button m_viewModeButton = null;

        [SerializeField]
        private Button m_orbitModeButton = null;

        [SerializeField]
        private Button m_panModeButton = null;

        private Mode m_prevMode = Mode.Orbit;
        private Mode m_currentMode = Mode.Orbit;
        public Mode CurrentMode
        {
            get { return m_currentMode; }
            private set
            {
                if(m_currentMode != value)
                {
                    m_currentMode = value;

                    UpdateButtonsVisibility();
                    ModeChanged?.Invoke(value);
                }
            }
        }

        private bool m_isOrthographicMode;
        public bool IsOrthographicMode
        {
            get { return m_isOrthographicMode; }
            set
            {
                if(m_isOrthographicMode != value)
                {
                    m_isOrthographicMode = value;
                    UpdateButtonsVisibility();
                }
            }
        }

        private IRTE m_editor;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            if (m_focusButton != null)
            {
                m_focusButton.interactable = m_editor.Selection.activeGameObject != null;
            }

            UnityEventHelper.AddListener(m_cancelButton, btn => btn.onClick, OnCancelClick);
            UnityEventHelper.AddListener(m_boxSelectionButton, btn => btn.onClick, OnBoxSelectionClick);
            UnityEventHelper.AddListener(m_focusButton, btn => btn.onClick, OnFocusClick);
            UnityEventHelper.AddListener(m_resetPositionButton, btn => btn.onClick, OnResetPositionClick);
            UnityEventHelper.AddListener(m_viewModeButton, btn => btn.onClick, OnViewModeClick);
            UnityEventHelper.AddListener(m_orbitModeButton, btn => btn.onClick, OnOrbitModeClick);
            UnityEventHelper.AddListener(m_panModeButton, btn => btn.onClick, OnPanModeClick);
        }

        private void OnDestroy()
        {
            m_editor.Selection.SelectionChanged -= OnSelectionChanged;

            UnityEventHelper.RemoveListener(m_cancelButton, btn => btn.onClick, OnCancelClick);
            UnityEventHelper.RemoveListener(m_boxSelectionButton, btn => btn.onClick, OnBoxSelectionClick);
            UnityEventHelper.RemoveListener(m_focusButton, btn => btn.onClick, OnFocusClick);
            UnityEventHelper.RemoveListener(m_resetPositionButton, btn => btn.onClick, OnResetPositionClick);
            UnityEventHelper.RemoveListener(m_viewModeButton, btn => btn.onClick, OnViewModeClick);
            UnityEventHelper.RemoveListener(m_orbitModeButton, btn => btn.onClick, OnOrbitModeClick);
            UnityEventHelper.RemoveListener(m_panModeButton, btn => btn.onClick, OnPanModeClick);
        }

        private void UpdateButtonsVisibility()
        {
            if(m_isOrthographicMode)
            {
                if (m_viewModeButton != null)
                {
                    m_viewModeButton.gameObject.SetActive(false);
                }

                if (m_orbitModeButton != null)
                {
                    m_orbitModeButton.gameObject.SetActive(false);
                }

                if (m_panModeButton != null)
                {
                    m_panModeButton.gameObject.SetActive(false);
                }

                if (m_resetPositionButton != null)
                {
                    m_resetPositionButton.gameObject.SetActive(false);
                }

                if (m_boxSelectionButton != null)
                {
                    m_boxSelectionButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection);
                }

                if (m_cancelButton != null)
                {
                    m_cancelButton.gameObject.SetActive(m_currentMode == Mode.BoxSelection);
                }

                if (m_focusButton != null)
                {
                    m_focusButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection);
                }
            }
            else
            {
                if (m_viewModeButton != null)
                {
                    m_viewModeButton.gameObject.SetActive(m_currentMode == Mode.View);
                }

                if (m_orbitModeButton != null)
                {
                    m_orbitModeButton.gameObject.SetActive(m_currentMode == Mode.Orbit);
                }

                if (m_panModeButton != null)
                {
                    m_panModeButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection && m_currentMode != Mode.Pan);
                }

                if (m_boxSelectionButton != null)
                {
                    m_boxSelectionButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection && m_currentMode != Mode.Pan);
                }

                if (m_cancelButton != null)
                {
                    m_cancelButton.gameObject.SetActive(m_currentMode == Mode.BoxSelection || m_currentMode == Mode.Pan);
                }

                if (m_focusButton != null)
                {
                    m_focusButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection && m_currentMode != Mode.Pan);
                }

                if (m_resetPositionButton != null)
                {
                    m_resetPositionButton.gameObject.SetActive(m_currentMode != Mode.BoxSelection && m_currentMode != Mode.Pan);
                }
            }
       
        }

        private void OnCancelClick()
        {
            Cancel();
        }

        private void OnBoxSelectionClick()
        {
            m_prevMode = CurrentMode;
            CurrentMode = Mode.BoxSelection;
        }

        private void OnFocusClick()
        {
            Focus?.Invoke();
        }

        private void OnResetPositionClick()
        {
            ResetPosition?.Invoke();
        }

        private void OnViewModeClick()
        {
            CurrentMode = Mode.Orbit;
        }

        private void OnOrbitModeClick()
        {
            CurrentMode = Mode.View;
        }

        private void OnPanModeClick()
        {
            m_prevMode = CurrentMode;
            CurrentMode = Mode.Pan;
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            if(m_focusButton != null)
            {
                m_focusButton.interactable = m_editor.Selection.activeGameObject != null;
            }
        }

        public void Cancel()
        {
            CurrentMode = m_prevMode;
        }
    }
}

