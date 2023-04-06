using Battlehub.UIControls;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class ConsoleView : View
    {
        [SerializeField]
        private TMP_InputField m_stackTrace = null;

        [SerializeField]
        private VirtualizingScrollRect m_scrollRect = null;

        private bool m_autoScroll;
        public bool CanAutoScroll
        {
            get { return m_autoScroll; }
            set
            {
                if (value)
                {
                    if (m_scrollRect.Index + m_scrollRect.VisibleItemsCount == m_scrollRect.ItemsCount)
                    {
                        m_autoScroll = true;
                      
                    }
                }
                else
                {
                    if(m_autoScroll)
                    {
                        m_scrollRect.verticalNormalizedPosition = 0;
                        m_autoScroll = false;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_stackTrace != null)
            {
                m_stackTrace.scrollSensitivity = 0;
            }
        }
    }
}
