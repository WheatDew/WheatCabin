using Battlehub.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class DockPanel : MonoBehaviour
    {
        public event RegionEventHandler<Transform> TabActivated;
        public event RegionEventHandler<Transform> TabDeactivated;
        public event RegionEventHandler<Transform> TabClosed;
        public event RegionEventHandler TabBeginDrag;
        public event RegionEventHandler TabEndDrag;

        public event RegionEventHandler RegionSelected;
        public event RegionEventHandler RegionUnselected;
        public event RegionEventHandler RegionCreated;
        public event RegionEventHandler<CancelArgs> RegionBeforeDepthChanged;
        public event RegionEventHandler<int> RegionDepthChanged;
        public event RegionEventHandler RegionDestroyed;
        public event RegionEventHandler RegionEnabled;
        public event RegionEventHandler RegionDisabled;
        public event RegionEventHandler<bool> RegionMaximized;

        public event RegionEventHandler<CancelArgs> RegionBeforeBeginDrag;
        public event RegionEventHandler RegionBeginDrag;
        public event RegionEventHandler RegionDrag;
        public event RegionEventHandler RegionEndDrag;
        public event RegionEventHandler RegionTranformChanged;

        public event ResizerEventHandler RegionBeginResize;
        public event ResizerEventHandler RegionResize;
        public event ResizerEventHandler RegionEndResize;

        [SerializeField]
        private BaseRaycaster m_raycaster = null;
        public BaseRaycaster Raycaster
        {
            get { return m_raycaster; }
        }

        [SerializeField]
        private Tab m_tabPrefab = null;
        public Tab TabPrefab
        {
            get { return m_tabPrefab; }
            set { m_tabPrefab = value; }
        }

        [SerializeField]
        private Region m_regionPrefab = null;
        public Region RegionPrefab
        {
            get { return m_regionPrefab; }
            set { m_regionPrefab = value;  }
        }

        private Region m_selectedRegion;
        public Region SelectedRegion
        {
            get { return m_selectedRegion; }
        }

        [SerializeField]
        private Region m_rootRegion = null;
        public Region RootRegion
        {
            get { return m_rootRegion; }
        }

        [SerializeField]
        private Transform m_docked = null;
        public Transform Docked
        {
            get { return m_docked; }
        }

        [SerializeField]
        private Transform m_free = null;
        public Transform Free
        {
            get { return m_free; }
        }

        [SerializeField]
        private RectTransform m_preview = null;
        public RectTransform Preview
        {
            get { return m_preview; }
        }

        [SerializeField]
        private RectTransform m_modal = null;
        public RectTransform Modal
        {
            get { return m_modal; }
        }

        private CursorHelper m_cursorHelper = new CursorHelper();
        public CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
            set { m_cursorHelper = value; }
        }

        private int m_regionId;
        public int RegionId
        {
            get { return m_regionId; }
            set { m_regionId = value; }
        }


        [HideInInspector, SerializeField]
        private Mask m_mask = null;

        [HideInInspector, SerializeField]
        private bool m_allowDragOutside = false;
        public bool AllowDragOutside
        {
            get { return m_allowDragOutside; }
        }

        [SerializeField]
        private float m_minWidth = 0;
        public float MinWidth
        {
            get { return m_minWidth; }
        }

        [SerializeField]
        private float m_minHeight = 0;
        public float MinHeight
        {
            get { return m_minHeight; }
        }

        private void Awake()
        {
            if (m_raycaster == null)
            {
                m_raycaster = GetComponentInParent<BaseRaycaster>();
                if (m_raycaster == null)
                {
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas)
                    {
                        m_raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                    }
                }
            }
            Region.Selected += OnRegionSelected;
            Region.Unselected += OnRegionUnselected;
            Region.Created += OnRegionCreated;
            Region.BeforeDepthChanged += OnRegionBeforeDepthChanged;
            Region.DepthChanged += OnRegionDepthChanged;
            Region.Destroyed += OnRegionDestroyed;
            Region.Enabled += OnRegionEnabled;
            Region.Disabled += OnRegionDisabled;
            Region.Maximized += OnRegionMaximized;

            Region.BeforeBeginDrag += OnRegionBeforeBeginDrag;
            Region.BeginDrag += OnRegionBeginDrag;
            Region.Drag += OnRegionDrag;
            Region.EndDrag += OnRegionEndDrag;
            Region.TransformChanged += OnRegionTranformChanged;

            Resizer.BeginResize += OnRegionBeginResize;
            Resizer.Resize += OnRegionResize;
            Resizer.EndResize += OnRegionEndResize;

            Region.TabActivated += OnTabActivated;
            Region.TabDeactivated += OnTabDeactivated;
            Region.TabClosed += OnTabClosed;
            Region.TabBeginDrag += OnTabBeginDrag;
            Region.TabEndDrag += OnTabEndDrag;

            if (m_rootRegion == null)
            {
                m_rootRegion = GetComponentInChildren<Region>();
            }

            if (m_rootRegion == null)
            {
                m_rootRegion = Instantiate(m_regionPrefab, m_docked);
                m_rootRegion.name = "Root Region";
                //if(m_rootRegion.FrameImage != null)
                //{
                //    m_rootRegion.FrameImage.enabled = false;
                //}

                //if(m_rootRegion.ContentRoot != null)
                //{
                //    m_rootRegion.ContentRoot.Stretch();
                //}
            }
        }

        private void Start()
        {
            if (m_mask != null)
            {
                m_mask.enabled = !m_allowDragOutside;
            }
        }

        private void OnDestroy()
        {
            Region.Selected -= OnRegionSelected;
            Region.Unselected -= OnRegionUnselected;
            Region.Created -= OnRegionCreated;
            Region.BeforeDepthChanged -= OnRegionBeforeDepthChanged;
            Region.DepthChanged -= OnRegionDepthChanged;
            Region.Destroyed -= OnRegionDestroyed;
            Region.Enabled -= OnRegionEnabled;
            Region.Disabled -= OnRegionDisabled;
            Region.Maximized -= OnRegionMaximized;

            Region.BeforeBeginDrag -= OnRegionBeforeBeginDrag;
            Region.BeginDrag -= OnRegionBeginDrag;
            Region.Drag -= OnRegionDrag;
            Region.EndDrag -= OnRegionEndDrag;
            Region.TransformChanged -= OnRegionTranformChanged;

            Resizer.BeginResize -= OnRegionBeginResize;
            Resizer.Resize -= OnRegionResize;
            Resizer.EndResize -= OnRegionEndResize;

            Region.TabActivated -= OnTabActivated;
            Region.TabDeactivated -= OnTabDeactivated;
            Region.TabClosed -= OnTabClosed;
            Region.TabBeginDrag -= OnTabBeginDrag;
            Region.TabEndDrag -= OnTabEndDrag;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Free != null)
            {
                foreach (Transform child in Free)
                {
                    Region region = child.GetComponent<Region>();
                    if (region != null)
                    {
                        region.Fit();
                    }
                }
            }
        }

        private void OnTabActivated(Region region, Transform arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if (TabActivated != null)
            {
                TabActivated(region, arg);
            }
        }

        private void OnTabDeactivated(Region region, Transform arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if (TabDeactivated != null)
            {
                TabDeactivated(region, arg);
            }
        }

        private void OnTabClosed(Region region, Transform arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if (m_selectedRegion == region)
            {
                m_selectedRegion.IsSelected = false;
            }

            if (TabClosed != null)
            {
                TabClosed(region, arg);
            }
        }

        private void OnTabBeginDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (TabBeginDrag != null)
            {
                TabBeginDrag(region);
            }
        }


        private void OnTabEndDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(TabEndDrag != null)
            {
                TabEndDrag(region);
            }
        }

        private void OnRegionSelected(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (m_selectedRegion != null)
            {
                m_selectedRegion.IsSelected = false;
            }

            m_selectedRegion = region;

            if (RegionSelected != null)
            {
                RegionSelected(m_selectedRegion);
            }
        }

        private void OnRegionUnselected(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (m_selectedRegion == region)
            {
                m_selectedRegion = null;
                if (RegionUnselected != null)
                {
                    RegionUnselected(region);
                }
            }
        }

        private void OnRegionBeforeDepthChanged(Region region, CancelArgs arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionBeforeDepthChanged != null)
            {
                RegionBeforeDepthChanged(region, arg);
            }
        }

        private void OnRegionDepthChanged(Region region, int depth)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionDepthChanged != null)
            {
                RegionDepthChanged(region, depth);
            }
        }

        private void OnRegionCreated(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionCreated != null)
            {
                RegionCreated(region);
            }
        }

        private void OnRegionDestroyed(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (m_modal != null)
            {
                if (region.transform.parent == m_modal)
                {
                    if (m_modal.childCount == 1)
                    {
                        StartCoroutine(CoDisableModal());
                    }
                }
            }

            if (RegionDestroyed != null)
            {
                RegionDestroyed(region);
            }
        }

        private void OnRegionEnabled(Region region)
        {
            if (RegionEnabled != null)
            {
                RegionEnabled(region);
            }
        }

        private void OnRegionDisabled(Region region)
        {
            if (RegionDisabled != null)
            {
                RegionDisabled(region);
            }
        }


        private void OnRegionMaximized(Region region, bool maximized)
        {
            if (RegionMaximized != null)
            {
                RegionMaximized(region, maximized);
            }
        }

        private IEnumerator CoDisableModal()
        {
            yield return new WaitForEndOfFrame();
            if (m_modal.childCount == 0)
            {
                m_modal.gameObject.SetActive(false);
            }
        }

        private void OnRegionBeginResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionBeginResize != null)
            {
                RegionBeginResize(resizer, region);
            }
        }

        private void OnRegionResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionResize != null)
            {
                RegionResize(resizer, region);
            }
        }

        private void OnRegionEndResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionEndResize != null)
            {
                RegionEndResize(resizer, region);
            }
        }

        private void OnRegionBeforeBeginDrag(Region region, CancelArgs args)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionBeforeBeginDrag != null)
            {
                RegionBeforeBeginDrag(region, args);
            }
        }

        private void OnRegionBeginDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionBeginDrag != null)
            {
                RegionBeginDrag(region);
            }
        }

        private void OnRegionDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionDrag != null)
            {
                RegionDrag(region);
            }
        }

        private void OnRegionEndDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionEndDrag != null)
            {
                RegionEndDrag(region);
            }
        }


        private void OnRegionTranformChanged(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionTranformChanged != null)
            {
                RegionTranformChanged(region);
            }
        }

        public void AddRegion(Tab tab, Transform content, bool isFree = false, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f)
        {
            m_rootRegion.Add(tab, content, isFree, splitType, flexibleSize);
        }

        public void RemoveRegion(Transform content)
        {
            Tab tab = Region.FindTab(content);
            if(tab != null)
            {
                tab.Close();
            }
        }

        public void AddPopupRegion(Tab tab, Transform content)
        {
            m_rootRegion.Add(tab, content, true, RegionSplitType.None, 0);
            
            Region region = Region.FindRegion(content);
            region.gameObject.AddComponent<RegionPopupBehaviour>();
        }

        public void AddPopupRegion(Tab tab, Transform content, Rect rect)
        {
            AddPopupRegion(tab, content, rect, new Vector2(0, 1));
        }

        public void AddPopupRegion(Tab tab, Transform content, Rect rect, Vector2 pivot, bool canResize = false)
        {
            m_rootRegion.Add(tab, content, true, RegionSplitType.None, 0);

            Region region = Region.FindRegion(content);
            region.gameObject.AddComponent<RegionPopupBehaviour>();
            region.MinWidth = Mathf.Min(rect.width, region.MinWidth);
            region.MinHeight = Mathf.Min(rect.height, region.MinHeight);
            region.CanResize = canResize;

            if (region.FrameImage != null)
            {
                region.FrameImage.enabled = false;
            }

            RectTransform free = (RectTransform)Free;
            RectTransform rt = (RectTransform)region.transform;
            rt.anchorMin = free.pivot;
            rt.anchorMax = free.pivot;
            rt.pivot = pivot;
            rt.sizeDelta = rect.size;
            rt.anchoredPosition = rect.position;
        }

        public void AddDropdownRegion(Tab tab, Transform content, RectTransform anchor, Vector2 size, bool canResize = false)
        {
            AddPopupRegion(tab, content, new Rect(0, 0, size.x, size.y), new Vector2(0, 1), canResize);

            RefreshDropdownRegion(content, anchor);
        }

        public void RefreshDropdownRegion(Transform content, RectTransform anchor)
        {
            Region region = Region.FindRegion(content);
            RectTransform regionRT = (RectTransform)region.transform;
            regionRT.pivot = new Vector2(0, 1);
            regionRT.anchorMin = new Vector2(0, 1);
            regionRT.anchorMax = new Vector2(0, 1);

            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];
            Vector3 topLeft = corners[1];

            Transform panel = Free;
         
            regionRT.localPosition = panel.InverseTransformPoint(bottomLeft);

            region.Fit(false);
            regionRT.GetWorldCorners(corners);

            if (Mathf.Abs(bottomLeft.y - corners[1].y) > 0.001f)
            {
                regionRT.pivot = Vector2.zero;
                regionRT.anchorMin = Vector2.zero;
                regionRT.anchorMax = Vector2.zero;
                regionRT.localPosition = panel.InverseTransformPoint(topLeft);

                regionRT.SetAnchors(new Vector2(0, 1), new Vector2(0, 1));

                region.Fit(false);

                OnRegionTranformChanged(region);
            }
            else
            {
                OnRegionTranformChanged(region);
            }
        }

       
        public void AddModalRegion(Transform headerContent, Transform content, float ratio = 0.7f, bool canResize = true)
        {
            AddModalRegion(headerContent, content, m_modal.rect.width * ratio, m_modal.rect.height * ratio);
        }

        public void AddModalRegion(Transform headerContent, Transform content, float minWidth, float minHeight, bool canResize = true, float margin = 50)
        {
            Rect rect = new Rect(
                Mathf.Max(0, m_modal.rect.width - minWidth) / 2,
                -Mathf.Max(0, m_modal.rect.height - minHeight) / 2,
                minWidth,
                minHeight);

            AddModalRegion(headerContent, content, minWidth, minHeight, rect, true, canResize, margin);
        }

        public void AddModalRegion(Transform headerContent, Transform content, float minWidth, float minHeight, Rect rect, bool center = false, bool canResize = true, float margin = 50)
        {
            AddModalRegion(headerContent, content, minWidth, minHeight, rect, new Vector2(0, 1), center, canResize, margin);
        }

        public void AddModalRegion(Transform headerContent, Transform content, float minWidth, float minHeight, Rect rect, Vector2 pivot, bool center = false, bool canResize = true, float margin = 50)
        {
            m_modal.gameObject.SetActive(true);

            Region region = Instantiate(m_regionPrefab, m_modal);
            region.name = "Region " + m_regionId++;
            RectTransform rt = (RectTransform)region.transform;

            headerContent.SetParent(region.HeaderImage.transform, false);
            //RectTransform headerContentRT = (RectTransform)headerContent;
            //headerContentRT.Stretch();

            content.SetParent(region.ContentPanel, false);
            RectTransform contentRT = (RectTransform)content;
            contentRT.Stretch();

            region.CanResize = canResize;
            region.MinWidth = minWidth;
            region.MinHeight = minHeight;

            if (rect.width > m_modal.rect.width - margin * 2)
            {
                rect.width = m_modal.rect.width - margin * 2;
            }

            if (rect.height > m_modal.rect.height - margin * 2)
            {
                rect.height = m_modal.rect.height - margin * 2;
            }

            rect.width = Mathf.Max(rect.width, minWidth);
            rect.height = Mathf.Max(rect.height, minHeight);

            if (center)
            {
                rect.position = new Vector2(Mathf.Max(0, m_modal.rect.width - rect.width) / 2,
                                           -Mathf.Max(0, m_modal.rect.height - rect.height) / 2);
            }

            rt.pivot = pivot;
            rt.anchorMin = pivot;
            rt.anchorMax = pivot;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);

            rt.anchoredPosition = rect.position;
            region.Fit();
            region.RaiseDepthChanged();

            Outline outline = region.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;

                Image image = content.GetComponentInChildren<Image>();
                if (image != null)
                {
                    if(region.FrameImage != null)
                    {
                        region.FrameImage.enabled = true;
                        region.FrameImage.color = image.color;
                    }
                }

            }
        }

        public void CopyTransform(Region target, Region source)
        {
            RectTransform rt = (RectTransform)source.transform;

            if (target.MinHeight > source.MinHeight)
            {
                target.MinHeight = source.MinHeight;
            }

            if (target.MinWidth > source.MinWidth)
            {
                target.MinWidth = source.MinWidth;
            }

            SetTransform(target, rt.anchoredPosition, rt.anchorMin, rt.anchorMax, rt.sizeDelta);
        }

        public void SetTransform(Region region, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            RectTransform rt = (RectTransform)region.transform;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            OnRegionTranformChanged(region);
        }

        public void ForceUpdateLayout()
        {
            HorizontalOrVerticalLayoutGroup[] layoutGroups = transform.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            for (int i = 0; i < layoutGroups.Length; ++i)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroups[i].transform);
            }
        }
    }

}
