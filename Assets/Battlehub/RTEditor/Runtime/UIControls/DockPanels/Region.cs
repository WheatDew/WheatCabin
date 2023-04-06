using Battlehub.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public enum RegionSplitType
    {
        None,
        Left,
        Top,
        Right,
        Bottom,
    }

    [Serializable]
    public class PersistentLayoutInfo
    {
        public string WindowType;
        public bool IsVertical;
        public PersistentLayoutInfo Child0 = null;
        public PersistentLayoutInfo Child1 = null;
        public float Ratio;
        public string Args;
        public bool CanDrag = true;
        public bool CanClose = true;
        public bool CanMaximize = true;
        public bool IsHeaderVisible = true;
        public bool IsOn = true;
        public PersistentLayoutInfo[] TabGroup;
    }

    public class LayoutInfo
    {
        [SerializeField, FormerlySerializedAs("Tab")]
        private Tab m_tab;
        public Tab Tab
        {
            get { return m_tab; }
            set { m_tab = value; }
        }

        [SerializeField, FormerlySerializedAs("Content")]
        private Transform m_content;
        public Transform Content
        {
            get { return m_content; }
            set { m_content = value; }
        }

        [SerializeField, FormerlySerializedAs("IsVertical")]
        private bool m_isVertical;
        public bool IsVertical
        {
            get { return m_isVertical; }
            set { m_isVertical = value; }
        }

        [SerializeField, FormerlySerializedAs("Child0")]
        private LayoutInfo m_child0;
        public LayoutInfo Child0
        {
            get { return m_child0; }
            set { m_child0 = value; }
        }

        [SerializeField, FormerlySerializedAs("Child1")]
        private LayoutInfo m_child1;
        public LayoutInfo Child1
        {
            get { return m_child1; }
            set { m_child1 = value; }
        }

        [SerializeField, FormerlySerializedAs("Ratio")]
        private float m_ratio;
        public float Ratio
        {
            get { return m_ratio; }
            set { m_ratio = value; }
        }

        [SerializeField]
        private string m_args;
        public string Args
        {
            get { return m_args; }
            set { m_args = value; }
        }

        private bool m_canDrag = true;
        public bool CanDrag
        {
            get { return m_canDrag; }
            set
            {
                if(m_canDrag != value)
                {
                    m_canDrag = value;
                    if (Tab != null)
                    {
                        Tab.CanDrag = value;
                    }
                }
            }
        }

        private bool m_canClose = true;
        public bool CanClose
        {
            get { return m_canClose; }
            set
            {
                if(m_canClose != value)
                {
                    m_canClose = value;
                    if (Tab != null)
                    {
                        Tab.CanClose = value;
                    }
                }
            }
        }

        private bool m_canMaximize = true;
        public bool CanMaximize
        {
            get { return m_canMaximize; }
            set
            {
                if(m_canMaximize != value)
                {
                    m_canMaximize = value;
                    if (Tab != null)
                    {
                        Tab.CanMaximize = value;
                    }
                }
            }
        }

        private bool m_isOn = true;
        public bool IsOn
        {
            get { return m_isOn; }
            set
            {
                if(m_isOn != value)
                {
                    m_isOn = value;
                    if(Tab != null)
                    {
                        Tab.IsOn = value;
                    }
                }
            }
        }

        public bool IsHeaderVisible = true;
        public LayoutInfo[] TabGroup;

        public string Header;
        public Sprite Icon;
        //[Obsolete("Use LayoutInfo(Transform content, Tab tab, bool canDrag = true, bool canClose = true)")]
        public LayoutInfo(Transform content, string header = null, Sprite icon = null, bool canDrag = true, bool canClose = true)
        {
            Content = content;
            Header = header;
            Icon = icon;
            CanDrag = canDrag;
            CanClose = canClose;
        }

        public LayoutInfo(Transform content, Tab tab, bool canDrag = true, bool canClose = true, bool canMaximize = true)
        {
            Content = content;
            Tab = tab;
            IsOn = tab.IsOn;
            CanDrag = canDrag;
            CanClose = canClose;
            CanMaximize = canMaximize;
            
        }

        public LayoutInfo(Transform content, Tab tab, string args, bool canDrag = true, bool canClose = true, bool canMaximize = true)
        {
            Content = content;
            Tab = tab;
            Args = args;
            IsOn = tab.IsOn;
            CanDrag = canDrag;
            CanClose = canClose;
            CanMaximize = canMaximize;
        }

        public LayoutInfo(bool isVertical, LayoutInfo child0, LayoutInfo child1, float ratio = 0.5f)
        {
            IsVertical = isVertical;
            Child0 = child0;
            Child1 = child1;
            Ratio = ratio;
        }

        public LayoutInfo(params LayoutInfo[] tabGroup)
        {
            TabGroup = tabGroup;
            if (TabGroup != null && TabGroup.Length == 0)
            {
                TabGroup = null;
            }
        }

        public static LayoutInfo Vertical(LayoutInfo top, LayoutInfo bottom, float ratio = 0.5f)
        {
            return new LayoutInfo(true, top, bottom, ratio);
        }

        public static LayoutInfo Horizontal(LayoutInfo left, LayoutInfo right, float ratio = 0.5f)
        {
            return new LayoutInfo(false, left, right, ratio);
        }

        public static LayoutInfo Group(params LayoutInfo[] tabGroup)
        {
            return new LayoutInfo(tabGroup);
        }
    }

    public delegate void RegionEventHandler(Region region);
    public delegate void RegionEventHandler<T>(Region region, T arg);

    public class Region : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static event RegionEventHandler Created;
        public static event RegionEventHandler Destroyed;
        public static event RegionEventHandler Selected;
        public static event RegionEventHandler Unselected;
        public static event RegionEventHandler<CancelArgs> BeforeBeginDrag;
        public static event RegionEventHandler BeginDrag;
        public static event RegionEventHandler Enabled;
        public static event RegionEventHandler Disabled;
        public static event RegionEventHandler Drag;
        public static event RegionEventHandler EndDrag;
        public static event RegionEventHandler TransformChanged;
        public static event RegionEventHandler<Transform> TabActivated;
        public static event RegionEventHandler<Transform> TabDeactivated;
        public static event RegionEventHandler<Transform> TabClosed;
        public static event RegionEventHandler TabBeginDrag;
        public static event RegionEventHandler TabEndDrag;
        public static event RegionEventHandler<CancelArgs> BeforeDepthChanged;
        public static event RegionEventHandler<int> DepthChanged;
        public static event RegionEventHandler<bool> Maximized;

        [SerializeField]
        private RectTransform m_commandsPanel = null;

        [SerializeField]
        private Button m_closeButton = null;

        [SerializeField]
        private Toggle m_maximizeToggle = null;
        private bool IsMaximized
        {
            get { return m_maximizeToggle == null ? false : m_maximizeToggle.isOn; }
            set
            {
                m_maximizeToggle.isOn = value;
            }
        }

        [SerializeField]
        private bool m_canResize = true;
        public bool CanResize
        {
            get { return m_canResize; }
            set { m_canResize = value; }
        }

        [SerializeField]
        private bool m_canMaximize = true;
        public bool CanMaximize
        {
            get { return m_canMaximize; }
            set
            {
                if(m_canMaximize != value)
                {
                    m_canMaximize = value;
                    OnCanMaximizeChanged();
                }
            }
        }

        private void OnCanMaximizeChanged()
        {
            if (m_maximizeToggle != null)
            {
                if (m_canMaximize)
                {
                    if (m_maximizeToggle.gameObject.IsPrefab())
                    {
                        m_maximizeToggle = Instantiate(m_maximizeToggle, m_commandsPanel, false);
                        m_maximizeToggle.name = "MaximizeToggle";
                    }
                    m_maximizeToggle.onValueChanged.AddListener(OnMaxmizeValueChanged);
                }
                else
                {
                    m_maximizeToggle.onValueChanged.RemoveListener(OnMaxmizeValueChanged);
                    if (!m_maximizeToggle.gameObject.IsPrefab())
                    {
                        Destroy(m_maximizeToggle.gameObject);
                    }
                }
            }
        }

        public bool IsHeaderVisible
        {
            get
            {
                if (m_headerImage == null)
                {
                    return false;
                }
                return m_headerImage.gameObject.activeSelf;
            }
            set
            {
                if (m_headerImage != null)
                {
                    m_headerImage.gameObject.SetActive(value);
                }
            }
        }

        [SerializeField]
        private float m_minWidth = 95;
        public float MinWidth
        {
            get { return m_minWidth; }
            set
            {
                m_minWidth = value;
                m_layoutElement.minWidth = m_minWidth;
            }
        }

        [SerializeField]
        private float m_minHeight = 140;
        public float MinHeight
        {
            get { return m_minHeight; }
            set
            {
                m_minHeight = value;
                m_layoutElement.minHeight = m_minHeight;
            }
        }

        [SerializeField]
        private LayoutElement m_layoutElement = null;

        [SerializeField]
        private ToggleGroup m_tabPanel = null;

        [SerializeField]
        private TabPanelScroller m_tabScroller = null;

        [SerializeField]
        private RectTransform m_contentRoot = null;

        [SerializeField]
        private RectTransform m_content = null;

        [SerializeField]
        private Transform m_contentPanel = null;

        [SerializeField]
        private RectTransform m_childrenPanel = null;

        [SerializeField]
        private DockPanel m_root = null;

        [SerializeField]
        private Image m_headerImage = null;
        public Image HeaderImage
        {
            get { return m_headerImage; }
        }

        [SerializeField]
        private Image m_frameImage = null;
        public Image FrameImage
        {
            get { return m_frameImage; }
        }

        public DockPanel Root
        {
            get { return m_root; }
        }

        private Tab m_prevTab;
        private Tab m_activeTab;
        public int ActiveTabIndex
        {
            get
            {
                if (m_activeTab != null)
                {
                    return m_activeTab.Index;
                }
                return -1;
            }
        }

        private LayoutGroup m_layoutGroup;
        public LayoutGroup LayoutGroup
        {
            get { return m_layoutGroup; }
        }

        public Transform ActiveContent
        {
            get
            {
                int activeTabIndex = ActiveTabIndex;

                if(activeTabIndex < 0)
                {
                    activeTabIndex = 0;
                }

                if (activeTabIndex >= 0 && activeTabIndex < m_contentPanel.childCount)
                {
                    return m_contentPanel.GetChild(activeTabIndex);
                }
                
                return null;
            }
        }

        public RectTransform ContentRoot
        {
            get { return m_contentRoot; }
        }

        public Transform Content
        {
            get { return m_content; }
        }

        public Transform ContentPanel
        {
            get { return m_contentPanel; }
        }

        public Transform ChildrenPanel
        {
            get { return m_childrenPanel; }
        }

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    if (m_isSelected)
                    {
                        if (Selected != null)
                        {
                            Selected(this);
                        }
                    }
                    else
                    {
                        if (Unselected != null)
                        {
                            Unselected(this);
                        }
                    }
                }
            }
        }

        public bool HasChildren
        {
            get { return ChildrenPanel != null && ChildrenPanel.childCount > 0; }
        }

        private bool m_isDraggingTab;
        private bool m_isDraggingOutside;
        private RectTransform m_pointerOverTab;
        private Region m_pointerOverRegion;
        private Region m_beginDragRegion;
        private Transform m_dragContent;
        private bool m_isFree = false;
        private RegionSplitType m_splitType;

        [SerializeField]
        private bool m_forceRebuildLayoutImmediate = true;

        private bool m_isDestroyed = false;

        protected virtual void Awake()
        {
            if (m_root == null)
            {
                m_root = GetComponentInParent<DockPanel>();
            }

            if (m_layoutElement != null)
            {
                m_layoutElement.minWidth = m_minWidth;
                m_layoutElement.minHeight = m_minHeight;
            }

            if (Created != null)
            {
                Created(this);
            }
        }

        protected virtual void Start()
        {
            OnCanMaximizeChanged();
           
            if (m_closeButton != null)
            {
                if (m_closeButton.gameObject.IsPrefab())
                {
                    m_closeButton = Instantiate(m_closeButton, m_commandsPanel, false);
                    m_closeButton.name = "CloseButton";
                }
                m_closeButton.onClick.AddListener(OnClose);
            }

            UpdateVisualState();
        }

        protected virtual void OnEnable()
        {
            if (Enabled != null)
            {
                Enabled(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (Disabled != null)
            {
                Disabled(this);
            }
        }

        protected virtual void OnDestroy()
        {
            m_isDestroyed = true;

            m_prevTab = null;

            if (transform.parent != null)
            {
                Region parent = transform.parent.GetComponentInParent<Region>();
                if (parent != null && parent.m_root == m_root)
                {
                    parent.DestroyChildRegion(transform.GetSiblingIndex());
                    parent.UpdateVisualState(1);

                    parent.CanResize = CanResize;
                    parent.IsHeaderVisible = IsHeaderVisible;
                    
                    parent.RaiseDepthChanged();

                    UpdateResizers();
                }
            }

            if (m_root != null)
            {
                m_root.CursorHelper.ResetCursor(this);
            }

            IsSelected = false;

            if (Destroyed != null)
            {
                Destroyed(this);
            }

            if (m_maximizeToggle != null)
            {
                m_maximizeToggle.onValueChanged.RemoveListener(OnMaxmizeValueChanged);
                if (!m_maximizeToggle.gameObject.IsPrefab())
                {
                    Destroy(m_maximizeToggle.gameObject);
                }
            }

            if (m_closeButton != null)
            {
                m_closeButton.onClick.RemoveListener(OnClose);
                if (!m_closeButton.gameObject.IsPrefab())
                {
                    Destroy(m_closeButton.gameObject);
                }
            }
        }

        private static void Subscribe(Tab tab, Region region)
        {
            tab.Toggle += region.OnTabToggle;
            tab.PointerDown += region.OnTabPointerDown;
            tab.Closed += region.OnTabClose;
            tab.InitializePotentialDrag += region.OnTabInitializePotentialDrag;
            tab.BeginDrag += region.OnTabBeginDrag;
            tab.Drag += region.OnTabDrag;
            tab.EndDrag += region.OnTabEndDrag;

            if (tab.IsOn)
            {
                region.m_prevTab = region.m_activeTab;
                region.m_activeTab = tab;
            }
        }

        private static void Unsubscribe(Tab tab, Region region)
        {
            tab.Toggle -= region.OnTabToggle;
            tab.PointerDown -= region.OnTabPointerDown;
            tab.Closed -= region.OnTabClose;
            tab.InitializePotentialDrag -= region.OnTabInitializePotentialDrag;
            tab.BeginDrag -= region.OnTabBeginDrag;
            tab.Drag -= region.OnTabDrag;
            tab.EndDrag -= region.OnTabEndDrag;

            if (tab == region.m_activeTab)
            {
                region.m_activeTab = null;
            }
        }

        public Region GetChild(int index)
        {
            return m_childrenPanel.GetChild(index).GetComponent<Region>();
        }

        private bool m_closing = false;
        public void CloseAllTabs()
        {
            m_closing = true;

            // Pass "true" to make sure to also cleanup "hidden" tabs in case the "IsHeaderVisible" option was set to false.
            // Fix for issue https://github.com/Battlehub0x/Unity_RuntimeEditor/issues/9, see details there.
            Tab[] tabs = GetComponentsInChildren<Tab>(true);
            for (int i = 0; i < tabs.Length; i++)
            {
                Tab tab = tabs[i];
                tab.Close();
            }

            Region[] regions = GetComponentsInChildren<Region>();
            for (int i = 0; i < regions.Length; ++i)
            {
                Region region = regions[i];
                if (region == this) 
                {
                    continue;
                }

                Destroy(region.gameObject);
            }

            m_closing = false;
        }

        public void Destroy()
        {
            CloseAllTabs();
            Destroy(gameObject);
        }

        public void Build(LayoutInfo layout)
        {
            CloseAllTabs();
            Build(layout, this);
            RaiseDepthChanged();

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(CoScrollToLeft());
            }
            else
            {
                ScrollTabHeaderToLeft();
            }
        }

        private IEnumerator CoScrollToLeft()
        {
            yield return new WaitForEndOfFrame();
            ScrollTabHeaderToLeft();
        }

        private void Build(LayoutInfo layout, Region region)
        {
            region.IsHeaderVisible = layout.IsHeaderVisible;

            if (layout.TabGroup != null)
            {
                for (int i = 0; i < layout.TabGroup.Length; ++i)
                {
                    LayoutInfo tabLayout = layout.TabGroup[i];
                    if (tabLayout.Tab == null)
                    {
                        //For backward compatibility
                        tabLayout.Tab = CreateTab(tabLayout.Icon, tabLayout.Header, tabLayout.CanDrag, tabLayout.CanClose, tabLayout.CanMaximize);
                    }
                    region.Add(tabLayout.Tab, tabLayout.Content, false, RegionSplitType.None, 0.3f);
                    ((RectTransform)tabLayout.Content).Stretch();
                }

                LayoutInfo onLayoutInfo = layout.TabGroup.Where(t => t.IsOn).FirstOrDefault();
                if (onLayoutInfo != null)
                {
                    Tab tab = FindTab(onLayoutInfo.Content);
                    if (tab != null)
                    {
                        tab.IsOn = true;
                    }
                }
            }
            else if (layout.Child0 != null && layout.Child1 != null)
            {
                if (layout.IsVertical)
                {
                    Rect rect = region.m_childrenPanel.rect;
                    Region region0 = CreateVerticalRegion(region, layout.Ratio);
                    Region region1 = CreateVerticalRegion(region, 1 - layout.Ratio);

                    CreateVerticalLayoutGroup(region);

                    Build(layout.Child0, region0);
                    Build(layout.Child1, region1);
                }
                else
                {
                    Rect rect = region.m_childrenPanel.rect;
                    Region region0 = CreateHorizontalRegion(region, layout.Ratio);
                    Region region1 = CreateHorizontalRegion(region, 1 - layout.Ratio);

                    CreateHorizontalLayoutGroup(region);

                    Build(layout.Child0, region0);
                    Build(layout.Child1, region1);
                }
            }
            else
            {
                if (layout.Tab == null)
                {
                    //For backward compatibility
                    layout.Tab = CreateTab(layout.Icon, layout.Header, layout.CanDrag, layout.CanClose, layout.CanMaximize);
                }
                region.Add(layout.Tab, layout.Content, false, RegionSplitType.None, 0.3f);
                ((RectTransform)layout.Content).Stretch();
            }

            region.UpdateVisualState();
        }

        public bool Validate(LayoutInfo layoutInfo)
        {
            if (layoutInfo.Child0 == null && layoutInfo.Child1 == null && layoutInfo.Content == null)
            {
                if (layoutInfo.TabGroup == null || layoutInfo.TabGroup.Length == 0)
                {
                    //Allow empty layout;
                    return true;
                }

                for (int i = 0; i < layoutInfo.TabGroup.Length; ++i)
                {
                    LayoutInfo tabGroupLayoutInfo = layoutInfo.TabGroup[i];
                    if (tabGroupLayoutInfo != null)
                    {
                        if (!Validate(tabGroupLayoutInfo))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (layoutInfo.Child0 != null && layoutInfo.Child1 != null && (!Validate(layoutInfo.Child0) || !Validate(layoutInfo.Child1)))
            {
                return false;
            }

            return true;
        }

        private Region CreateVerticalRegion(Region region, float ratio)
        {
            Region childRegion = Instantiate(m_root.RegionPrefab, region.m_childrenPanel, false);
            childRegion.name = "Region " + m_root.RegionId++;
            childRegion.m_layoutElement.preferredHeight = -1;
            childRegion.m_layoutElement.preferredWidth = -1;
            childRegion.m_layoutElement.flexibleHeight = Mathf.Clamp01(ratio);
            childRegion.m_layoutElement.flexibleWidth = -1;
            return childRegion;
        }

        private Region CreateHorizontalRegion(Region region, float ratio)
        {
            Region childRegion = Instantiate(m_root.RegionPrefab, region.m_childrenPanel, false);
            childRegion.name = "Region " + m_root.RegionId++;
            childRegion.m_layoutElement.preferredWidth = -1;
            childRegion.m_layoutElement.preferredHeight = -1;
            childRegion.m_layoutElement.flexibleWidth = Mathf.Clamp01(ratio);
            childRegion.m_layoutElement.flexibleHeight = -1;
            return childRegion;
        }

        public Transform GetDragRegion()
        {
            Transform parent = transform;

            while (parent != null)
            {
                if (parent.parent == m_root.Free || parent.parent == m_root.Modal)
                {
                    if (parent.parent != null)
                    {
                        return parent;
                    }
                }

                parent = parent.parent;
            }
            return null;
        }

        public bool IsModal()
        {
            if (transform.parent != null)
            {
                if (transform.parent.GetComponentInParent<Region>() != null)
                {
                    return false;
                }
            }

            Transform parent = transform;
            while (parent != null)
            {
                if (parent.parent == m_root.Modal)
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        public bool IsFreeOrModal()
        {
            if (transform.parent != null)
            {
                if (transform.parent.GetComponentInParent<Region>() != null)
                {
                    return false;
                }
            }

            Transform parent = transform;
            while (parent != null)
            {
                if (parent.parent == m_root.Free || parent.parent == m_root.Modal)
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private bool m_maxmimizing = false;
        public void Maximize(bool maximize)
        {
            if (GetDragRegion() != null)
            {
                return;
            }

            if (m_root.Free == null || m_root.Modal == null)
            {
                return;
            }

            if (m_maxmimizing)
            {
                return;
            }

            m_maxmimizing = true;

            if(m_maxmimizing)
            {
                if(m_layoutElement.flexibleWidth < 0)
                {
                    m_layoutElement.flexibleWidth = 0;
                }

                if(m_layoutElement.flexibleHeight < 0)
                {
                    m_layoutElement.flexibleHeight = 0;
                }
            }

            HashSet<Region> regionAndAncestors = new HashSet<Region>(GetComponentsInParent<Region>(true));
            HashSet<Region> regionAndChildren = new HashSet<Region>(GetComponentsInChildren<Region>(true));

            Region[] children = m_root.RootRegion.GetComponentsInChildren<Region>(true);
            for (int i = 0; i < children.Length; ++i)
            {
                if (maximize)
                {
                    if (regionAndChildren.Contains(children[i]) && children[i] != this)
                    {
                        continue;
                    }

                    if (!regionAndAncestors.Contains(children[i]))
                    {
                        children[i].gameObject.SetActive(false);
                        children[i].IsMaximized = false;
                    }
                    else
                    {
                        children[i].gameObject.SetActive(true);
                        children[i].IsMaximized = true;
                        if (Mathf.Approximately(children[i].m_layoutElement.flexibleWidth, 0))
                        {
                            children[i].m_layoutElement.flexibleWidth = 0.001f;
                        }

                        if (Mathf.Approximately(children[i].m_layoutElement.flexibleHeight, 0))
                        {
                            children[i].m_layoutElement.flexibleHeight = 0.001f;
                        }
                    }
                }
                else
                {
                    children[i].gameObject.SetActive(true);
                    children[i].IsMaximized = false;
                }
            }
            ForceUpdateLayoutImmediate(m_root.transform);
            UpdateResizers();
            if (Maximized != null)
            {
                Maximized(this, maximize);
            }
            m_maxmimizing = false;
        }

        public void Fit(bool resize = true)
        {
            if (m_root.Free == null || m_root.Modal == null)
            {
                return;
            }

            Resizer[] resizers = GetComponentsInChildren<Resizer>();
            float resizerSize = resizers.Select(r => (RectTransform)r.transform).Max(r => Mathf.Min(r.rect.width, r.rect.height));

            RectTransform rt = ((RectTransform)transform);
            float minWidth = resize ? m_layoutElement.minWidth + resizerSize : rt.rect.width;
            float minHeight = resize ? m_layoutElement.minHeight + resizerSize : rt.rect.height;

            Rect rect = rt.rect;
            if (rect.width < minWidth)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minWidth);
            }
            if (rect.height < minHeight)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
            }

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            RectTransform parentPanel = (RectTransform)GetDragRegion().parent;
            for (int i = 0; i < corners.Length; ++i)
            {
                Vector3 corner = parentPanel.InverseTransformPoint(corners[i]);
                if (corner.x < parentPanel.rect.xMin)
                {
                    corner.x = parentPanel.rect.xMin;
                }

                if ((i == 0 || i == 1) && corner.x + minWidth > parentPanel.rect.xMax)
                {
                    corner.x = parentPanel.rect.xMax - minWidth;
                }

                if (corner.x > parentPanel.rect.xMax)
                {
                    corner.x = parentPanel.rect.xMax;
                }

                if ((i == 2 || i == 3) && corner.x - minWidth < parentPanel.rect.xMin)
                {
                    corner.x = parentPanel.rect.xMin + minWidth;
                }

                if (corner.y < parentPanel.rect.yMin)
                {
                    corner.y = parentPanel.rect.yMin;
                }

                if ((i == 0 || i == 3) && corner.y + minHeight > parentPanel.rect.yMax)
                {
                    corner.y = parentPanel.rect.yMax - minHeight;
                }

                if (corner.y > parentPanel.rect.yMax)
                {
                    corner.y = parentPanel.rect.yMax;
                }

                if ((i == 1 || i == 2) && corner.y - minHeight < parentPanel.rect.yMin)
                {
                    corner.y = parentPanel.rect.yMin + minHeight;
                }

                corner = parentPanel.TransformPoint(corner);
                corners[i] = corner;
            }

            Vector3 position = (corners[0] + corners[2]) * 0.5f;
            for (int i = 0; i < corners.Length; ++i)
            {
                Vector3 corner = rt.InverseTransformPoint(corners[i]);
                corners[i] = corner;
            }

            rt.offsetMin = corners[0];
            rt.offsetMax = corners[2];

            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.position = position;

            if (TransformChanged != null)
            {
                TransformChanged(this);
            }
        }

        public void ScrollTabHeaderToRight()
        {
            if (m_tabScroller != null)
            {
                m_tabScroller.ScrollToRight();
            }
        }

        public void ScrollTabHeaderToLeft()
        {
            if (m_tabScroller != null)
            {
                m_tabScroller.ScrollToLeft();
            }
        }

        public bool CanAdd(bool isFree = false)
        {
            return !(m_childrenPanel.childCount > 0 && !isFree);
        }

        private Tab CreateTab(Sprite icon, string header, bool canDrag, bool canClose, bool canMaximize)
        {
            Tab tab = Instantiate(Root.TabPrefab);
            tab.CanDrag = canDrag;
            tab.CanClose = canClose;
            tab.CanMaximize = canMaximize;
            tab.name = "Tab " + header;
            tab.Icon = icon;
            tab.Text = header;
            return tab;
        }

        public void Add(Sprite icon, string header, Transform content, bool isFree = false, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f, bool canDrag = true, bool canClose = true, bool canMaximize = true)
        {
            Tab tab = CreateTab(icon, header, canDrag, canClose, canMaximize);
            Add(tab, content, isFree, splitType, flexibleSize);
        }

        public void Add(Tab tab, Transform content, bool isFree = false, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f)
        {
            if (m_childrenPanel.childCount > 0 && !isFree && splitType == RegionSplitType.None)
            {
                throw new InvalidOperationException("Unable to Add content. Region has children and is not a \"leaf\" region.");
            }

            if (isFree)
            {
                RectTransform rootRegionRT = (RectTransform)m_root.Free.transform;
                CreateFreeRegion(tab, content, rootRegionRT.rect.size * 0.5f, Vector2.one * 0.5f, rootRegionRT.transform.position);
            }
            else
            {
                if (this == m_root.RootRegion && m_tabPanel.transform.childCount == 0 && !m_root.RootRegion.HasChildren)
                {
                    splitType = RegionSplitType.None;
                }

                Insert(m_tabPanel.transform.childCount, tab, content, splitType, flexibleSize);
                ((RectTransform)content).Stretch();

                if (this == m_root.RootRegion)
                {
                    CanResize = true;
                }
            }

            UpdateResizers();
            UpdateVisualState();
        }

        public void RemoveAt(int index)
        {
            RemoveAt(index, true);
        }
      
        private void RemoveAt(int index, bool destroyRegionIfPossible)
        {
            if (index < 0 || m_tabPanel.transform.childCount <= index)
            {
                return;
            }

            Tab tab = m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
            if (m_tabPanel.transform.childCount > 1 && index == ActiveTabIndex)
            {
                if (m_prevTab != null && m_prevTab.transform.parent == m_tabPanel.transform)
                {
                    m_prevTab.IsOn = true;
                }
                else 
                {
                    Tab nextTab;
                    if (index < m_tabPanel.transform.childCount - 1)
                    {
                        nextTab = m_tabPanel.transform.GetChild(index + 1).GetComponent<Tab>();
                    }
                    else
                    {
                        nextTab = m_tabPanel.transform.GetChild(index - 1).GetComponent<Tab>();
                    }
                    nextTab.IsOn = true;
                }
            }

            tab.OnClosing();
            Unsubscribe(tab, this);

            Transform content = m_contentPanel.transform.GetChild(index);

            //ViewRoot.GetWorldCorners(corners) works wrong if this event raised before Destroy()
            //if (TabClosed != null)
            //{
            //    TabClosed(this, content);
            //}
            // If there will be no more tabs, then also destroy the region 
            if (destroyRegionIfPossible && m_tabPanel.transform.childCount == 1 && this != m_root.RootRegion)
            {
                Destroy(gameObject);
            }
            //else
            {
                Destroy(tab.gameObject);
                Destroy(content.gameObject);
            }


            if (TabClosed != null)
            {
                TabClosed(this, content);
            }


            UpdateResizers();
            UpdateVisualState();

            m_root.CursorHelper.ResetCursor(this);
        }
       

        public void Move(int index, int targetIndex, Region targetRegion, RegionSplitType targetSplitType = RegionSplitType.None)
        {
            if (m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Remove content. Region has children and is not a \"leaf\" region.");
            }

            if (index < 0 || m_tabPanel.transform.childCount <= index)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Tab tab = m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
            Transform content = m_contentPanel.transform.GetChild(index);
            Move(tab, content, targetIndex, targetRegion, targetSplitType);
            UpdateResizers();
            UpdateVisualState();
        }

        private void Move(Tab tab, Transform content, int targetIndex, Region targetRegion, RegionSplitType targetSplitType = RegionSplitType.None)
        {
            if (m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Remove content. Region has children and is not a \"leaf\" region.");
            }

            Debug.Assert(content.parent == m_contentPanel);

            Unsubscribe(tab, this);

            bool destroy = m_contentPanel.childCount == 1 && targetRegion != this && m_root.RootRegion != this;

            targetRegion.Insert(targetIndex, tab, content, targetSplitType);
            targetRegion.RaiseDepthChanged();
            RaiseDepthChanged();

            if (destroy)
            {
                Destroy(gameObject);
            }
        }

        private void DestroyChildRegion(int index)
        {
            if (m_childrenPanel == null)
            {
                return;
            }

            if (m_childrenPanel.childCount == 2)
            {
                index = (index + 1) % 2;
                Region childRegion = m_childrenPanel.GetChild(index).GetComponent<Region>();
                if (childRegion != null && childRegion.m_contentPanel != null && childRegion.m_tabPanel != null && childRegion.m_tabPanel.transform != null)
                {
                    if (childRegion.m_contentPanel.childCount == 0)
                    {
                        if(!childRegion.m_isDestroyed)
                        {
                            childRegion.MoveChildrenToParentRegion(this);
                        }
                    }
                    else
                    {
                        if (!childRegion.m_isDestroyed)
                        {
                            childRegion.MoveContentsToRegion(this);
                        }

                        HorizontalOrVerticalLayoutGroup layoutGroup = m_childrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
                        if (layoutGroup != null)
                        {
                            Destroy(layoutGroup);
                        }
                        m_layoutGroup = null;
                    }

                    Destroy(childRegion.gameObject);
                }
            }
        }

        private void Insert(int index, Tab tab, Transform content, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f)
        {
            switch (splitType)
            {
                case RegionSplitType.None:
                    if (m_childrenPanel.childCount > 0)
                    {
                        throw new InvalidOperationException("Unable to Add content. Region has children and is not a \"leaf\" region.");
                    }
                    Insert(index, tab, content);
                    break;
                case RegionSplitType.Left:
                    SplitLeft(tab, content, flexibleSize);
                    break;
                case RegionSplitType.Top:
                    SplitTop(tab, content, flexibleSize);
                    break;
                case RegionSplitType.Right:
                    SplitRight(tab, content, flexibleSize);
                    break;
                case RegionSplitType.Bottom:
                    SplitBottom(tab, content, flexibleSize);
                    break;
            }
        }

        private void Insert(int index, Tab tab, Transform content)
        {
            content.SetParent(m_contentPanel, false);
            content.SetSiblingIndex(index);

            tab.transform.SetParent(m_tabPanel.transform, false);
            tab.transform.SetSiblingIndex(index);
            tab.ToggleGroup = m_tabPanel;

            Subscribe(tab, this);

            // Make sure we turn on the new tab *first* or else Unity's UI.Toggle.Set() won't let us turn off the tab that was previously active (when TabGroup.allowSwitchOff == false).
            // Otherwise, that tab's On/Off state callback reports that it is still On because TabGroup won't let it turn off.
            // Also, do we even need to explicitly turn off all the other tabs in the group below? Don't they get turned off automatically when the new tab is turned on?
            tab.IsOn = true;
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            //for (int i = 0; i < tabs.Length; ++i)
            //{
            //    if (tabs[i] != tab)
            //    {
            //        tabs[i].IsOn = false;
            //    }
            //}

            IsSelected = true;
            CanMaximize = tabs.All(t => t.CanMaximize);
        }

        private void SplitTop(Tab tab, Transform content, float flexibleSize)
        {
            tab.transform.SetParent(m_root.transform, false);
            content.transform.SetParent(m_root.transform, false);

            MoveContentsToChildRegion();
            Region region = CreateVerticalRegion(tab, content, flexibleSize);

            CreateVerticalLayoutGroup(this);

            region.transform.SetSiblingIndex(0);

            ((RectTransform)content).Stretch();
        }

        private void SplitBottom(Tab tab, Transform content, float flexibleSize)
        {
            tab.transform.SetParent(m_root.transform, false);
            content.transform.SetParent(m_root.transform, false);

            MoveContentsToChildRegion();

            Region region = CreateVerticalRegion(tab, content, flexibleSize);

            CreateVerticalLayoutGroup(this);

            region.transform.SetSiblingIndex(1);

            ((RectTransform)content).Stretch();
        }

        private void SplitLeft(Tab tab, Transform content, float flexibleSize)
        {
            tab.transform.SetParent(m_root.transform, false);
            content.transform.SetParent(m_root.transform, false);

            MoveContentsToChildRegion();

            Region region = CreateHorizontalRegion(tab, content, flexibleSize);

            CreateHorizontalLayoutGroup(this);

            region.transform.SetSiblingIndex(0);

            ((RectTransform)content).Stretch();
        }

        private void SplitRight(Tab tab, Transform content, float flexibleSize)
        {
            tab.transform.SetParent(m_root.transform, false);
            content.transform.SetParent(m_root.transform, false);

            MoveContentsToChildRegion();

            Region region = CreateHorizontalRegion(tab, content, flexibleSize);

            CreateHorizontalLayoutGroup(this);

            region.transform.SetSiblingIndex(1);

            ((RectTransform)content).Stretch();
        }

        private static void CreateVerticalLayoutGroup(Region region)
        {
            HorizontalLayoutGroup horizontalLg = region.m_childrenPanel.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLg != null)
            {
                DestroyImmediate(horizontalLg);
            }

            VerticalLayoutGroup lg = region.m_childrenPanel.GetComponent<VerticalLayoutGroup>();
            if (lg == null)
            {
                lg = region.m_childrenPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = false;
            lg.childForceExpandWidth = true;
            lg.spacing = -1;

            region.m_layoutGroup = lg;
        }

        private static void CreateHorizontalLayoutGroup(Region region)
        {
            VerticalLayoutGroup verticalLg = region.m_childrenPanel.GetComponent<VerticalLayoutGroup>();
            if (verticalLg != null)
            {
                DestroyImmediate(verticalLg);
            }

            HorizontalLayoutGroup lg = region.m_childrenPanel.GetComponent<HorizontalLayoutGroup>();
            if (lg == null)
            {
                lg = region.m_childrenPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = true;
            lg.childForceExpandWidth = false;
            lg.spacing = -1;

            region.m_layoutGroup = lg;
        }

        private Region CreateVerticalRegion(Tab tab, Transform content, float flexibleHeight)
        {
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_root.RegionId++;

            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.flexibleHeight = flexibleHeight;
            region.m_layoutElement.flexibleWidth = -1;
            region.Insert(0, tab, content);
            return region;
        }

        private Region CreateHorizontalRegion(Tab tab, Transform content, float flexibleWidth)
        {
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_root.RegionId++;

            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = flexibleWidth;
            region.m_layoutElement.flexibleHeight = -1;
            region.Insert(0, tab, content);
            return region;
        }

        private void MoveContentsToChildRegion()
        {
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_root.RegionId++;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = 0.7f;
            region.m_layoutElement.flexibleHeight = 0.7f;

            if (m_contentPanel.childCount == 0)
            {
                if (m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
                {
                    CreateHorizontalLayoutGroup(region);
                }
                else
                {
                    CreateVerticalLayoutGroup(region);
                }

                MoveChildrenToRegion(region);
            }
            else
            {
                Tab[] tabs = MoveContentsToRegion(region);
                region.IsHeaderVisible = IsHeaderVisible;
                region.CanResize = CanResize;
                region.CanMaximize = tabs.All(t => t.CanMaximize);
                ActivateBestTab(tabs, true);
            }
        }

        private void MoveChildrenToParentRegion(Region parentRegion)
        {
            MoveChildrenToRegion(parentRegion);

            bool isHorizontalLayout = false;
            if (m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
            {
                isHorizontalLayout = true;
            }

            bool isParentHorizontalLayout = false;
            if (parentRegion.m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
            {
                isParentHorizontalLayout = true;
            }

            if (isHorizontalLayout != isParentHorizontalLayout)
            {
                HorizontalOrVerticalLayoutGroup layoutGroup = parentRegion.m_childrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
                if (layoutGroup != null)
                {
                    DestroyImmediate(layoutGroup);
                }

                if (isHorizontalLayout)
                {
                    CreateHorizontalLayoutGroup(parentRegion);
                }
                else
                {
                    CreateVerticalLayoutGroup(parentRegion);
                }
            }
        }

        private void MoveChildrenToRegion(Region region)
        {
            List<Transform> childrenList = new List<Transform>();
            for (int i = m_childrenPanel.childCount - 1; i >= 0; i--)
            {
                Transform child = m_childrenPanel.GetChild(i);
                childrenList.Add(child);
            }

            for (int i = childrenList.Count - 1; i >= 0; i--)
            {
                childrenList[i].SetParent(region.m_childrenPanel, false);
            }
        }

        private Tab[] MoveContentsToRegion(Region region)
        {
            Transform[] contents = m_contentPanel.OfType<Transform>().ToArray();
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            for (int i = 0; i < tabs.Length; ++i)
            {
                Tab tab = tabs[i];
                Unsubscribe(tab, this);

                tab.transform.SetParent(region.m_tabPanel.transform, false);
                contents[i].transform.SetParent(region.m_contentPanel, false);

                Subscribe(tab, region);

                tab.ToggleGroup = region.m_tabPanel;
            }

            return tabs;
        }

        private void OnTabToggle(Tab tab, bool isOn)
        {
            Transform content = m_contentPanel.GetChild(tab.Index);
            content.gameObject.SetActive(isOn);
            m_prevTab = m_activeTab;
            m_activeTab = tab;

            if (isOn)
            {
                if (TabActivated != null)
                {
                    TabActivated(this, content);
                }
            }
            else
            {
                if (TabDeactivated != null)
                {
                    TabDeactivated(this, content);
                }
            }
        }

        private void OnTabPointerDown(Tab tab, PointerEventData args)
        {
            IsSelected = true;
        }

        private void OnTabClose(Tab sender)
        {
            if (IsMaximized)
            {
                Maximize(false);
            }

            RemoveAt(sender.Index, !m_closing);
        }

        private void OnTabInitializePotentialDrag(Tab tab, PointerEventData args)
        {
            if (m_root.Modal != null && m_root.Modal.childCount > 0 && transform.parent != m_root.Modal || IsMaximized || m_tabPanel == null)
            {
                m_isDraggingTab = false;
                return;
            }

            m_isDraggingTab = true;
        }

        private void SetRaycastTargets(bool value)
        {
            Region[] regions = m_root.GetComponentsInChildren<Region>();
            for (int i = 0; i < regions.Length; ++i)
            {
                if (regions[i].m_frameImage != null)
                {
                    regions[i].m_frameImage.raycastTarget = value;
                }

            }
        }

        private void OnTabBeginDrag(Tab tab, PointerEventData args)
        {
            if (!m_isDraggingTab)
            {
                return;
            }

            m_root.CursorHelper.SetCursor(this, null);

            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;
            m_beginDragRegion = m_pointerOverRegion = tab.GetComponentInParent<Region>();
            m_dragContent = m_contentPanel.GetChild(tab.Index);

            SetRaycastTargets(true);
            BeginDragInsideOfTabPanel(this, tab, args);

            if (TabBeginDrag != null)
            {
                TabBeginDrag(this);
            }
        }

        private void OnTabDrag(Tab tab, PointerEventData args)
        {
            if (!m_isDraggingTab)
            {
                return;
            }

            Region region = GetTargetRegion(args);
            bool isRegionChanged = false;
            if (region != m_pointerOverRegion)
            {
                isRegionChanged = true;
                m_pointerOverRegion = region;
                if (region.m_tabPanel.gameObject.activeInHierarchy && region.m_childrenPanel.childCount == 0)
                {
                    tab.transform.SetParent(region.m_tabPanel.transform, false);
                }
            }

            Vector2 localPoint;
            RectTransform tabPanelRT = (RectTransform)region.m_tabPanel.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tabPanelRT, args.position, args.pressEventCamera, out localPoint))
            {
                Rect tabPanelRect = tabPanelRT.rect;
                //tabPanelRect.yMax *= 2.0f;
                //tabPanelRect.yMin *= 2.0f;
                if (tabPanelRect.Contains(localPoint) || region == m_root.RootRegion && region.m_contentPanel.childCount == 0 && region.m_childrenPanel.childCount == 0)
                {
                    if (m_isDraggingOutside || isRegionChanged)
                    {
                        m_isDraggingOutside = false;
                        BeginDragInsideOfTabPanel(region, tab, args);
                    }

                    DragInsideOfTabPanel(region, tab, args, localPoint, tabPanelRT);
                }
                else
                {
                    if (!m_isDraggingOutside)
                    {
                        m_isDraggingOutside = true;
                        BeginDragOutsideOfTabPanel(tab, args);
                        SetMaxTabSiblingIndex(tab);
                    }


                    DragOutsideOfTabPanel(region, tab, args, isRegionChanged);
                }
            }
            else
            {
                if (!m_isDraggingOutside)
                {
                    m_isDraggingOutside = true;
                    BeginDragOutsideOfTabPanel(tab, args);
                    SetMaxTabSiblingIndex(tab);
                }

                DragOutsideOfTabPanel(region, tab, args, isRegionChanged);
            }
        }

        private void BeginDragInsideOfTabPanel(Region region, Tab tab, PointerEventData args)
        {
            if (region.m_tabPanel.gameObject.activeInHierarchy)
            {
                tab.transform.SetParent(region.m_tabPanel.transform, false);
            }
        }

        private void DragInsideOfTabPanel(Region region, Tab tab, PointerEventData args, Vector2 localPoint, RectTransform tabPanelRT)
        {
            localPoint.y = 0;
            tab.PreviewPosition = tabPanelRT.TransformPoint(localPoint);

            RectTransform tabTransform = (RectTransform)tab.transform;
            tab.PreviewContentSize = tabTransform.rect.size;

            SetTabSiblingIndex(region, tab, args, localPoint);

            m_isFree = false;
            m_splitType = RegionSplitType.None;
            tab.IsPreviewContentActive = false;
        }

        private void BeginDragOutsideOfTabPanel(Tab tab, PointerEventData args)
        {

        }

        private void DragOutsideOfTabPanel(Region region, Tab tab, PointerEventData args, bool isRegionChanged)
        {
            RectTransform contentRT = region.m_content;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRT, args.position, args.pressEventCamera, out localPoint))
            {
                RegionSplitType splitType = RegionSplitType.None;
                bool isFree = false;

                float w = contentRT.rect.width;
                float h = contentRT.rect.height;

                localPoint.y = -localPoint.y;

                if (w / 3 <= localPoint.x && localPoint.x <= 2 * w / 3 &&
                    h / 3 <= localPoint.y && localPoint.y <= 2 * h / 3 ||
                    m_beginDragRegion == region && m_beginDragRegion.m_contentPanel.transform.childCount == 1)
                {
                    Vector3 worldPoint;
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_root.Preview, args.position, args.pressEventCamera, out worldPoint))
                    {
                        //floating window 
                        tab.PreviewPosition = worldPoint;
                        tab.IsPreviewContentActive = true;

                        isFree = true;
                        splitType = RegionSplitType.None;
                    }
                }
                else
                {
                    isFree = false;

                    float x = localPoint.x;
                    float y = (w / h) * localPoint.y;
                    float wy = w - y;

                    if (x > y && x > wy)
                    {
                        splitType = RegionSplitType.Right;
                    }
                    else if (x < y && x < wy)
                    {
                        splitType = RegionSplitType.Left;
                    }
                    else if (x < y && x > wy)
                    {
                        splitType = RegionSplitType.Bottom;
                    }
                    else
                    {
                        splitType = RegionSplitType.Top;
                    }
                }

                if (m_isFree != isFree || m_splitType != splitType || isRegionChanged)
                {
                    tab.IsPreviewContentActive = true;
                    m_isFree = isFree;
                    m_splitType = splitType;

                    if (m_isFree)
                    {
                        tab.PreviewContentSize = new Vector2(w / 3, h / 3);
                    }
                    else
                    {
                        RectTransform tabTransform = (RectTransform)tab.transform;
                        Vector2 pivot =
                            (splitType == RegionSplitType.Left || splitType == RegionSplitType.Right) ?
                                Vector2.Scale(new Vector2(Mathf.Min(w / 3, tabTransform.rect.width), -tabTransform.rect.height), tabTransform.pivot) :
                                Vector2.Scale(new Vector2(Mathf.Min(w, tabTransform.rect.width), -tabTransform.rect.height), tabTransform.pivot);
                        switch (splitType)
                        {
                            case RegionSplitType.Top:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot);
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Bottom:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot - new Vector2(0, 2 * h / 3));
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Left:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot + new Vector2(0, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                            case RegionSplitType.Right:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot + new Vector2(2 * w / 3, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                        }
                    }
                }
            }
        }

        private void OnTabEndDrag(Tab tab, PointerEventData args)
        {
            m_root.CursorHelper.ResetCursor(this);

            if (!m_isDraggingTab)
            {
                return;
            }

            Region region = GetTargetRegion(args);
            tab.transform.SetParent(region.m_tabPanel.transform, false);

            m_isDraggingTab = false;

            if (m_isFree)
            {
                Region freeRegion = Instantiate(m_root.RegionPrefab, m_root.Free);
                freeRegion.name = "Region " + m_root.RegionId++;

                RectTransform rt = (RectTransform)freeRegion.transform;
                RectTransform beginRt = (RectTransform)m_beginDragRegion.transform;
                Vector2 size = beginRt.rect.size;
                Vector2 endSz = tab.RectSize;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2((endSz.x * 0.5f) / size.x, 1 - (endSz.y * 0.5f) / size.y);
                rt.sizeDelta = size;

                Vector3 worldPos = Vector3.zero;
                if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, args.position, args.pressEventCamera, out worldPos))
                {
                    Debug.Assert(false);
                }
                freeRegion.transform.position = worldPos;

                Unsubscribe(tab, this);
                freeRegion.Insert(0, tab, m_dragContent);

                if (m_contentPanel.childCount == 0 && this != m_root.RootRegion)
                {
                    Destroy(gameObject);
                }

                freeRegion.Fit();

                freeRegion.RaiseDepthChanged();

                Outline outline = freeRegion.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;

                    if(freeRegion.m_frameImage != null)
                    {
                        freeRegion.m_frameImage.enabled = true;
                        freeRegion.m_frameImage.color = m_headerImage.color;
                    }
                }
            }
            else
            {
                Unsubscribe(tab, m_beginDragRegion);

                Region targetRegion = tab.GetComponentInParent<Region>();
                Move(tab, m_dragContent, tab.Index, targetRegion, m_splitType);
                targetRegion.UpdateVisualState();

                m_dragContent.localPosition = Vector3.zero;
                ((RectTransform)m_dragContent).Stretch();
            }

            IEnumerable<Tab> children = m_beginDragRegion.m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>());
            ActivateBestTab(children, false);

            if (children.FirstOrDefault() != null)
            {
                m_beginDragRegion.CanMaximize = children.All(t => t.CanMaximize);
            }

            UpdateResizers();
            UpdateVisualState();
            SetRaycastTargets(false);

            m_dragContent = null;
            m_beginDragRegion = null;
            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;

            ForceUpdateLayoutImmediate(m_root.transform);

            if (TabEndDrag != null)
            {
                TabEndDrag(this);
            }
        }

        private void SetTabSiblingIndex(Region region, Tab tab, PointerEventData args, Vector2 localPoint)
        {
            foreach (RectTransform childRT in region.m_tabPanel.transform)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(childRT, args.position, args.pressEventCamera, out localPoint))
                {
                    if (childRT.rect.Contains(localPoint))
                    {
                        if (childRT != m_pointerOverTab)
                        {
                            m_pointerOverTab = childRT;

                            Tab pointerOverTab = m_pointerOverTab.GetComponent<Tab>();
                            int index = pointerOverTab.Index;
                            tab.Index = index;
                            tab.IsPreviewContentActive = false;
                        }
                    }
                }
            }
        }

        private void SetMaxTabSiblingIndex(Tab tab)
        {
            Region region = tab.GetComponentInParent<Region>();
            tab.transform.SetSiblingIndex(Mathf.Max(region.m_tabPanel.transform.childCount - 1, 0));
            m_pointerOverTab = null;
        }

        private static Tab ActivateBestTab(IEnumerable<Tab> tabs, bool incrementActionCounter = true)
        {
            long minID = -1;
            Tab bestTab = null;
            int numActive = 0;

            foreach (Tab tab in tabs)
            {
                if (tab.IsOn)
                {
                    numActive++;
                }
                if (tab.LastActionID > minID)
                {
                    minID = tab.LastActionID;
                    bestTab = tab;
                }
            }

            if (numActive == 0 && bestTab != null)
            {
                bestTab.SetIsOn(true, false);
            }

            return bestTab;
        }

        private RectTransform CreateFreeRegion(Tab tab, Transform content, Vector2 size, Vector2 pivot, Vector3 worldPos)
        {
            Region freeRegion = Instantiate(m_root.RegionPrefab, m_root.Free);
            freeRegion.name = "Region " + m_root.RegionId++;

            RectTransform rt = (RectTransform)freeRegion.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.position = worldPos;

            freeRegion.Insert(0, tab, content);

            ((RectTransform)content).Stretch();

            freeRegion.Fit();

            freeRegion.RaiseDepthChanged();

            ForceUpdateLayoutImmediate(freeRegion.transform);

            Outline outline = freeRegion.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                if (freeRegion.m_frameImage != null && freeRegion.m_headerImage != null)
                {
                    freeRegion.m_frameImage.enabled = true;
                    freeRegion.m_frameImage.color = m_headerImage.color;
                }
            }

            return rt;
        }

        private void ForceUpdateLayoutImmediate(Transform transform)
        {
            if (!m_forceRebuildLayoutImmediate)
            {
                return;
            }

            HorizontalOrVerticalLayoutGroup[] layoutGroups = transform.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            for (int i = 0; i < layoutGroups.Length; ++i)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroups[i].transform);
            }
        }

        private void UpdateVisualState(int expectedChildrenCount = 0)
        {
            if (m_headerImage != null)
            {
                m_headerImage.enabled = m_childrenPanel.childCount == expectedChildrenCount;
            }

            if (m_maximizeToggle != null)
            {
                m_maximizeToggle.gameObject.SetActive(CanMaximize && GetDragRegion() == null && m_childrenPanel.childCount == expectedChildrenCount);

                if (m_root == null || m_root.RootRegion == this)
                {
                    m_maximizeToggle.gameObject.SetActive(false);
                }
            }

            if(m_closeButton != null)
            {
                m_closeButton.gameObject.SetActive(m_tabPanel.transform.childCount > 1);
            }
        }

        private void UpdateResizers()
        {
            if (m_root == null || m_root.RootRegion == null)
            {
                return;
            }

            if (m_root.Free != null)
            {
                Region[] freeRegions = m_root.Free.OfType<Transform>().Select(t => t.GetComponent<Region>()).ToArray();
                foreach (Region region in freeRegions)
                {
                    if(region == null)
                    {
                        continue;
                    }

                    UpdateMinSize(region);
                    region.Fit();

                    Resizer[] freeResizers = region.GetComponentsInChildren<Resizer>();
                    for (int i = 0; i < freeResizers.Length; ++i)
                    {
                        freeResizers[i].UpdateState();
                    }
                }
            }

            if (m_root.Modal != null)
            {
                Region[] modalRegions = m_root.Modal.OfType<Transform>().Select(t => t.GetComponent<Region>()).ToArray();
                foreach (Region region in modalRegions)
                {
                    if (region == null)
                    {
                        continue;
                    }

                    UpdateMinSize(region);
                    region.Fit();

                    Resizer[] modalResizers = region.GetComponentsInChildren<Resizer>();
                    for (int i = 0; i < modalResizers.Length; ++i)
                    {
                        modalResizers[i].UpdateState();
                    }
                }
            }

            UpdateMinSize(m_root.RootRegion);

            Resizer[] resizers = m_root.RootRegion.GetComponentsInChildren<Resizer>();
            for (int i = 0; i < resizers.Length; ++i)
            {
                resizers[i].UpdateState();
            }
        }

        private static Vector2 UpdateMinSize(Region region)
        {
            Vector2 size;
            int childrenCount = region.m_childrenPanel.childCount;
            if (childrenCount >= 2)
            {
                Region child0 = region.m_childrenPanel.GetChild(childrenCount - 2).GetComponent<Region>();
                Vector2 child0Size = UpdateMinSize(child0);

                Region child1 = region.m_childrenPanel.GetChild(childrenCount - 1).GetComponent<Region>();
                Vector2 child1Size = UpdateMinSize(child1);

                if (region.m_childrenPanel.GetComponent<VerticalLayoutGroup>())
                {
                    size.y = child0Size.y + child1Size.y;
                    size.x = Mathf.Max(child0Size.x, child1Size.x);
                }
                else
                {
                    size.x = child0Size.x + child1Size.x;
                    size.y = Mathf.Max(child0Size.y, child1Size.y);
                }
            }
            else
            {
                DockPanel childRoot = region.ContentPanel.GetComponentInChildren<DockPanel>(true);
                if (childRoot != null)
                {
                    size = new Vector2(childRoot.MinWidth, childRoot.MinHeight);
                }
                else
                {
                    size = new Vector2(region.m_minWidth, region.m_minHeight);
                }
            }

            region.MinWidth = size.x;
            region.MinHeight = size.y;
            //region.m_layoutElement.minWidth = size.x;
            //region.m_layoutElement.minHeight = size.y;

            return size;
        }

        private List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        private Region GetTargetRegion(PointerEventData args)
        {
            m_raycastResults.Clear();
            m_root.Raycaster.Raycast(args, m_raycastResults);
            Region result = null;
            for (int i = 0; i < m_raycastResults.Count; ++i)
            {
                RaycastResult raycastResult = m_raycastResults[i];
                Region region = raycastResult.gameObject.GetComponent<Region>();
                if(region == null || region.m_tabPanel == null)
                {
                    continue;
                }

                result = region; 
                if (result.m_root == m_root && result.IsHeaderVisible)
                {
                    break;
                }
            }

            if (result == null)
            {
                result = m_root.RootRegion;
            }

            return result;
        }

        public void MoveRegionToForeground()
        {
            if (BeforeDepthChanged != null)
            {
                CancelArgs args = new CancelArgs();
                BeforeDepthChanged(this, args);
                if (args.Cancel)
                {
                    return;
                }
            }

            if (m_root.Free != null || m_root.Modal != null)
            {
                Transform dragRegion = GetDragRegion();
                if (dragRegion != null)
                {
                    if (dragRegion.GetSiblingIndex() != dragRegion.parent.childCount - 1)
                    {
                        dragRegion.SetSiblingIndex(Mathf.Max(0, dragRegion.parent.childCount - 1));

                        RaiseDepthChanged();
                    }
                }
            }
        }

        public void RaiseDepthChanged()
        {
            Transform dragRegionTransform = GetDragRegion();
            if (dragRegionTransform != null)
            {
                if (DepthChanged != null)
                {
                    foreach (Transform child in dragRegionTransform.parent)
                    {
                        Region[] regions = child.GetComponentsInChildren<Region>();
                        for (int i = 0; i < regions.Length; ++i)
                        {
                            Region region = regions[i];
                            DepthChanged(region, child.GetSiblingIndex() + 1);
                        }
                    }
                }
            }
            else
            {
                if (DepthChanged != null)
                {
                    DepthChanged(this, 0);
                }
            }
        }

        private void OnMaxmizeValueChanged(bool value)
        {
            Maximize(value);
        }

        private void OnClose()
        {
            Destroy();
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (GetDragRegion() != null)
            {
                eventData.useDragThreshold = false;
            }
        }

        private bool m_isDragging;
        private Vector3 m_prevPoint;
        private Transform m_dragRegion;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (BeforeBeginDrag != null)
            {
                CancelArgs cancelArgs = new CancelArgs();
                BeforeBeginDrag(this, cancelArgs);
                if (cancelArgs.Cancel)
                {
                    return;
                }
            }

            m_root.CursorHelper.SetCursor(this, null);
            m_dragRegion = GetDragRegion();

            if (m_dragRegion)
            {
                if (m_root.Modal != null && m_root.Modal.childCount > 0 && m_dragRegion.parent != m_root.Modal)
                {
                    m_isDragging = false;
                }
                else
                {
                    m_isDragging = RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)m_dragRegion, eventData.position, eventData.pressEventCamera, out m_prevPoint);
                    if (m_isDragging)
                    {
                        MoveRegionToForeground();
                        if (BeginDrag != null)
                        {
                            BeginDrag(this);
                        }
                    }
                }
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)Root.transform, eventData.position, eventData.pressEventCamera))
            //{
            //    return;
            //}

            if (m_isDragging)
            {
                Vector3 point;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)m_dragRegion, eventData.position, eventData.pressEventCamera, out point))
                {
                    Vector3 delta = point - m_prevPoint;
                    m_prevPoint = point;
                    m_dragRegion.position += delta;

                    if (Drag != null)
                    {
                        Drag(this);
                    }
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            m_root.CursorHelper.ResetCursor(this);

            if (m_dragRegion == null)
            {
                return;
            }

            if (!m_root.AllowDragOutside)
            {
                Region region = m_dragRegion.GetComponentInChildren<Region>();
                region.Fit();
            }

            m_dragRegion = null;

            if (m_isDragging)
            {
                if (EndDrag != null)
                {
                    EndDrag(this);
                }
            }

            m_isDragging = false;
        }


        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            MoveRegionToForeground();

            IsSelected = true;
        }

        public static Region FindRegion(Transform content)
        {
            return content.GetComponentInParent<Region>();
        }

        public static Tab FindTab(Transform content)
        {
            Region region = content.GetComponentInParent<Region>();
            if (region == null)
            {
                return null;
            }

            if (content.parent == null)
            {
                return null;
            }
            while (content.parent != null && content.parent != region.m_contentPanel)
            {
                content = content.parent;
            }
            if (content == null || content.parent == null)
            {
                return null;
            }

            int index = content.GetSiblingIndex();
            if (region.m_tabPanel.transform.childCount == 0)
            {
                return null;
            }

            // Fix for issue https://github.com/Battlehub0x/Unity_RuntimeEditor/issues/7, see details there.
            // One of the tabs got destroyed already, but cleanup of the window is still ongoing
            if (content.parent.childCount != region.m_tabPanel.transform.childCount)
            {
                // Always take the previous tab (assumes new window(s) are appended so destroyed window(s) sit(s) at the beginning)
                if (index > 0)
                {
                    index -= 1;
                }
            }

            return region.m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
        }
    }
}

