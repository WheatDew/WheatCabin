using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.UIControls.DockPanels;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTEditor.Models;

namespace Battlehub.RTEditor
{
    public class PromptDialogArgs : DialogCancelArgs
    {
        private DialogCancelArgs m_args;

        public virtual string Text { get; private set; }
        public override bool Cancel 
        {
            get { return m_args.Cancel; }
            set { m_args.Cancel = value; } 
        }
        public PromptDialogArgs(string text)
        {
            Text = text;
            m_args = new DialogCancelArgs();
        }
        public PromptDialogArgs(string text, DialogCancelArgs args)
        {
            Text = text;
            m_args = args;
        }
    }

    public class Workspace : MonoBehaviour
    {
        public event Action<Workspace> AfterLayout;
        public event Action<Transform> WindowCreated;
        public event Action<Transform> WindowDestroyed;
        public event Action DeferUpdate;

        [SerializeField]
        private DialogManager m_dialogManager = null;
        public DialogManager DialogManager
        {
            get { return m_dialogManager; }
            set { m_dialogManager = value; }
        }

        [SerializeField]
        private RectTransform m_inputDialog = null;
        public RectTransform InputDialog
        {
            get { return m_inputDialog; }
            set { m_inputDialog = value; }
        }
     
        [SerializeField]
        private DockPanel m_dockPanel = null;
        public DockPanel DockPanel
        {
            get { return m_dockPanel; }
            set { m_dockPanel = value; }
        }


        [SerializeField]
        private Transform m_componentsRoot = null;
        public Transform ComponentsRoot
        {
            get { return m_componentsRoot; }
            set { m_componentsRoot = value; }
        }

        [SerializeField]
        private RectTransform m_toolsRoot = null;
        public RectTransform ToolsRoot
        {
            get { return m_toolsRoot; }
            set { m_toolsRoot = value; }
        }

        [SerializeField]
        private RectTransform m_topBar = null;
        public RectTransform TopBar
        {
            get { return m_topBar; }
            set { m_topBar = value; }
        }

        [SerializeField]
        private RectTransform m_bottomBar = null;
        public RectTransform BottomBar
        {
            get { return m_bottomBar; }
            set { m_bottomBar = value; }
        }

        [SerializeField]
        private RectTransform m_leftBar = null;
        public RectTransform LeftBar
        {
            get { return m_leftBar; }
            set { m_leftBar = value; }
        }

        [SerializeField]
        private RectTransform m_rightBar = null;
        public RectTransform RightBar
        {
            get { return m_rightBar; }
            set { m_rightBar = value; }
        }

        private bool m_isPointerOverActiveWindow = true;
        public bool IsPointerOverActiveWindow
        {
            get { return m_isPointerOverActiveWindow; }
            set { m_isPointerOverActiveWindow = value; }
        }

        private RuntimeWindow[] Windows
        {
            get { return m_editor.Windows; }
        }

        private IInput Input
        {
            get { return m_editor.Input; }
        }

        private IUIRaycaster Raycaster
        {
            get { return m_editor.Raycaster; }
        }

        public readonly Dictionary<Transform, string> m_windowToType = new Dictionary<Transform, string>();
        public readonly Dictionary<string, HashSet<Transform>> m_windows = new Dictionary<string, HashSet<Transform>>();
        public readonly Dictionary<Transform, List<Transform>> m_extraComponents = new Dictionary<Transform, List<Transform>>();

        public Func<IWindowManager, LayoutInfo> m_overrideLayoutCallback;
        public string m_activateWindowOfType;

        private IRTE m_editor;
        private ILocalization m_localization;
        private IWindowManager m_windowManager;
        private ILayoutStorageModel m_layoutStorage;
        private bool m_lockUpdateLayout;
        private bool m_isTabDragInProgress;
        
        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            m_windowManager = IOC.Resolve<IWindowManager>();
            m_editor = IOC.Resolve<IRTE>();
            m_localization = IOC.Resolve<ILocalization>();
            m_layoutStorage = IOC.Resolve<ILayoutStorageModel>();

            Unsubscribe();

            if(m_dockPanel != null)
            {
                m_dockPanel.CursorHelper = m_editor.CursorHelper;
                if (RenderPipelineInfo.UseRenderTextures)
                {
                    DepthMaskingBehavior depthMaskingBehavior = m_dockPanel.GetComponent<DepthMaskingBehavior>();
                    Destroy(depthMaskingBehavior);
                }
            }

            Subscribe();
        }


        private void OnDestroy()
        {
            Unsubscribe();
            m_layoutStorage = null;
        }

        public void SetTools(Transform tools)
        {
            Transform window = GetWindow(RuntimeWindowType.ToolsPanel.ToString().ToLower());
            if (window != null)
            {
                OnContentDestroyed(window);
            }

            SetContent(m_toolsRoot, tools);
        }

        public void SetLeftBar(Transform tools)
        {
            SetContent(m_leftBar, tools);
        }

        public void SetRightBar(Transform tools)
        {
            SetContent(m_rightBar, tools);
        }

        public void SetTopBar(Transform tools)
        {
            SetContent(m_topBar, tools);
        }

        public void SetBottomBar(Transform tools)
        {
            SetContent(m_bottomBar, tools);
        }

