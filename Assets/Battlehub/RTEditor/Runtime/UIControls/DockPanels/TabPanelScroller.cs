using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class TabPanelScroller : MonoBehaviour
    {
        [SerializeField]
        private RepeatButton m_left = null;

        [SerializeField]
        private RepeatButton m_right = null;

        [SerializeField]
        private RectTransform m_viewport;

        [SerializeField]
        private HorizontalLayoutGroup m_content = null;

        [SerializeField]
        private float m_sensitivity = 500;

        private float ViewportLeft
        {
            get { return m_viewport.localPosition.x; }
        }

        private float ViewportRight
        {
            get { return m_viewport.localPosition.x + m_viewport.rect.width; }
        }

        private float ContentLeft
        {
            get { return m_content.transform.localPosition.x; }
            set
            {
                Vector3 pos = m_content.transform.localPosition;
                pos.x = value;
                m_content.transform.localPosition = pos;
            }
        }

        private float ContentRight
        {
            get { return m_content.transform.localPosition.x + ContentSize; }
            set
            {
                Vector3 pos = m_content.transform.localPosition;
                pos.x = value - ContentSize;
                m_content.transform.localPosition = pos;
            }
        }

        private float m_contentSize;
        private float ContentSize
        {
            get { return m_contentSize; }
        }

        private Region m_region;
        private TransformChildrenChangeListener m_transformChildrenChangeListener;
        private bool m_updateButtonsState;
        private bool m_isStarted;
        private bool m_isDraggingRegion;

        private void Awake()
        {
            m_region = GetComponentInParent<Region>();

            if(m_region != null)
            {

                m_region.Root.TabBeginDrag += OnTabBeginDrag;
                m_region.Root.TabEndDrag += OnTabEndDrag;
            }
            
            UpdateContentSize();
            m_viewport = GetComponent<RectTransform>();
            m_transformChildrenChangeListener = m_content.gameObject.AddComponent<TransformChildrenChangeListener>();    
        }


        private IEnumerator Start()
        {
            yield return null;

            m_updateButtonsState = true;
            m_isStarted = true;
            m_transformChildrenChangeListener.TransformChildrenChanged += OnTabPanelChildrenChanged;

            UpdateContentSize();
            UpdateButtonsState();
        }

        private void OnDestroy()
        {
            if(m_region != null && m_region.Root != null)
            {
                m_region.Root.TabBeginDrag -= OnTabBeginDrag;
                m_region.Root.TabEndDrag -= OnTabEndDrag;
            }

            if (m_transformChildrenChangeListener != null)
            {
                m_transformChildrenChangeListener.TransformChildrenChanged -= OnTabPanelChildrenChanged;
            }
        }

        private void OnTabBeginDrag(Region region)
        {
            m_isDraggingRegion = true;
            m_left.gameObject.SetActive(false);
            m_right.gameObject.SetActive(false);
        }

        private void OnTabEndDrag(Region region)
        {
            m_isDraggingRegion = false;
            m_updateButtonsState = true;
        }

        private void OnRectTransformDimensionsChange()
        {
            if(m_isStarted)
            {
                m_updateButtonsState = true;
            }
        }

        private void OnTabPanelChildrenChanged()
        {
            UpdateContentSize();
            UpdateButtonsState();
        }

        private void UpdateContentSize()
        {
            m_contentSize = 0;
            foreach (Transform child in m_content.transform)
            {
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                m_contentSize += layoutElement.minWidth;
            }
        }

        private void UpdateButtonsState()
        {
            if (ContentRight < ViewportRight && ContentLeft < ViewportLeft)
            {
                ContentRight = ViewportRight;
                if (ContentLeft > ViewportLeft)
                {
                    ContentLeft = ViewportLeft;
                }
            }

            if (m_viewport.rect.width < ContentSize && !m_isDraggingRegion)
            {
                if (ContentLeft < ViewportLeft)
                {
                    m_left.gameObject.SetActive(true);
                }
                else
                {
                    DisableLeft();
                }

                if (ContentRight > ViewportRight)
                {
                    m_right.gameObject.SetActive(true);
                }
                else
                {
                    DisableRight();
                }
            }
            else
            {
                DisableRight();
                DisableLeft();
            }
        }

        private void DisableRight()
        {
            if (m_right.IsPressed)
            {
                m_right.OnPointerUp(null);
            }
            m_right.gameObject.SetActive(false);
        }

        private void DisableLeft()
        {
            if (m_left.IsPressed)
            {
                m_left.OnPointerUp(null);
            }
            m_left.gameObject.SetActive(false);
        }

        private void Update()
        {
            if(m_updateButtonsState)
            {
                OnTabPanelChildrenChanged();
                m_updateButtonsState = false;
            }

            if (m_right.IsPressed)
            {
                ContentLeft -= Time.deltaTime * m_sensitivity;
                if (ContentLeft < ViewportLeft)
                {
                    m_left.gameObject.SetActive(true);
                }

                if (ContentRight <= ViewportRight)
                {
                    ContentRight = ViewportRight;
                    DisableRight();
                }
            }
            else if (m_left.IsPressed)
            {
                ContentLeft += Time.deltaTime * m_sensitivity;
                if (ContentRight > ViewportRight)
                {
                    m_right.gameObject.SetActive(true);
                }

                if (ContentLeft >= ViewportLeft)
                {
                    ContentLeft = ViewportLeft;
                    DisableLeft();
                }
            }
        }

        public void ScrollToRight()
        {
            if (m_viewport.rect.width < ContentSize)
            {
                ContentRight = ViewportRight;
                DisableRight();

                OnTabPanelChildrenChanged();
            }
        }

        public void ScrollToLeft()
        {
            ContentLeft = ViewportLeft;
            DisableLeft();

            OnTabPanelChildrenChanged();
        }
    }
}

