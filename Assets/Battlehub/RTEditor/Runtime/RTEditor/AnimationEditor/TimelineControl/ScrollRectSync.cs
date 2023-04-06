using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ScrollRectSync : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect m_scroll1 = null;
        [SerializeField]
        private ScrollRect m_scroll2 = null;
        [SerializeField]
        private bool m_vertical = true;
        [SerializeField]
        private bool m_horizontal = false;
        
        private void Awake()
        {
            m_scroll1.onValueChanged.AddListener(OnScroll1ValueChanged);
            m_scroll2.onValueChanged.AddListener(OnScroll2ValueChanged);
        }

        private void OnDestroy()
        {
            if(m_scroll1 != null)
            {
                m_scroll1.onValueChanged.RemoveListener(OnScroll1ValueChanged);
            }

            if (m_scroll2 != null)
            {
                m_scroll2.onValueChanged.RemoveListener(OnScroll2ValueChanged);
            }
        }

        private void OnScroll1ValueChanged(Vector2 value)
        {
            if(m_vertical)
            {
                m_scroll2.verticalNormalizedPosition = value.y;
            }

            if(m_horizontal)
            {
                m_scroll2.horizontalNormalizedPosition = value.x;
            }
        }

        private void OnScroll2ValueChanged(Vector2 value)
        {
            if (m_vertical)
            {
                m_scroll1.verticalNormalizedPosition = value.y;
            }

            if (m_horizontal)
            {
                m_scroll1.horizontalNormalizedPosition = value.x;
            }
        }
        
    }

}