        private void SetContent(Transform root, Transform content)
        {
            if (root != null)
            {
                foreach (Transform child in root)
                {
                    Destroy(child.gameObject);
                }
            }

            if (content != null)
            {
                content.SetParent(root, false);

                RectTransform rt = content as RectTransform;
                if (rt != null)
                {
                    rt.Stretch();
                }

                content.gameObject.SetActive(true);
            }
        }

        public LayoutInfo CreateLayoutInfo(Transform content, string header, Sprite icon)
        {
            return CreateLayoutInfo(content, null,  header, icon);
        }

        public LayoutInfo CreateLayoutInfo(Transform content, GameObject tabPrefab, string header, Sprite icon)
        {
            return CreateLayoutInfo(content, tabPrefab, null, header, icon); 
        }

        public LayoutInfo CreateLayoutInfo(Transform content, GameObject tabPrefab, string args, string header, Sprite icon)
        {
            Tab tab = tabPrefab != null ? tabPrefab.GetComponent<Tab>() : null;
            if(tab != null)
            {
                tab = Instantiate(tab);
            }
            else
            {
                tab = Instantiate(DockPanel.TabPrefab);
            }

            tab.name = "Tab " + header;
            tab.Text = header;
            tab.Icon = icon;
            return new LayoutInfo(content, tab, args);
        }


        public bool ValidateLayout(LayoutInfo layoutInfo)
        {
            return DockPanel.RootRegion.Validate(layoutInfo);
        }

        public void OverrideDefaultLayout(Func<IWindowManager, LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            m_overrideLayoutCallback = buildLayoutCallback;
            m_activateWindowOfType = activateWindowOfType;
        }

        public void SetDefaultLayout()
        {
            if (m_overrideLayoutCallback != null)
            {
                SetLayout(m_overrideLayoutCallback, m_activateWindowOfType);
            }
            else
            {
                SetLayout(wm => IWindowManagerExt.GetBuiltInDefaultLayout(wm), RuntimeWindowType.Scene.ToString().ToLower());
            }
        }

        public void SetLayout(Func<IWindowManager, LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            Region rootRegion = DockPanel.RootRegion;
            if (rootRegion == null)
            {
                return;
            }
            if (m_editor == null)
            {
                return;
            }

            try
            {
                m_lockUpdateLayout = true;

                bool waitForEndOfFrameBeforeReset = rootRegion.HasChildren;
                //bool waitForEndOfFrameBeforeReset = false;

                ClearRegion(rootRegion);
                foreach (Transform child in DockPanel.Free)
                {
                    Region region = child.GetComponent<Region>();
                    ClearRegion(region);
                    Destroy(region.gameObject);
                }

                m_editor.StartCoroutine(CoSetLayout(waitForEndOfFrameBeforeReset, buildLayoutCallback, activateWindowOfType));
            }
            catch
            {
                m_lockUpdateLayout = false;
            }
        }

        private IEnumerator CoSetLayout(bool waitForEndOfFrame, Func<IWindowManager, LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            if(waitForEndOfFrame)
            {
                //Wait for OnDestroy of destroyed windows 
                yield return new WaitForEndOfFrame();
            }
            
            try
            {
                m_lockUpdateLayout = true;                
                LayoutInfo layout = buildLayoutCallback(m_windowManager);
                if(layout != null)
                {
                    if (layout.Content != null || layout.TabGroup != null || layout.Child0 != null && layout.Child1 != null)
                    {
                        DockPanel.RootRegion.Build(layout);
                    }

                    if (!string.IsNullOrEmpty(activateWindowOfType))
                    {
                        ActivateWindow(activateWindowOfType);
                    }
                }
            }
            finally
            {
                m_lockUpdateLayout = false;
            }

            RuntimeWindow[] windows = Windows;
            if (windows != null)
            {
                for (int i = 0; i < windows.Length; ++i)
                {
                    windows[i].EnableRaycasts();
                    windows[i].HandleResize();
                }
            }

            if (AfterLayout != null)
            {
                AfterLayout(this);
            }
        }

        public LayoutInfo GetLayout()
        {
            LayoutInfo layout = new LayoutInfo();
            ToLayout(DockPanel.RootRegion, layout);
            return layout;
        }

        private void ToLayout(Region region, Transform content, LayoutInfo layoutInfo)
        {
            foreach (KeyValuePair<string, HashSet<Transform>> kvp in m_windows)
            {
                if (kvp.Value.Contains(content))
                {
                    layoutInfo.Content = content;

                    RuntimeWindow window = content.GetComponentInChildren<RuntimeWindow>(true);
                    if (window != null)
                    {
                        layoutInfo.Args = window.Args;
                    }

                    Tab tab = Region.FindTab(content);
                    if (tab != null)
                    {
                        layoutInfo.CanDrag = tab.CanDrag;
                        layoutInfo.CanClose = tab.CanClose;
                        layoutInfo.IsOn = tab.IsOn;
                        layoutInfo.CanMaximize = tab.CanMaximize;
                    }

                    layoutInfo.IsHeaderVisible = region.IsHeaderVisible;

                    break;
                }
            }
        }

