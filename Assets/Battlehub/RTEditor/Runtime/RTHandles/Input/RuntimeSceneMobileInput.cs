using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
    [Obsolete("Use MobileSceneInput")] //09.12.2020
    public class RuntimeSceneMobileInput : RuntimeSelectionInputBase
    {
        [SerializeField]
        private Button m_rotateButton = null;
        [SerializeField]
        private Button m_panButton = null;
        [SerializeField]
        private Button m_cancelButton = null;
        [SerializeField]
        private Button m_focusButton = null;

        [SerializeField]
        private float m_zoomSensitivity = 1;
        private float m_rotateSensitivity = 1;

        private bool m_allowRotate;
        private bool m_allowPan;

        private Vector2 m_prevPointerXY;
        private float m_prevDistanceBetweenPointers;

        protected RuntimeSceneComponent SceneComponent
        {
            get { return (RuntimeSceneComponent)m_component; }
        }

        protected override void Start()
        {
            base.Start();
            UpdateVisualState();

            if(m_rotateButton != null)
            {
                m_rotateButton.onClick.AddListener(OnRotateClick);
            }
            if(m_panButton != null)
            {
                m_panButton.onClick.AddListener(OnPanClick);
            }
            if(m_cancelButton != null)
            {
                m_cancelButton.onClick.AddListener(OnCancelClick);
            }
            if(m_focusButton != null)
            {
                m_focusButton.onClick.AddListener(OnFocus);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_rotateButton != null)
            {
                m_rotateButton.onClick.RemoveListener(OnRotateClick);
            }
            if (m_panButton != null)
            {
                m_panButton.onClick.RemoveListener(OnPanClick);
            }
            if (m_cancelButton != null)
            {
                m_cancelButton.onClick.RemoveListener(OnCancelClick);
            }
            if (m_focusButton != null)
            {
                m_focusButton.onClick.RemoveListener(OnFocus);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
                    
            if (m_component.Editor.Input.GetPointerDown(1))
            {
                m_component.Editor.Tools.IsViewing = true;
                m_prevDistanceBetweenPointers = (SceneComponent.Editor.Input.GetPointerXY(0) - m_component.Editor.Input.GetPointerXY(1)).magnitude;
            }
            else if(m_component.Editor.Input.GetPointer(1))
            {
                float distance = (SceneComponent.Editor.Input.GetPointerXY(0) - m_component.Editor.Input.GetPointerXY(1)).magnitude;
                SceneComponent.Orbit(0, 0, (distance - m_prevDistanceBetweenPointers) * m_zoomSensitivity / 100);
                m_prevDistanceBetweenPointers = distance;
            }
            else if(m_component.Editor.Input.GetPointerUp(1))
            {
                if(m_allowRotate)
                {
                    m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
                }
                else if(m_allowPan)
                {
                    m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
                    SceneComponent.BeginPan(m_prevPointerXY);
                }
                else
                {
                    m_component.Editor.Tools.IsViewing = false;
                }
            }
            else
            {
                if (m_allowRotate)
                {
                    if(m_component.Editor.Input.GetPointerDown(0))
                    {
                        m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
                    }
                    else if (m_component.Editor.Input.GetPointer(0))
                    {
                        Vector2 pointerXY = SceneComponent.Editor.Input.GetPointerXY(0);

                        float deltaX = (pointerXY.x - m_prevPointerXY.x) * m_rotateSensitivity / 10;
                        float deltaY = (pointerXY.y - m_prevPointerXY.y) * m_rotateSensitivity / 10;

                        m_prevPointerXY = pointerXY;

                        SceneComponent.Orbit(deltaX, deltaY, 0);
                    }
                }
                else if (m_allowPan)
                {
                    if (m_component.Editor.Input.GetPointerDown(0))
                    {
                        m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
                        SceneComponent.BeginPan(m_prevPointerXY);
                    }
                    else if (m_component.Editor.Input.GetPointer(0))
                    {
                        Vector2 pointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
                        SceneComponent.Pan(pointerXY);
                        m_prevPointerXY = pointerXY;
                    }
                }
            }
        }

        private void OnRotateClick()
        {
            m_allowRotate = true;
            m_allowPan = false;
            m_component.Editor.Tools.IsViewing = true;

            m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);

            UpdateVisualState();
        }

        private void OnPanClick()
        {
            m_allowPan = true;
            m_allowRotate = false;
            m_component.Editor.Tools.IsViewing = true;

            m_prevPointerXY = SceneComponent.Editor.Input.GetPointerXY(0);
            SceneComponent.BeginPan(m_prevPointerXY);

            UpdateVisualState();
        }

        private void OnCancelClick()
        {
            m_allowPan = false;
            m_allowRotate = false;
            m_component.Editor.Tools.IsViewing = false;

            UpdateVisualState();
        }

        private void OnFocus()
        {
            SceneComponent.Focus(FocusMode.Default);
        }

        private void UpdateVisualState()
        {
            if(m_allowRotate || m_allowPan)
            {
                m_cancelButton.gameObject.SetActive(true);
                m_rotateButton.gameObject.SetActive(false);
                m_panButton.gameObject.SetActive(false);
            }
            else
            {
                m_cancelButton.gameObject.SetActive(false);
                m_rotateButton.gameObject.SetActive(true);
                m_panButton.gameObject.SetActive(true);
            }
        }

    }
}


