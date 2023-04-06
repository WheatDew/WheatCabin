using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class TimelineBoxSelectionCancelArgs
    {
        public bool Cancel
        {
            get;
            set;
        }

        public Vector2 LocalPoint
        {
            get;
            private set;
        }

        public TimelineBoxSelectionCancelArgs(Vector2 localPoint)
        {
            LocalPoint = localPoint;
        }
    }
    public delegate void TimelineBoxSelectionEvent<T>(T args);
    public delegate void TimelineBoxSelectionEvent(Vector2Int min, Vector2Int max);

    public class TimelineBoxSelection : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public event TimelineBoxSelectionEvent<TimelineBoxSelectionCancelArgs> BeginSelection;   
        public event TimelineBoxSelectionEvent Selection;

        [SerializeField]
        private RectTransform m_box = null;
        [SerializeField]
        private TimelinePointer m_pointer = null;
        private RectTransform m_rt;
        private bool m_isInProgress;
        private bool m_isReady;

        private Vector2Int m_start1;
        private Vector2Int m_start2;
        private Vector2Int m_end;

        private void Awake()
        {
            m_rt = (RectTransform)transform;
            if(m_pointer == null)
            {
                m_pointer = GetComponentInParent<TimelinePointer>();
            }
        }

        private void Start()
        {
            m_box.sizeDelta = new Vector2(0, 0);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);

            m_isReady = false;

            Vector2 point;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rt, eventData.position, eventData.pressEventCamera, out point))
            {
                Vector2Int coord;
                if (m_pointer.GetKeyframeCoordinate(point, false, false, out coord))
                {
                    m_start1 = new Vector2Int(coord.x, coord.y - 1);
                    m_start2 = new Vector2Int(coord.x, coord.y);

                    if (BeginSelection != null)
                    {
                        TimelineBoxSelectionCancelArgs cancelArgs = new TimelineBoxSelectionCancelArgs(point);
                        BeginSelection(cancelArgs);
                        if(!cancelArgs.Cancel)
                        {
                            m_isReady = true;
                        }
                    }
                    else
                    {
                        m_isReady = true;
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);

            m_isInProgress = m_isReady;
            if(m_isInProgress)
            {
                Vector2 startPoint = m_pointer.GetViewportPosition(m_start1);
                Vector2 endPoint = m_pointer.GetViewportPosition(m_start2);

                m_box.anchoredPosition = startPoint;
                Vector2 size = endPoint - startPoint;
                m_box.sizeDelta = new Vector2(Mathf.Max(2, Mathf.Abs(size.x)), Mathf.Abs(size.y));

            }
        }


        public void OnDrag(PointerEventData eventData)
        {

            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
            if (!m_isInProgress)
            {
                return;
            }

            Vector2 point;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rt, eventData.position, eventData.pressEventCamera, out point))
            {
                Vector2Int coord;
                if (m_pointer.GetKeyframeCoordinate(point, false, true, out coord))
                {
                    m_end = new Vector2Int(coord.x, coord.y - 1); 

                    Vector2Int min = new Vector2Int(Mathf.Min(m_start1.x, m_start2.x, m_end.x), Mathf.Min(m_start1.y, m_start2.y, m_end.y));
                    Vector2Int max = new Vector2Int(Mathf.Max(m_start1.x, m_start2.x, m_end.x), Mathf.Max(m_start1.y, m_start2.y, m_end.y));

                    point = m_pointer.GetViewportPosition(min);
                    m_box.anchoredPosition = point;

                    Vector2 size = point - m_pointer.GetViewportPosition(max);
                    m_box.sizeDelta = new Vector2(Mathf.Max(2, Mathf.Abs(size.x)), Mathf.Abs(size.y));
                }
            }

            
        }

        public void OnDrop(PointerEventData eventData)
        {
            Drop();

            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dropHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Drop();

            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
        }

        private void Drop()
        {
            if (m_isInProgress)
            {
                if (Selection != null)
                {
                    Vector2Int min = new Vector2Int(Mathf.Min(m_start1.x, m_start2.x, m_end.x), Mathf.Min(m_start1.y + 1, m_start2.y + 1, m_end.y + 1));
                    Vector2Int max = new Vector2Int(Mathf.Max(m_start1.x, m_start2.x, m_end.x), Mathf.Max(m_start1.y, m_start2.y, m_end.y));

                    Selection(min, max);
                }
            }

            m_isReady = false;
            m_isInProgress = false;
            m_box.sizeDelta = new Vector2(0, 0);
        }

    }

}