        private void ToLayout(Region region, LayoutInfo layoutInfo)
        {
            if (region.HasChildren)
            {
                Region childRegion0 = region.GetChild(0);
                Region childRegion1 = region.GetChild(1);

                RectTransform rt0 = (RectTransform)childRegion0.transform;
                RectTransform rt1 = (RectTransform)childRegion1.transform;

                Vector3 delta = rt0.localPosition - rt1.localPosition;
                layoutInfo.IsVertical = Mathf.Abs(delta.x) < Mathf.Abs(delta.y);

                if (layoutInfo.IsVertical)
                {
                    float y0 = Mathf.Max(0.000000001f, rt0.sizeDelta.y - childRegion0.MinHeight);
                    float y1 = Mathf.Max(0.000000001f, rt1.sizeDelta.y - childRegion1.MinHeight);

                    layoutInfo.Ratio = y0 / (y0 + y1);
                }
                else
                {
                    float x0 = Mathf.Max(0.000000001f, rt0.sizeDelta.x - childRegion0.MinWidth);
                    float x1 = Mathf.Max(0.000000001f, rt1.sizeDelta.x - childRegion1.MinWidth);

                    layoutInfo.Ratio = x0 / (x0 + x1);
                }

                layoutInfo.Child0 = new LayoutInfo();
                layoutInfo.Child1 = new LayoutInfo();

                ToLayout(childRegion0, layoutInfo.Child0);
                ToLayout(childRegion1, layoutInfo.Child1);
            }
            else
            {
                if (region.ContentPanel.childCount > 1)
                {
                    layoutInfo.TabGroup = new LayoutInfo[region.ContentPanel.childCount];
                    for (int i = 0; i < region.ContentPanel.childCount; ++i)
                    {
                        Transform content = region.ContentPanel.GetChild(i);
                        LayoutInfo tabLayout = new LayoutInfo();

                        ToLayout(region, content, tabLayout);
                        layoutInfo.TabGroup[i] = tabLayout;
                    }
                }
                else if (region.ContentPanel.childCount == 1)
                {
                    Transform content = region.ContentPanel.GetChild(0);
                    ToLayout(region, content, layoutInfo);
                }
            }
        }

        private void ClearRegion(Region rootRegion)
        {
            rootRegion.CloseAllTabs();
        }

        public string GetWindowTypeName(Transform content)
        {
            if(content == null || !m_windowToType.ContainsKey(content))
            {
                return null;
            }
            return m_windowToType[content];
        }

        public Transform GetWindow(string windowTypeName)
        {
            HashSet<Transform> hs;
            if (m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                return hs.FirstOrDefault();
            }
            return null;
        }

        public Transform[] GetWindows()
        {
            return m_windows.Values.SelectMany(w => w).ToArray();
        }

        public Transform[] GetWindows(string windowTypeName)
        {
            HashSet<Transform> hs;
            if (m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                return hs.ToArray();
            }
            return new Transform[0];
        }

        public Transform[] GetComponents(Transform content)
        {
            List<Transform> extraComponents;
            if (m_extraComponents.TryGetValue(content, out extraComponents))
            {
                return extraComponents.ToArray();
            }
            return new Transform[0];
        }

        public bool IsActive(string windowTypeName)
        {
            HashSet<Transform> hs;
            if (m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                foreach (Transform content in hs)
                {
                    Tab tab = Region.FindTab(content);
                    if (tab != null && tab.IsOn)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsActive(Transform content)
        {
            Tab tab = Region.FindTab(content);
            return tab != null && tab.IsOn;
        }

        public bool ActivateWindow(string windowTypeName)
        {
            Transform content = GetWindow(windowTypeName);
            if (content == null)
            {
                return false;
            }
            return ActivateWindow(content);
        }

        public bool ActivateWindow(Transform content)
        {
            if (content == null)
            {
                return false;
            }

            if (m_isTabDragInProgress)
            {
                return false;
            }

            Tab tab = Region.FindTab(content);
            if (tab == null)
            {
                return false;
            }

            Region region = content.GetComponentInParent<Region>();
            if (region != null)
            {
                region.MoveRegionToForeground();

                IsPointerOverActiveWindow = m_editor != null && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)region.transform, Input.GetPointerXY(0), Raycaster.eventCamera);
                if (IsPointerOverActiveWindow)
                {
                    RuntimeWindow[] windows = Windows;
                    for (int i = 0; i < windows.Length; ++i)
                    {
                        windows[i].DisableRaycasts();
                    }
                }
            }
         
            tab.IsOn = true;
            return true;
        }

        public Transform CreateWindow(string windowTypeName, bool isFree = true, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f, Transform parentWindow = null)
        {
            return CreateWindow(windowTypeName, out _, isFree, splitType, flexibleSize, parentWindow);
        }

        private Transform CreateWindow(string windowTypeName, out Dialog dialog, bool isFree = true, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f, Transform parentWindow = null)
        {
            WindowDescriptor wd;
            GameObject content;
            bool isDialog;

            Transform window = CreateWindow(windowTypeName, out wd, out content, out isDialog);
            if (!window)
            {
                dialog = null;
                return window;
            }

            if (isDialog && isFree)
            {
                RectTransform rt = window as RectTransform;
                dialog = m_dialogManager.ShowDialog(
                    wd.Icon, 
                    wd.Header, 
                    content.transform, 
                    null, "OK", 
                    null, "Cancel",
                    100,
                    100,
                    rt != null ? rt.rect.width : 700,
                    rt != null ? rt.rect.height : 400, 
                    true);

                dialog.IsCancelVisible = false;
                dialog.IsOkVisible = false;
            }
            else
            {
                dialog = null;

                Region targetRegion = null;
                if (parentWindow != null)
                {
                    targetRegion = parentWindow.GetComponentInParent<Region>();
                }

                if (targetRegion == null)
                {
                    targetRegion = DockPanel.RootRegion;
                }

                if(splitType == RegionSplitType.None)
                {
                    while (targetRegion.HasChildren)
                    {
                        targetRegion = targetRegion.GetChild(0);
                    }
                }
                
                Tab tab = InstantiateTab(wd);
                tab.name = "Tab " + wd.Header;
                tab.Text = wd.Header;
                tab.Icon = wd.Icon;

                targetRegion.Add(tab, content.transform, isFree, splitType, flexibleSize);

                if (!isFree)
                {
                    ForceLayoutUpdate();
                }

                RuntimeWindow region = window.GetComponentInParent<RuntimeWindow>();
                if (region != null)
                {
                    region.HandleResize();
                }

                targetRegion.RaiseDepthChanged();

            }

            ActivateContent(wd, content);

            if (WindowCreated != null)
            {
                WindowCreated(window);
            }

            return window;
        }
    
        public Transform CreatePopup(string windowTypeName, bool canResize = false, float minWidth = 10, float minHeight = 10)
        {
            return CreatePopup(windowTypeName, Vector2.zero, canResize, minWidth, minHeight);
        }

        public bool ScreenPointToLocalPointInRectangle(RectTransform rectTransform, Vector3 screenPoint, out Vector2 position)
        {
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            Camera camera = appearance.UIForegroundScaler.GetComponent<Canvas>().worldCamera;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out position);
        }

