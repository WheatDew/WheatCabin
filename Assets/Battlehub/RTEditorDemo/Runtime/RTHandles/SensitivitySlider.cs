using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles.Demo
{
    public class SensitivitySlider : MonoBehaviour
    {
        [SerializeField]
        private MobileSceneInput m_mobileInput = null;

        [SerializeField]
        private Slider m_sensitivity = null;

        private float m_defaultMoveSensitivity;
        private float m_defaultZoomSensitivty;
        private float m_defaultRotateSensitivity;

        private void Awake()
        {
            UnityEventHelper.AddListener(m_sensitivity, slider => slider.onValueChanged, OnSensitivityChanged);

            m_defaultMoveSensitivity = m_mobileInput.MoveSensitivity;
            m_defaultRotateSensitivity = m_mobileInput.RotateSensitivity;
            m_defaultZoomSensitivty = m_mobileInput.ZoomSensitivity;
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_sensitivity, slider => slider.onValueChanged, OnSensitivityChanged);
        }

        private void OnSensitivityChanged(float value)
        {
            m_mobileInput.MoveSensitivity = m_defaultMoveSensitivity * value;
            m_mobileInput.RotateSensitivity = m_defaultRotateSensitivity * value;
            m_mobileInput.ZoomSensitivity = m_defaultZoomSensitivty * value;
        }

    }

}
