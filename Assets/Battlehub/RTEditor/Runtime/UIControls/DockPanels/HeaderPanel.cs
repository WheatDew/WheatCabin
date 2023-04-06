using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class HeaderPanel : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement m_layoutElement = null;

        [SerializeField]
        private Transform m_tabPanel = null;

        private TransformChildrenChangeListener m_listener;

        [SerializeField]
        private bool m_isVisible = true;
        public bool IsVisible
        {
            get { return m_isVisible; }
            set
            {
                m_isVisible = value;
                RecalculateHeight();
            }
        }


        private void Start()
        {
            if (m_layoutElement == null)
            {
                m_layoutElement = GetComponent<LayoutElement>();
            }

            RecalculateHeight();

            m_listener = m_tabPanel.gameObject.AddComponent<TransformChildrenChangeListener>();
            m_listener.TransformChildrenChanged += OnChildrenChanged;
        }

        private void OnDestroy()
        {
            if(m_listener != null)
            {
                m_listener.TransformChildrenChanged -= OnChildrenChanged;
                Destroy(m_listener);
            }
        }

        private void OnChildrenChanged()
        {
            RecalculateHeight();
        }

        private void RecalculateHeight()
        {
            float height = 0;
            Transform panel = transform;
            if (m_tabPanel.childCount > 0)
            {
                panel = m_tabPanel;
            }
           
            foreach (RectTransform child in panel)
            {
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    height = Mathf.Max(height, Mathf.Max(layoutElement.preferredHeight, layoutElement.minHeight));
                }
            }

            m_layoutElement.preferredHeight = m_isVisible ? height : 0;
        }

    }
}