        public Transform CreatePopup(string windowTypeName, Vector2 position, bool canResize = false, float minWidth = 10, float minHeight = 10)
        {
            Transform window = CreateWindow(windowTypeName, out WindowDescriptor wd, out GameObject content, out _);
            if(window == null)
            {
                return null;
            }

            Tab tab = InstantiateTab(wd);
            tab.name = "Tab " + wd.Header;
            tab.Text = wd.Header;
            tab.Icon = wd.Icon;

            RectTransform rt = window as RectTransform;
            Vector2 size;
            size.x = rt != null ? Mathf.Max(rt.rect.width, minWidth) : minWidth;
            size.y = rt != null ? Mathf.Max(rt.rect.height, minHeight) : minHeight;

            DockPanel.AddPopupRegion(tab, window, new Rect(position.x, position.y, size.x, size.y), new Vector2(0.0f, 1.0f), canResize);
            
            Region region = Region.FindRegion(window);
            RectTransform regionRT = (RectTransform)region.transform;
            regionRT.SetAnchors(new Vector2(0, 1), new Vector2(0, 1));

            region.Fit(false);

            ActivateContent(wd, content);
            if (WindowCreated != null)
            {
                WindowCreated(window);
            }

            return window;
        }
        public Transform CreateDropdown(string windowTypeName, RectTransform anchor, bool canResize = false, float minWidth = 10, float minHeight = 10)
        {
            Transform window = CreateWindow(windowTypeName, out WindowDescriptor wd, out GameObject content, out _);
            
            Tab tab = InstantiateTab(wd);
            tab.name = "Tab " + wd.Header;
            tab.Text = wd.Header;
            tab.Icon = wd.Icon;

            RectTransform rt = window as RectTransform;
            Vector2 size;
            size.x = rt != null ? Mathf.Max(rt.rect.width, minWidth) : minWidth;
            size.y = rt != null ? Mathf.Max(rt.rect.height, minHeight) : minHeight;

            DockPanel.AddDropdownRegion(tab, window, anchor, size, canResize);

            ActivateContent(wd, content);
            if (WindowCreated != null)
            {
                WindowCreated(window);
            }

            return window;
        }

        public void SetWindowArgs(Transform content, string args)
        {
            RuntimeWindow window = content.GetComponentInChildren<RuntimeWindow>(true);
            if (window != null)
            {
                window.Args = args;
            }
        }

        public void DestroyWindow(Transform content)
        {
            Tab tab = Region.FindTab(content);
            if (tab != null)
            {
                DockPanel.RemoveRegion(content);
            }
            else
            {
                OnContentDestroyed(content);
            }
        }

        public void DestroyWindowsOfType(string windowTypeName)
        {
            foreach(Transform content in m_windows[windowTypeName].ToArray())
            {
                DestroyWindow(content);
            }
        }

