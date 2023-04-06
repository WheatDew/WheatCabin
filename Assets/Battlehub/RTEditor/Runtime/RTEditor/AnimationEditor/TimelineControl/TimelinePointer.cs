using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public delegate void TimelinePointerEvent();
    public delegate void TimelinePointerEvent<T>(T arg);
    
    public class TimelinePointer : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public class PointerArgs
        {
            public int Row;
            public int Col;
            public int Range;
        }

        public event TimelinePointerEvent<int> SampleChanged;
        public event TimelinePointerEvent<PointerArgs> PointerDown;
        public event TimelinePointerEvent BeginDrag;
        public event TimelinePointerEvent<int> Drag;
        public event TimelinePointerEvent Drop;

        [SerializeField]
        private RectTransform m_pointer = null;

        [SerializeField]
        private RectTransform m_timelineArea = null;

        [SerializeField]
        private RectTransform m_workArea = null;

        private TimelineGridParameters m_parameters;

        private float m_visibleColumns;
        private float m_columnWidth;
        private int m_sample;
        private Vector2 m_offset;
        private bool m_isPointerDragInProgress;
        public bool IsPointerDragInProgress
        {
            get { return m_isDragInProgress; }
        }

        private bool m_isDragInProgress;
        public bool IsDragInProgress
        {
            get { return m_isDragInProgress; }
            set { m_isDragInProgress = value; }
        }
        private Vector2Int m_prevCoord;

        private int m_samplesCount;
        public int ColumnsCount
        {
            get { return m_samplesCount; }
        }

        public int Range
        {
            get { return Mathf.FloorToInt(5.0f / m_columnWidth); }
        }
        
        public int GetSample()
        {
            return m_sample;
        }

        public void SetSample(int sample, bool raiseEvent)
        {
            int oldSample = m_sample;
            m_sample = sample;

            Vector3 pos = m_pointer.transform.localPosition;
            pos.x = (m_sample - m_offset.x) * m_columnWidth;
            m_pointer.transform.localPosition = pos;

            if (raiseEvent && oldSample != m_sample)
            {
                if (SampleChanged != null)
                {
                    SampleChanged(m_sample);
                }
            }
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            Vector2 maxSupportedViewportSize = new Vector2(4096, 4096);
            SetGridParameters(parameters, maxSupportedViewportSize);
        }

        public void SetGridParameters(TimelineGridParameters parameters, Vector2 viewportSize)
        {
            m_parameters = parameters;
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval)
        {
            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            m_samplesCount = Mathf.FloorToInt(m_parameters.VertLines * interval.x) + 1;

            float px = interval.x * normalizedSize.x;
            m_visibleColumns = m_parameters.VertLines * Mathf.Pow(m_parameters.VertLinesSecondary, Mathf.Log(px, m_parameters.VertLinesSecondary));
            m_columnWidth = viewportSize.x / m_visibleColumns;

            int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            int vLinesCount = m_parameters.VertLines;

            m_offset.x = -(1 - 1 / normalizedSize.x) * normalizedOffset.x  * m_visibleColumns;
            m_offset.y = (1 - normalizedSize.y) * (1 - normalizedOffset.y) * (m_parameters.HorLines - 1);

            Vector3 pos = m_pointer.transform.localPosition;
            pos.x = (m_sample - m_offset.x) * m_columnWidth;
            m_pointer.transform.localPosition = pos;
        }

        private void UpdatePointerPosition(PointerEventData eventData)
        {
            Vector2Int coord;
            if (GetKeyframeCoord(eventData, false, out coord))
            {
                SetSample(coord.x, true);
            }
        }

        private bool GetKeyframeCoord(PointerEventData eventData, bool precise, out Vector2Int coord)
        {
            Vector2 point;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_workArea, eventData.position, eventData.pressEventCamera, out point))
            {
                coord = new Vector2Int(-1, -1);
                return false;
            }

            return GetKeyframeCoordinate(point, precise, false, out coord);
        }

        public Vector2 GetViewportPosition(Vector2Int coord)
        {
            return new Vector2(
                (coord.x - m_offset.x) * m_columnWidth ,
                -(coord.y - m_offset.y + 1) * m_parameters.FixedHeight);
        }

        public bool GetKeyframeCoordinate(Vector2 point, bool precise, bool rowCenter, out Vector2Int coord)
        {
            const float margin = 0.15f;
            bool result = true;

            float sampleF = m_offset.x + point.x / m_columnWidth;
            int sample = Mathf.RoundToInt(sampleF);
            if (sample < 0)
            {
                sample = 0;
            }
            else if(sample >= m_samplesCount)
            {
                sample = m_samplesCount - 1;
            }

            if (precise)
            {
                float distance = Mathf.Abs(sample - sampleF);
                distance *= m_columnWidth;
                if (distance > m_parameters.FixedHeight * (1 - 4.0f * margin))
                {
                    sample = -1;
                    result = false;
                }
            }

            float rowF = m_offset.y - (point.y / m_parameters.FixedHeight);
            int row = rowCenter ? Mathf.RoundToInt(rowF) : Mathf.FloorToInt(rowF);

            if (precise)
            {
                int nextRow = Mathf.CeilToInt(rowF);
                if (rowF - row < margin ||
                    nextRow - rowF < margin)
                {
                    row = 0;
                    result = false;
                }
            }

            coord = new Vector2Int(sample, row);
            return result;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_timelineArea, eventData.position, eventData.pressEventCamera))
            {
                UpdatePointerPosition(eventData);
            }
            else
            {
                Vector2Int coord;
                if (!GetKeyframeCoord(eventData, true, out coord))
                {
                    coord = new Vector2Int(-1, -1);
                }

                                
                if (PointerDown != null)
                {
                    PointerArgs args = new PointerArgs();
                    args.Col = coord.x;
                    args.Row = coord.y;
                    args.Range = Range;
                    PointerDown(args);
                }
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_timelineArea, eventData.position, eventData.pressEventCamera))
            {
                m_isPointerDragInProgress = true;
            }
            else
            {
                m_isPointerDragInProgress = false;
                m_isDragInProgress = GetKeyframeCoord(eventData, false, out m_prevCoord);
                if(BeginDrag != null)
                {
                    BeginDrag();
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_isPointerDragInProgress)
            {
                UpdatePointerPosition(eventData);
            }
            else
            {
                if(m_isDragInProgress)
                {
                    if (Drag != null)
                    {
                        Vector2Int coord;
                        if (GetKeyframeCoord(eventData, false, out coord) && coord != m_prevCoord)
                        {
                            Drag(coord.x - m_prevCoord.x);
                            m_prevCoord = coord;
                        }
                    }
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if(m_isDragInProgress)
            {
                if(Drop != null)
                {
                    Drop();
                }
            }

            m_isPointerDragInProgress = false;
            m_isDragInProgress = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_isDragInProgress)
            {
                if (Drop != null)
                {
                    Drop();
                }
            }

            m_isPointerDragInProgress = false;
            m_isDragInProgress = false;
        }
    }
}

