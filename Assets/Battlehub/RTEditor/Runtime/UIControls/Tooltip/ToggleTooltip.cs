using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.TooltipControl
{
    [RequireComponent(typeof(Tooltip))]
    public class ToggleTooltip : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_toggle;

        [TextArea]
        [SerializeField]
        private string m_onText = "On Tooltip";

        [TextArea]
        [SerializeField]
        private string m_offText = "Off Tooltip";

        private Tooltip m_tooltip;
        
        private void Start()
        {
            m_tooltip = GetComponent<Tooltip>();

            if (m_toggle == null)
            {
                m_toggle = GetComponent<Toggle>();
                OnValueChanged(m_toggle.isOn);
            }

            if (m_toggle != null)
            {
                m_toggle.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnValueChanged(bool value)
        {
            if (value)
            {
                m_tooltip.Text = m_onText;
                m_tooltip.Refresh();
            }
            else
            {
                m_tooltip.Text = m_offText;
                m_tooltip.Refresh();
            }
        }

    }
}

