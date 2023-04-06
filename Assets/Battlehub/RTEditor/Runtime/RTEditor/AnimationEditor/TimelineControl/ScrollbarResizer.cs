using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ScrollbarResizer : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public const float k_minSize = 0.15f;
        public const float k_minValue = 0.0001f;
        public const float k_maxValue = 0.9999f;

        private Scrollbar m_scrollbar;
        private RectTransform m_scrollbarRect;
        private ScrollbarResizer m_other;
        private ScrollRect m_scrollView;
        
        private Vector2 Position
        {
            get { return m_scrollbarRect.InverseTransformPoint(transform.TransformPoint(Vector3.zero)); }
        }

        private Vector2 m_beginDragPoint;
        private Vector2 m_beginDragPosition;
        private bool m_isDragging;
        
        private bool IsEnd
        {
            get
            {
                if(m_scrollbar.direction == Scrollbar.Direction.BottomToTop)
                {
                    return Position.y < m_other.Position.y;
                }
                else if(m_scrollbar.direction == Scrollbar.Direction.LeftToRight)
                {
                    return Position.x > m_other.Position.x;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }

        private bool IsVertical
        {
            get
            {
                if (m_scrollbar.direction == Scrollbar.Direction.BottomToTop)
                {
                    return true;
                }
                else if (m_scrollbar.direction == Scrollbar.Direction.LeftToRight)
                {
                    return false;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }

        private void Awake()
        {
            m_scrollbar = GetComponentInParent<Scrollbar>();

            
            m_scrollbarRect = m_scrollbar.GetComponent<RectTransform>();
            m_other = m_scrollbar.GetComponentsInChildren<ScrollbarResizer>(true).Where(r => r != this).First();
            m_scrollView = GetComponentInParent<ScrollRect>();
            
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_isDragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollbarRect, eventData.position, eventData.pressEventCamera, out m_beginDragPoint);
            
            m_beginDragPosition = Position;
                       
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(!m_isDragging)
            {
                return;
            }

            Vector2 point;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollbarRect, eventData.position, eventData.pressEventCamera, out point))
            {
                RectTransform slidingArea = (RectTransform)m_scrollbar.handleRect.parent;

                RectTransform.Axis axis;
                float viewportSize;
                float slidingAreaSize;
                float slidingAreaSizeDelta;
                float offset;
                if (IsVertical)
                {
                    axis = RectTransform.Axis.Vertical;
                    viewportSize = m_scrollView.viewport.rect.height;
                    slidingAreaSize = slidingArea.rect.height;
                    slidingAreaSizeDelta = slidingArea.sizeDelta.y;
                    point.x = m_beginDragPoint.x;
                    point = ClampPoint(point, slidingAreaSize, slidingAreaSizeDelta);
                    offset = (m_beginDragPoint - point).y;
                    if(!IsEnd)
                    {
                        offset = -offset;
                    }
                }
                else
                {
                    axis = RectTransform.Axis.Horizontal;
                    viewportSize = m_scrollView.viewport.rect.width;
                    slidingAreaSize = slidingArea.rect.width;
                    slidingAreaSizeDelta = slidingArea.sizeDelta.x;
                    point.y = m_beginDragPoint.y;
                    point = ClampPoint(point, slidingAreaSize, slidingAreaSizeDelta);
                    offset = (m_beginDragPoint - point).x;
                    if(IsEnd)
                    {
                        offset = -offset;
                    }
                }
                 
                float sizeRatio;
                float handleSize;
                
                GetHandleSizeAndSizeRatio(slidingAreaSize, offset + slidingAreaSizeDelta, out handleSize, out sizeRatio);
                float newValue = GetNewValue(slidingAreaSize, handleSize, point, ref sizeRatio);

                m_scrollView.content.SetSizeWithCurrentAnchors(axis, viewportSize / sizeRatio);
                m_scrollbar.value = newValue;
            }
        }

        private float GetNewValue(float slidingAreaSize, float handleSize, Vector2 point, ref float sizeRatio)
        {
            float newValue;
            if (IsEnd)
            {
                if(IsVertical)
                {
                    if (Mathf.Approximately(slidingAreaSize, handleSize))
                    {
                        newValue = 0;
                        sizeRatio = k_maxValue;
                    }
                    else
                    {
                        newValue = 1 + m_other.Position.y / (slidingAreaSize - handleSize);
                    }
                }
                else
                {
                    if (Mathf.Approximately(slidingAreaSize, handleSize))
                    {
                        newValue = 0;
                        sizeRatio = k_maxValue;
                    }
                    else
                    {
                        newValue = m_other.Position.x / (slidingAreaSize - handleSize);
                    }
                }
            }
            else
            {
                if(IsVertical)
                {
                    if (Mathf.Approximately(slidingAreaSize, handleSize))
                    {
                        newValue = 0;
                        sizeRatio = k_maxValue;
                    }
                    else
                    {
                        newValue = 1 + (m_beginDragPosition.y - (m_beginDragPoint - point).y) / (slidingAreaSize - handleSize);
                    }
                }
                else
                {
                    if (Mathf.Approximately(slidingAreaSize, handleSize))
                    {
                        newValue = 0;
                        sizeRatio = k_maxValue;
                    }
                    else
                    {
                        newValue = (m_beginDragPosition.x - (m_beginDragPoint - point).x) / (slidingAreaSize  - handleSize);
                    }
                }
            }

            return newValue;
        }

        private Vector2 ClampPoint(Vector2 point, float slidingAreaSize, float slidingAreaSizeDelta)
        {
            if (IsEnd)
            {
                if (IsVertical)
                {
                    float maxY = -slidingAreaSize + slidingAreaSizeDelta;
                    if (point.y + m_beginDragPosition.y - m_beginDragPoint.y < maxY)
                    {
                        point.y = maxY - m_beginDragPosition.y + m_beginDragPoint.y;
                    }
                }
                else
                {
                    float maxX = slidingAreaSize - slidingAreaSizeDelta;
                    if (point.x + m_beginDragPosition.x - m_beginDragPoint.x > maxX)
                    {
                        point.x = maxX - m_beginDragPosition.x + m_beginDragPoint.x;
                    }
                }
                
            }
            else
            {
                if (IsVertical)
                {
                    float minY = 0;
                    if (point.y + m_beginDragPosition.y - m_beginDragPoint.y > minY)
                    {
                        point.y = minY - m_beginDragPosition.y + m_beginDragPoint.y;
                    }
                }
                else
                {
                    float minX = 0;
                    if (point.x + m_beginDragPosition.x - m_beginDragPoint.x < minX)
                    {
                        point.x = minX - m_beginDragPosition.x + m_beginDragPoint.x;
                    }
                }
                
            }

            return point;
        }

        private void GetHandleSizeAndSizeRatio(float slidingAreaSize,  float offset, out float handleSize, out float sizeRatio)
        {
            handleSize = (m_beginDragPosition - m_other.Position).magnitude + offset;
            sizeRatio = handleSize / slidingAreaSize;
            sizeRatio = Mathf.Min(k_maxValue, Mathf.Max(k_minSize, sizeRatio));
            handleSize = slidingAreaSize * sizeRatio;
        }

        public void OnDrop(PointerEventData eventData)
        {
            m_scrollbar.value = Mathf.Clamp(m_scrollbar.value, k_minValue, k_maxValue);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_scrollbar.value = Mathf.Clamp(m_scrollbar.value, k_minValue, k_maxValue);
        }
    }
}