        public Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
            bool canResize,
            float minWidth,
            float minHeight)
        {
            return CreateDialogWindow(windowTypeName, header, okAction, cancelAction, minWidth, minHeight, -1, -1, canResize);
        }

        public Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
            float minWidth,
            float minHeight,
            float preferredWidth,
            float preferredHeight,
            bool canResize = true)
        {
            WindowDescriptor wd;
            GameObject content;
            bool isDialog;

            Transform window = CreateWindow(windowTypeName, out wd, out content, out isDialog);
            if (!window)
            {
                return window;
            }

            if (isDialog)
            {
                if (header == null)
                {
                    header = wd.Header;
                }
                
                if(preferredWidth < 0)
                {
                    RectTransform rt = window as RectTransform;
                    preferredWidth = rt != null ? rt.rect.width : minWidth;
                }
                if (preferredHeight < 0)
                {
                    RectTransform rt = window as RectTransform;
                    preferredHeight = rt != null ? rt.rect.height : minHeight;
                }

                Dialog dialog = m_dialogManager.ShowDialog(wd.Icon, header, content.transform,
                    okAction, m_localization.GetString("ID_RTEditor_WM_Dialog_OK", "OK"),
                    cancelAction, m_localization.GetString("ID_RTEditor_WM_Dialog_Cancel", "Cancel"),
                    minWidth, minHeight, preferredWidth, preferredHeight, canResize);
                dialog.IsCancelVisible = false;
                dialog.IsOkVisible = false;
            }
            else
            {
                throw new ArgumentException(windowTypeName + " is not a dialog");
            }

            ActivateContent(wd, content);

            return window;
        }

        public void DestroyDialogWindow()
        {
            m_dialogManager.CloseDialog();
        }

        public Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out GameObject content, out bool isDialog)
        {
            if (DockPanel == null)
            {
                Debug.LogError("Unable to create window. m_dockPanel == null. Set m_dockPanel field");
            }

            windowTypeName = windowTypeName.ToLower();

            content = null;
            wd = m_windowManager.GetWindowDescriptor(windowTypeName, out isDialog);
            if (wd == null)
            {
                Debug.LogWarningFormat("{0} window was not found", windowTypeName);
                return null;
            }

            if (wd.Created >= wd.MaxWindows && wd.MaxWindows > 0)
            {
                return null;
            }
            wd.Created++;

            if(wd.TabPrefab == null && DockPanel.TabPrefab != null)
            {
                wd.TabPrefab = DockPanel.TabPrefab.gameObject;
            }

            if (wd.ContentPrefab != null)
            {
                wd.ContentPrefab.SetActive(false);
                content = Instantiate(wd.ContentPrefab);
                content.name = windowTypeName;

                Transform[] children = content.transform.OfType<Transform>().ToArray();
                for (int i = 0; i < children.Length; ++i)
                {
                    Transform component = children[i];
                    if (!(component is RectTransform))
                    {
                        component.gameObject.SetActive(false);
                        component.transform.SetParent(m_componentsRoot, false);
                    }
                }

                List<Transform> extraComponents = new List<Transform>();
                for (int i = 0; i < children.Length; ++i)
                {
                    if (children[i].parent == m_componentsRoot)
                    {
                        extraComponents.Add(children[i]);
                    }
                }

                m_extraComponents.Add(content.transform, extraComponents);
            }
            else
            {
                content = new GameObject();
                content.AddComponent<RectTransform>();
                content.name = "Empty Content";

                m_extraComponents.Add(content.transform, new List<Transform>());
            }

            HashSet<Transform> windows;
            if (!m_windows.TryGetValue(windowTypeName, out windows))
            {
                windows = new HashSet<Transform>();
                m_windows.Add(windowTypeName, windows);
            }

            m_windowToType.Add(content.transform, windowTypeName);
            windows.Add(content.transform);

            return content.transform;
        }

        private void ActivateContent(WindowDescriptor wd, GameObject content)
        {
            List<Transform> extraComponentsList;
            m_extraComponents.TryGetValue(content.transform, out extraComponentsList);
            for (int i = 0; i < extraComponentsList.Count; ++i)
            {
                extraComponentsList[i].gameObject.SetActive(true);
            }

            wd.ContentPrefab.SetActive(true);
            content.SetActive(true);
        }

        public void MessageBox(string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            m_dialogManager.ShowDialog(null, header, text,
                ok, m_localization.GetString("ID_RTEditor_WM_Dialog_OK", "OK"),
                null, m_localization.GetString("ID_RTEditor_WM_Dialog_Cancel", "Cancel"));
        }

        public void MessageBox(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            m_dialogManager.ShowDialog(icon, header, text,
                ok, m_localization.GetString("ID_RTEditor_WM_Dialog_OK", "OK"),
                null, m_localization.GetString("ID_RTEditor_WM_Dialog_Cancel", "Cancel"));
        }

        public void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            m_dialogManager.ShowDialog(null, header, text, ok, okText, cancel, cancelText);
        }

        public void Confirmation(
            string header, 
            string text, 
            DialogAction<DialogCancelArgs> ok,
            DialogAction<DialogCancelArgs> cancel,
            DialogAction<DialogCancelArgs> alt,
            string okText = "OK", 
            string cancelText = "Cancel",
            string altText = "Close")
        {
            m_dialogManager.ShowComplexDialog(null, header, text, ok, okText, cancel, cancelText, alt, altText);
        }

        public void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            m_dialogManager.ShowDialog(icon, header, text, ok, okText, cancel, cancelText);
        }

        public void Confirmation(
            Sprite icon, 
            string header, 
            string text, 
            DialogAction<DialogCancelArgs> ok, 
            DialogAction<DialogCancelArgs> cancel, 
            DialogAction<DialogCancelArgs> alt,
            string okText = "OK", 
            string cancelText = "Cancel",
            string altText = "Close")
        {
            m_dialogManager.ShowComplexDialog(icon, header, text, ok, okText, cancel, cancelText, alt, altText);
        }

        public void Prompt(string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            Prompt(null, header, text, ok, cancel, okText, cancelText);
        }
        public virtual void Prompt(Sprite icon, string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            RectTransform inputDialog = Instantiate(m_inputDialog);
            InputViewModel input = inputDialog.GetComponent<InputViewModel>();
            
            input.Text = text;
            input.gameObject.SetActive(true);
            
            Dialog(icon, header, input.transform,
                (sender, okArgs) =>
                {
                    ok?.Invoke(sender, new PromptDialogArgs(input.Text, okArgs));
                },
                (sender, cancelArgs) =>
                {
                    cancel?.Invoke(sender, new PromptDialogArgs(input.Text, cancelArgs));
                }, okText, cancelText, 350, 100, 350, 90, false);
        }

        public void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true)
        {
            m_dialogManager.ShowDialog(null, header, content, ok, okText, cancel, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true)
        {
            m_dialogManager.ShowDialog(icon, header, content, ok, okText, cancel, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public bool IsPointerOver(RuntimeWindow testWindow)
        {
            return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)testWindow.transform, Input.GetPointerXY(0), Raycaster.eventCamera);
        }

        public Transform FindPointerOverWindow(RuntimeWindow exceptWindow = null)
        {
            foreach (KeyValuePair<string, HashSet<Transform>> kvp in m_windows)
            {
                foreach (Transform content in kvp.Value)
                {
                    RuntimeWindow window = content.GetComponentInChildren<RuntimeWindow>();

                    if (window != null && window != exceptWindow && IsPointerOver(window) )
                    {
                        Tab tab = Region.FindTab(window.transform);
                        if (tab != null && tab.IsOn)
                        {
                            return content;
                        }
                    }
                }
            }
            return null;
        }

        public void CopyTransform(Transform targetConent, Transform sourceContent)
        {
            DockPanel.CopyTransform(Region.FindRegion(targetConent), Region.FindRegion(sourceContent));
        }

        public void SetTransform(Transform content, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            DockPanel.SetTransform(Region.FindRegion(content), anchoredPosition, anchorMin, anchorMax, sizeDelta);
        }

        private Tab InstantiateTab(WindowDescriptor wd)
        {
            Tab tab = GetTabPrefab(wd);
            if (tab == null)
            {
                tab = DockPanel.TabPrefab;
            }
            tab = Instantiate(tab);
            return tab;
        }

        private Tab GetTabPrefab(WindowDescriptor wd)
        {
            if(wd.TabPrefab != null)
            {
                return wd.TabPrefab.GetComponent<Tab>();
            }
            return null;
        }

        public string GetHeaderText(Transform content)
        {
            Tab tab = Region.FindTab(content);
            return tab.Text;
        }

        public Sprite GetHeaderIcon(Transform content)
        {
            Tab tab = Region.FindTab(content);
            return tab.Icon;
        }

        public void SetHeaderText(Transform content, string headerText)
        {
            Tab tab = Region.FindTab(content);
            tab.Text = headerText;
        }

        public void SetHeaderIcon(Transform content, Sprite icon)
        {
            Tab tab = Region.FindTab(content);
            tab.Icon = icon;
        }

        public void ForceLayoutUpdate()
        {
            if(m_lockUpdateLayout)
            {
                return;
            }

            DockPanel.ForceUpdateLayout();
        }

        private void Subscribe()
        {
            if(m_dialogManager != null)
            {
                m_dialogManager.DialogCreated += OnDialogCreated;
                m_dialogManager.DialogDestroyed += OnDialogDestroyed;
            }
            
            if(m_dockPanel != null)
            {
                m_dockPanel.TabActivated += OnTabActivated;
                m_dockPanel.TabDeactivated += OnTabDeactivated;
                m_dockPanel.TabClosed += OnTabClosed;
                m_dockPanel.TabBeginDrag += OnTabBeginDrag;
                m_dockPanel.TabEndDrag += OnTabEndDrag;

                m_dockPanel.RegionBeforeDepthChanged += OnRegionBeforeDepthChanged;
                m_dockPanel.RegionDepthChanged += OnRegionDepthChanged;
                m_dockPanel.RegionSelected += OnRegionSelected;
                m_dockPanel.RegionUnselected += OnRegionUnselected;
                m_dockPanel.RegionEnabled += OnRegionEnabled;
                m_dockPanel.RegionDisabled += OnRegionDisabled;
                m_dockPanel.RegionMaximized += OnRegionMaximized;
                m_dockPanel.RegionBeforeBeginDrag += OnRegionBeforeBeginDrag;
                m_dockPanel.RegionBeginResize += OnBeginResize;
                m_dockPanel.RegionEndResize += OnRegionEndResize;
            }
        }

        private void Unsubscribe()
        {
            if (m_dockPanel != null)
            {
                m_dockPanel.TabActivated -= OnTabActivated;
                m_dockPanel.TabDeactivated -= OnTabDeactivated;
                m_dockPanel.TabClosed -= OnTabClosed;
                m_dockPanel.TabBeginDrag -= OnTabBeginDrag;
                m_dockPanel.TabEndDrag -= OnTabEndDrag;

                m_dockPanel.RegionBeforeDepthChanged -= OnRegionBeforeDepthChanged;
                m_dockPanel.RegionDepthChanged -= OnRegionDepthChanged;
                m_dockPanel.RegionSelected -= OnRegionSelected;
                m_dockPanel.RegionUnselected -= OnRegionUnselected;
                m_dockPanel.RegionEnabled -= OnRegionEnabled;
                m_dockPanel.RegionDisabled -= OnRegionDisabled;
                m_dockPanel.RegionMaximized -= OnRegionMaximized;
                m_dockPanel.RegionBeforeBeginDrag -= OnRegionBeforeBeginDrag;
                m_dockPanel.RegionBeginResize -= OnBeginResize;
                m_dockPanel.RegionEndResize -= OnRegionEndResize;
            }

            if (m_dialogManager != null)
            {
                m_dialogManager.DialogCreated -= OnDialogCreated;
                m_dialogManager.DialogDestroyed -= OnDialogDestroyed;
            }
        }

        private void OnDialogCreated(Dialog sender)
        {
            SetBlockRaycasts(m_topBar, false);
            SetBlockRaycasts(m_bottomBar, false);
            SetBlockRaycasts(m_leftBar, false);
            SetBlockRaycasts(m_rightBar, false);
        }

        private void OnDialogDestroyed(Dialog dialog)
        {
            RuntimeWindow dialogWindow = dialog.Content.GetComponentInParent<RuntimeWindow>();

            OnContentDestroyed(dialog.Content);

            if (!m_dialogManager.IsDialogOpened)
            {
                SetBlockRaycasts(m_topBar, true);
                SetBlockRaycasts(m_bottomBar, true);
                SetBlockRaycasts(m_leftBar, true);
                SetBlockRaycasts(m_rightBar, true);

                Transform pointerOverWindow = dialog.Content != null ? FindPointerOverWindow(dialogWindow) : null;
                if (pointerOverWindow != null)
                {
                    RuntimeWindow window = pointerOverWindow.GetComponentInChildren<RuntimeWindow>();
                    if (window == null)
                    {
                        window = m_editor.GetWindow(RuntimeWindowType.Scene);
                    }
                    window.IsPointerOver = true;
                    m_editor.ActivateWindow(window);
                }
                else
                {
                    RuntimeWindow window = m_editor.GetWindow(RuntimeWindowType.Scene);
                    m_editor.ActivateWindow(window);
                }

                if(DeferUpdate != null)
                {
                    DeferUpdate();
                }
            }
        }

        private void SetBlockRaycasts(RectTransform rt, bool blockRaycasts)
        {
            if (rt == null)
            {
                return;
            }

            CanvasGroup canvasGroup = rt.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = blockRaycasts;
            }
        }

        private void OnRegionSelected(Region region)
        {
        }

        private void OnRegionUnselected(Region region)
        {

        }

        private void OnBeginResize(Resizer resizer, Region region)
        {

        }

        private void OnRegionEndResize(Resizer resizer, Region region)
        {
            if (DeferUpdate != null)
            {
                DeferUpdate();
            }
        }

        private int m_lastTabActionID = -1;
        private void OnTabActivated(Region region, Transform content)
        {
            List<Transform> extraComponents;
            if (m_extraComponents.TryGetValue(content, out extraComponents))
            {
                for (int i = 0; i < extraComponents.Count; ++i)
                {
                    Transform extraComponent = extraComponents[i];
                    extraComponent.gameObject.SetActive(true);
                }
            }

            Tab tab = Region.FindTab(content);
            if(tab.LastActionID > m_lastTabActionID)
            {
                m_lastTabActionID = tab.LastActionID;

                if (!m_isTabDragInProgress)
                {
                    RuntimeWindow window = region.ContentPanel.GetComponentInChildren<RuntimeWindow>();
                    if (window != null)
                    {
                        window.Editor.ActivateWindow(window);
                    }
                }
            }
        }

        private void OnTabDeactivated(Region region, Transform content)
        {
            List<Transform> extraComponents;
            if (m_extraComponents.TryGetValue(content, out extraComponents))
            {
                for (int i = 0; i < extraComponents.Count; ++i)
                {
                    Transform extraComponent = extraComponents[i];
                    if (extraComponent)
                    {
                        extraComponent.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnTabClosed(Region region, Transform content)
        {
            OnContentDestroyed(content);

            if(!m_lockUpdateLayout)
            {
                RuntimeWindow[] windows = Windows;
                for (int i = 0; i < windows.Length; ++i)
                {
                    windows[i].HandleResize();
                }
            }
        }

        private void OnTabBeginDrag(Region region)
        {
            m_isTabDragInProgress = true;
        }

        private void OnTabEndDrag(Region region)
        {
            m_isTabDragInProgress = false;
        }

        private void OnRegionDisabled(Region region)
        {
            if (region.ActiveContent != null)
            {
                List<Transform> extraComponents;
                if (m_extraComponents.TryGetValue(region.ActiveContent, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
                    {
                        Transform extraComponent = extraComponents[i];
                        if (extraComponent)
                        {
                            extraComponent.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        private void OnRegionEnabled(Region region)
        {
            if (region.ActiveContent != null)
            {
                List<Transform> extraComponents;
                if (m_extraComponents.TryGetValue(region.ActiveContent, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
                    {
                        Transform extraComponent = extraComponents[i];
                        extraComponent.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnRegionMaximized(Region region, bool maximized)
        {
            if (!maximized)
            {
                RuntimeWindow[] windows = m_dockPanel.RootRegion.GetComponentsInChildren<RuntimeWindow>();
                for (int i = 0; i < windows.Length; ++i)
                {
                    windows[i].HandleResize();
                }
            }
        }

        private void OnContentDestroyed(Transform content)
        {
            string windowTypeName = m_windows.Where(kvp => kvp.Value.Contains(content)).Select(kvp => kvp.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(windowTypeName))
            {
                HashSet<Transform> windowsOfType = m_windows[windowTypeName];
                windowsOfType.Remove(content);
                m_windowToType.Remove(content);

                if (windowsOfType.Count == 0)
                {
                    m_windows.Remove(windowTypeName);
                }

                List<Transform> extraComponents = new List<Transform>();
                if (m_extraComponents.TryGetValue(content, out extraComponents))
                {
                    for (int i = 0; i < extraComponents.Count; ++i)
                    {
                        Destroy(extraComponents[i].gameObject);
                    }
                }

                bool isDialog;
                WindowDescriptor wd = m_windowManager.GetWindowDescriptor(windowTypeName, out isDialog);
                if (wd != null)
                {
                    wd.Created--;
                    Debug.Assert(wd.Created >= 0);

                    if (WindowDestroyed != null)
                    {
                        WindowDestroyed(content);
                    }
                }
            }

            RenderTextureCamera[] rtc = FindObjectsOfType<RenderTextureCamera>();
            for (int i = 0; i < rtc.Length; ++i)
            {
                if (rtc[i] != null)
                {
                    rtc[i].TryResizeRenderTexture();
                }
            }

            ForceLayoutUpdate();
            if (m_coForceUpdateLayout == null && !m_lockUpdateLayout && gameObject.activeSelf)
            {
                m_coForceUpdateLayout = CoForceUpdateLayout();
                StartCoroutine(m_coForceUpdateLayout);
            }
        }

        private IEnumerator m_coForceUpdateLayout;
        private IEnumerator CoForceUpdateLayout()
        {
            yield return new WaitForEndOfFrame();
            m_coForceUpdateLayout = null;
            ForceLayoutUpdate();
        }

        private void CancelIfRegionIsNotActive(Region region, CancelArgs arg)
        {
            if (m_editor.ActiveWindow == null)
            {
                return;
            }

            Region activeRegion = m_editor.ActiveWindow.GetComponentInParent<Region>();
            if (activeRegion == null)
            {
                return;
            }

            if (!region.IsModal() && activeRegion.GetDragRegion() != region.GetDragRegion())
            {
                arg.Cancel = true;
            }
        }

        private void OnRegionBeforeBeginDrag(Region region, CancelArgs arg)
        {
            CancelIfRegionIsNotActive(region, arg);
        }

        private void OnRegionBeforeDepthChanged(Region region, CancelArgs arg)
        {
            CancelIfRegionIsNotActive(region, arg);
        }

        private void OnRegionDepthChanged(Region region, int depth)
        {
            RuntimeWindow[] windows = region.GetComponentsInChildren<RuntimeWindow>(true);
            for (int i = 0; i < windows.Length; ++i)
            {
                RuntimeWindow window = windows[i];
                if(window is RuntimeCameraWindow)
                {
                    RuntimeCameraWindow cameraWindow = (RuntimeCameraWindow)window;
                    cameraWindow.SetCameraDepth(10 + depth * 5);
                }
                
                window.Depth = (region.IsModal() ? 2048 + depth : depth) * 5;
                if (window.GetComponentsInChildren<RuntimeWindow>().Length > 1)
                {
                    window.Depth -= 1;
                }
            }
        }


        [Obsolete("Use ILayoutStorageModel.LayoutExists")]
        public bool LayoutExist(string name)
        {
            return m_layoutStorage.LayoutExists(name);
        }

        [Obsolete("Use GetLayout in combination with ILayoutStorageModel.SaveLayout")]
        public void SaveLayout(string name)
        {
            m_layoutStorage.SaveLayout(name, GetLayout());
        }

        [Obsolete("Use ILayoutStorageModel.GetLayout")]
        public LayoutInfo GetLayout(string name, GameObject tabPrefab = null)
        {
            return m_layoutStorage.LoadLayout(name);
        }

        [Obsolete("Use SetLayout in combination with ILayoutStorageModel.GetLayout")]
        public void LoadLayout(string name, GameObject tabPrefab = null)
        {
            ClearRegion(DockPanel.RootRegion);
            foreach (Transform child in DockPanel.Free)
            {
                Region region = child.GetComponent<Region>();
                ClearRegion(region);
                Destroy(region.gameObject);
            }

            LayoutInfo layoutInfo = GetLayout(name, tabPrefab);
            if (layoutInfo == null)
            {
                return;
            }

            SetLayout(wm => layoutInfo);

            RuntimeWindow[] windows = Windows;
            for (int i = 0; i < windows.Length; ++i)
            {
                windows[i].EnableRaycasts();
                windows[i].HandleResize();
            }
        }

        [Obsolete("Use ILayoutStorageModel.DeleteLayout")]
        public void DeleteLayout(string name)
        {
            m_layoutStorage.DeleteLayout(name);
        }
    }
}
