using UnityEngine;

namespace Battlehub.RTEditor
{
   
    public class AnchorPreset : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_element = null;

        [SerializeField]
        private RectTransform m_template = null;

        [SerializeField]
        private RectTransform m_topLeftAnchor = null;

        [SerializeField]
        private RectTransform m_topRightAnchor = null;

        [SerializeField]
        private RectTransform m_bottomRightAnchor = null;

        [SerializeField]
        private RectTransform m_bottomLeftAnchor = null;

        [SerializeField]
        private RectTransform m_pivot = null;

        [SerializeField]
        private RectTransform m_horizontalStretchArrow = null;

        [SerializeField]
        private RectTransform m_verticalStreatchArrow = null;

        [SerializeField]
        private RectTransform m_horizontalAlignmentLine = null;

        [SerializeField]
        private RectTransform m_verticalAlignmentLine = null;

        public bool IsPivotVisible
        {
            get { return m_pivot.gameObject.activeSelf; }
            set { m_pivot.gameObject.SetActive(value); }
        }

        private bool m_isPositionVisble;
        public bool IsPositionVisible
        {
            get { return m_isPositionVisble; }
            set
            {
                if(m_isPositionVisble != value)
                {
                    m_isPositionVisble = value;
                    if (m_isPositionVisble)
                    {
                        Copy(m_element, m_template);
                    }
                    else
                    {
                        m_element.pivot = Vector2.one * 0.5f;
                        m_element.anchorMin = Vector2.one * 0.25f;
                        m_element.anchorMax = Vector2.one * 0.75f;
                        //TODO: replace hardcoded size
                        m_element.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 17);
                        m_element.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 17);

                        m_element.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        public enum HAlign
        {
            Custom,
            Left,
            Center,
            Right,
            Stretch
        }

        public enum VAlign
        {
            Custom,
            Top,
            Middle,
            Bottom,
            Stretch
        }

        public HAlign HorizontalAlignment
        {
            get
            {
                if (IsLeftAligned)
                {
                    return HAlign.Left;
                }
                else if(IsCenterAligned)
                {
                    return HAlign.Center;
                }
                else if(IsRightAligned)
                {
                    return HAlign.Right;
                }
                else if(IsHorizontallyStretched)
                {
                    return HAlign.Stretch;
                }
                return HAlign.Custom;
            }
        }

        public VAlign VerticalAlignment
        {
            get
            {

                if (IsTopAligned)
                {
                    return VAlign.Top;
                }
                else if (IsMiddleAligned)
                {
                    return VAlign.Middle;
                }
                else if (IsBottomAligned)
                {
                    return VAlign.Bottom;
                }
                else if(IsVerticallyStretched)
                {
                    return VAlign.Stretch;
                }

                return VAlign.Custom;
            }
        }

        public bool IsLeftAligned
        {
            get { return Approximately(m_template.anchorMin.x, 0) && Approximately(m_template.anchorMax.x, 0); }
        }

        public bool IsRightAligned
        {
            get { return Approximately(m_template.anchorMax.x, 1) && Approximately(m_template.anchorMin.x, 1); }
        }

        public bool IsCenterAligned
        {
            get { return Approximately(m_template.anchorMin.x, 0.5f) && Approximately(m_template.anchorMax.x, 0.5f); }
        }

        public bool IsHorizontallyStretched
        {
            get { return Approximately(m_template.anchorMin.x, 0) && Approximately(m_template.anchorMax.x, 1); }
        }

        public bool IsBottomAligned
        {
            get { return Approximately(m_template.anchorMin.y, 0) && Approximately(m_template.anchorMax.y, 0);  }
        }

        public bool IsTopAligned
        {
            get { return Approximately(m_template.anchorMax.y, 1) && Approximately(m_template.anchorMin.y, 1); }
        }

        public bool IsMiddleAligned
        {
            get { return Approximately(m_template.anchorMin.y, 0.5f) && Approximately(m_template.anchorMax.y, 0.5f); }
        }

        public bool IsVerticallyStretched
        {
            get { return Approximately(m_template.anchorMax.y, 1) && Approximately(m_template.anchorMin.y, 0); }
        }

        private void Start()
        {
            m_pivot.anchorMin = m_template.pivot;
            m_pivot.anchorMax = m_template.pivot;
            m_pivot.pivot = m_template.pivot;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            bool alignedToLeft = IsLeftAligned;
            bool alignedToRight = IsRightAligned;
            bool alignedToCenter = IsCenterAligned;
            bool stretchedHorizontally = IsHorizontallyStretched;

            bool alignedToBottom = IsBottomAligned;
            bool alignedToTop = IsTopAligned;
            bool alignedToMiddle = IsMiddleAligned;
            bool stretchedVertically = IsVerticallyStretched;

            m_horizontalStretchArrow.gameObject.SetActive(stretchedHorizontally);
            m_verticalStreatchArrow.gameObject.SetActive(stretchedVertically);

            m_horizontalAlignmentLine.gameObject.SetActive((alignedToLeft || alignedToCenter || alignedToRight) && !stretchedHorizontally);
            m_verticalAlignmentLine.gameObject.SetActive((alignedToBottom || alignedToMiddle || alignedToTop) && !stretchedVertically);

            if (alignedToLeft)
            {
                m_horizontalAlignmentLine.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 1);
            }
            else if (alignedToRight)
            {
                m_horizontalAlignmentLine.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 1);
            }
            else if (alignedToCenter)
            {
                m_horizontalAlignmentLine.offsetMin = SetX(m_horizontalAlignmentLine.offsetMin, -0.5f);
                m_horizontalAlignmentLine.offsetMax = SetX(m_horizontalAlignmentLine.offsetMax, 0.5f);
                m_horizontalAlignmentLine.anchorMin = SetX(m_horizontalAlignmentLine.anchorMin, 0.5f);
                m_horizontalAlignmentLine.anchorMax = SetX(m_horizontalAlignmentLine.anchorMax, 0.5f);
                m_horizontalAlignmentLine.anchoredPosition = Vector2.zero;   
            }

            if (alignedToTop)
            {
                m_verticalAlignmentLine.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);
            }
            else if (alignedToBottom)
            {
                m_verticalAlignmentLine.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 1);
            }
            else if (alignedToMiddle)
            {
                m_verticalAlignmentLine.offsetMin = SetY(m_verticalAlignmentLine.offsetMin, -0.5f);
                m_verticalAlignmentLine.offsetMax = SetY(m_verticalAlignmentLine.offsetMax, 0.5f);
                m_verticalAlignmentLine.anchorMin = SetY(m_verticalAlignmentLine.anchorMin, 0.5f);
                m_verticalAlignmentLine.anchorMax = SetY(m_verticalAlignmentLine.anchorMax, 0.5f);
                m_verticalAlignmentLine.anchoredPosition = Vector2.zero;
            }

            bool isCustom = HorizontalAlignment == HAlign.Custom || VerticalAlignment == VAlign.Custom;
            m_topLeftAnchor.gameObject.SetActive(!isCustom);
            m_topRightAnchor.gameObject.SetActive(!isCustom);
            m_bottomRightAnchor.gameObject.SetActive(!isCustom);
            m_bottomLeftAnchor.gameObject.SetActive(!isCustom);

            m_topLeftAnchor.pivot = m_topLeftAnchor.anchorMax = m_topLeftAnchor.anchorMin = new Vector2(m_template.anchorMin.x, m_template.anchorMax.y);
            m_topRightAnchor.pivot = m_topRightAnchor.anchorMax = m_topRightAnchor.anchorMin = new Vector2(m_template.anchorMax.x, m_template.anchorMax.y);
            m_bottomRightAnchor.pivot = m_bottomRightAnchor.anchorMax = m_bottomRightAnchor.anchorMin = new Vector2(m_template.anchorMax.x, m_template.anchorMin.y);
            m_bottomLeftAnchor.pivot = m_bottomLeftAnchor.anchorMax = m_bottomLeftAnchor.anchorMin = new Vector2(m_template.anchorMin.x, m_template.anchorMin.y);
        }


        public void CopyFrom(AnchorPreset from)
        {
            CopyFrom(from.m_template);
            UpdateVisuals();
        }

        public void CopyFrom(RectTransform rectTranform)
        {
            Copy(m_template, rectTranform);
            Copy(m_pivot, rectTranform);

            UpdateVisuals();
        }

        private static void Copy(RectTransform to, RectTransform from)
        {
            to.anchorMin = from.anchorMin;
            to.anchorMax = from.anchorMax;
            to.anchoredPosition = from.anchoredPosition;
            to.sizeDelta = from.sizeDelta;
            to.offsetMin = from.offsetMin;
            to.offsetMax = from.offsetMax;
        }

        public void CopyTo(RectTransform rectTranform, bool copyPivot, bool copyPostion)
        {
            Vector3 localPos = rectTranform.localPosition;
            Vector2 size = rectTranform.rect.size;
            rectTranform.anchorMin = m_template.anchorMin;
            rectTranform.anchorMax = m_template.anchorMax;
            rectTranform.localPosition = localPos;
            
            if (copyPivot)
            {
                rectTranform.pivot = m_template.pivot;
                //rectTranform.anchoredPosition = Vector2.zero;
                rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }

            if (copyPostion)
            {
                Vector3 offsetMin = rectTranform.offsetMin;
                Vector3 offsetMax = rectTranform.offsetMax;

                bool anchorsXEqual = Approximately(rectTranform.anchorMin.x, rectTranform.anchorMax.x);
                bool anchorsYEqual = Approximately(rectTranform.anchorMin.y, rectTranform.anchorMax.y);

                if (!anchorsXEqual)
                {
                    offsetMin.x = 0;
                    offsetMax.x = 0;
                }
                
                if (!anchorsYEqual)
                {
                    offsetMin.y = 0;
                    offsetMax.y = 0;
                }

                rectTranform.offsetMin = offsetMin;
                rectTranform.offsetMax = offsetMax;

                if (anchorsXEqual)
                {
                    rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                }

                if (anchorsYEqual)
                {
                    rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                }

                rectTranform.anchoredPosition = m_template.anchoredPosition;
            }
        }

        private Vector2 SetX(Vector2 v, float x)
        {
            v.x = x;
            return v;
        }

        private Vector2 SetY(Vector2 v, float y)
        {
            v.y = y;
            return v;
        }

        private static bool Approximately(float a, float b)
        {
            return RectTransformPropertyConverter.Approximately(a, b);
        }
    }
}
