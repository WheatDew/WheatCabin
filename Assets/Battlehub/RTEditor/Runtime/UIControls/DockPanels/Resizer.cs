using Battlehub.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public delegate void ResizerEventHandler(Resizer resizer, Region region);

    public class Resizer : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static event ResizerEventHandler BeginResize;
        public static event ResizerEventHandler Resize;
        public static event ResizerEventHandler EndResize;

        [SerializeField]
        private Texture2D m_cursor = null;

        [SerializeField]
        private Region m_region;

        private RectTransform m_parentRT;
        private LayoutElement m_layout;
        private LayoutElement m_siblingLayout;

        private bool m_isEnabled;
        private bool m_isFree;
        
        [SerializeField]
        private float m_dx = -1;
        [SerializeField]
        private float m_dy = -1;
        private Vector2 m_adjustment;
        private bool m_isDragging;

        [SerializeField]
        private bool m_forceRebuildLayoutImmediate = true;

        private void Awake()
        {
            if(m_region == null)
            {
                m_region = GetComponentInParent<Region>();
            }

            m_layout = m_region.GetComponent<LayoutElement>();
        }

        private void OnDestroy()
        {
            if(m_region != null && m_region.Root != null)
            {
                m_region.Root.CursorHelper.ResetCursor(this);
            }
        }

        private void Start()
        {
            if (m_region == m_region.Root.RootRegion)
            {
                Destroy(gameObject);
            }
            else
            {
                UpdateState();
            }
        }

        public void UpdateState()
        {
            if(gameObject.activeInHierarchy)
            {
                StartCoroutine(CoUpdateState());
            }
        }

        private IEnumerator CoUpdateState()
        {
            yield return new WaitForEndOfFrame();
            if(m_region.transform.parent == null)
            {
                yield break;
            }

            m_isEnabled = m_isFree = m_region.IsFreeOrModal();
            if (!m_isFree)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, transform.position);

                RectTransform rt = (RectTransform)transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, canvas.worldCamera, out m_adjustment);

                if (m_dx == 0 || m_dy == 0)
                {
                    int siblingIndex = m_region.transform.GetSiblingIndex();

                    HorizontalOrVerticalLayoutGroup layoutGroup = m_region.GetComponentInParent<HorizontalOrVerticalLayoutGroup>();
                    if (layoutGroup is HorizontalLayoutGroup)
                    {
                        if (siblingIndex == 0 && m_dx > 0)
                        {
                            m_isEnabled = true;
                        }
                        else if (siblingIndex == 1 && m_dx < 0)
                        {
                            m_isEnabled = true;
                        }
                    }
                    else
                    {
                        if (siblingIndex == 0 && m_dy < 0)
                        {
                            m_isEnabled = true;
                        }
                        else if (siblingIndex == 1 && m_dy > 0)
                        {
                            m_isEnabled = true;
                        }
                    }

                    if (m_isEnabled && m_region.transform.parent.childCount > (siblingIndex + 1) % 2)
                    {
                        m_siblingLayout = m_region.transform.parent.GetChild((siblingIndex + 1) % 2).GetComponent<LayoutElement>();
                        m_parentRT = (RectTransform)m_region.transform.parent;
                    }
                    else
                    {
                        m_isEnabled = false;
                        m_siblingLayout = null;
                        m_parentRT = null;
                    }

                }
            }
            else
            {
                m_siblingLayout = null;
                m_parentRT = null;
            }
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if(!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }
           

            eventData.useDragThreshold = false;

            if(m_cursor != null)
            {
                m_region.Root.CursorHelper.SetCursor(this, m_cursor);
            }
            else
            {
                m_region.Root.CursorHelper.SetCursor(this, m_dx != 0 ? KnownCursor.HResize : KnownCursor.VResize);
            }   
        }


        private HorizontalOrVerticalLayoutGroup[] m_layoutGroups;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }
            if (eventData.pointerId == -2 || eventData.pointerId == -3)
            {
                return;
            }

            if (m_isFree)
            {
                Vector2 position = eventData.position;
                Camera camera = eventData.pressEventCamera;

                RectTransform rt = (RectTransform)transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out m_adjustment);
            }
            if (m_forceRebuildLayoutImmediate)
            {
                List<HorizontalOrVerticalLayoutGroup> lgList = new List<HorizontalOrVerticalLayoutGroup>();
                Region[] regions = m_region.Root.GetComponentsInChildren<Region>();
                for(int i = 0; i < regions.Length; ++i)
                {
                    HorizontalOrVerticalLayoutGroup lg = regions[i].ChildrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if(lg != null)
                    {
                        lgList.Add(lg);
                    }
                    lg = regions[i].Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if(lg != null)
                    {
                        lgList.Add(lg);
                    }
                }
                m_layoutGroups = lgList.ToArray();

            }

            if (BeginResize != null)
            {
                BeginResize(this, m_region);
            }
           
            m_isDragging = true;
        }


        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }
            if(!m_isDragging)
            {
                return;
            }

            if(m_isFree)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)m_region.Root.transform, eventData.position, eventData.pressEventCamera))
                {
                    return;
                }

                RectTransform rt = (RectTransform)transform;
                RectTransform regionRT = (RectTransform)m_region.transform;
                RectTransform freePanelRt = (RectTransform)m_region.Root.Free;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(freePanelRt, eventData.position, eventData.pressEventCamera, out Vector2 point);

                Vector2 pivotOffset = Vector2.Scale(freePanelRt.rect.size, freePanelRt.pivot);
                pivotOffset.y *= -1;

                point += pivotOffset;

                Vector2 offsetMin = regionRT.offsetMin;
                Vector2 offsetMax = regionRT.offsetMax;

                float size = Mathf.Min(rt.rect.width, rt.rect.height);
                float minWidth = m_layout.minWidth + size;
                float minHeight = m_layout.minHeight + size;

                if (m_dx < 0)
                {
                    offsetMin.x = point.x - m_adjustment.x;
                    if (offsetMax.x - offsetMin.x < minWidth)
                    {
                        offsetMin.x = offsetMax.x - minWidth;
                    }
                }
                else if (m_dx > 0)
                {
                    offsetMax.x = point.x - m_adjustment.x;
                    if (offsetMax.x - offsetMin.x < minWidth)
                    {
                        offsetMax.x = offsetMin.x + minWidth;
                    }
                }

                if (m_dy < 0)
                {
                    offsetMin.y = point.y - m_adjustment.y;
                    if (offsetMax.y - offsetMin.y < minHeight)
                    {
                        offsetMin.y = offsetMax.y - minHeight;
                    }
                }
                else if (m_dy > 0)
                {
                    offsetMax.y = point.y - m_adjustment.y;
                    if (offsetMax.y - offsetMin.y < minHeight)
                    {
                        offsetMax.y = offsetMin.y + minHeight;
                    }
                }

                regionRT.offsetMin = offsetMin;
                regionRT.offsetMax = offsetMax;
            }
            else
            {
                Vector2 size = m_parentRT.rect.size - new Vector2(m_layout.minWidth + m_siblingLayout.minWidth, m_layout.minHeight + m_siblingLayout.minHeight);
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRT, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                {
                    Vector2 pivotPosition = m_parentRT.rect.size * m_parentRT.pivot;

                    const float kMinFlexSize = 0.00000001f;

                    if (m_dx > 0 || m_dy > 0)
                    {
                        localPoint = pivotPosition + localPoint - new Vector2(m_layout.minWidth, m_layout.minHeight);

                        m_layout.flexibleWidth = Mathf.Max(kMinFlexSize, (size.x == 0) ? 1 : localPoint.x / size.x);
                        m_layout.flexibleHeight = Mathf.Max(kMinFlexSize, (size.y == 0) ? 1 : localPoint.y / size.y);
                        m_siblingLayout.flexibleWidth = Mathf.Max(kMinFlexSize, 1 - m_layout.flexibleWidth);
                        m_siblingLayout.flexibleHeight = Mathf.Max(kMinFlexSize, 1 - m_layout.flexibleHeight);
                    }
                    else
                    {
                        localPoint = pivotPosition + localPoint - new Vector2(m_siblingLayout.minWidth, m_siblingLayout.minHeight);

                        m_siblingLayout.flexibleWidth = Mathf.Max(kMinFlexSize, (size.x == 0) ? 1 : localPoint.x / size.x);
                        m_siblingLayout.flexibleHeight = Mathf.Max(kMinFlexSize, (size.y == 0) ? 1 : localPoint.y / size.y);
                        m_layout.flexibleWidth = Mathf.Max(kMinFlexSize, 1 - m_siblingLayout.flexibleWidth);
                        m_layout.flexibleHeight = Mathf.Max(kMinFlexSize, 1 - m_siblingLayout.flexibleHeight);
                    }
                }  
            }

            if(m_forceRebuildLayoutImmediate)
            {
                if(m_layoutGroups == null)
                {
                    Debug.LogError("m_layoutGroups is null " + m_region.Root.name);
                }
                else
                {
                    for (int i = 0; i < m_layoutGroups.Length; ++i)
                    {
                        HorizontalOrVerticalLayoutGroup lg = m_layoutGroups[i];
                        if(lg == null)
                        {
                            Debug.LogError("layoutGroup " + i + " is null " + m_region.Root.name);
                            continue;
                        }
                        RectTransform rt = (RectTransform)lg.transform;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                    }
                }
            }
            
            if (Resize != null)
            {
                Resize(this, m_region);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            m_isDragging = false;
            m_layoutGroups = null;
            if (!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }
            
            m_region.Root.CursorHelper.ResetCursor(this);

            if(EndResize != null)
            {
                EndResize(this, m_region);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }

            if (!m_isDragging)
            {
                if(m_cursor != null)
                {
                    m_region.Root.CursorHelper.SetCursor(this, m_cursor);
                }
                else
                {
                    m_region.Root.CursorHelper.SetCursor(this, m_dx != 0 ? KnownCursor.HResize : KnownCursor.VResize);
                }
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!m_region.CanResize)
            {
                return;
            }
            if (!m_isEnabled)
            {
                return;
            }
            if (!m_isDragging)
            {
                m_region.Root.CursorHelper.ResetCursor(this);
            }
        }
    }
}

