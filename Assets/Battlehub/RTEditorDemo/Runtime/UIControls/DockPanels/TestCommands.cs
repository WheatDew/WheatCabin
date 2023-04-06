using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;

namespace Battlehub.UIControls.DockPanels
{
    public class TestTabDelegate : ITabDelegate
    {
        private DialogManager m_dm;
        
        public TestTabDelegate(DialogManager dialogManager)
        {
            m_dm = dialogManager;
        }

        public void OnTabAttemptClose(Tab tab)
        {
            Dialog dlg = m_dm.ShowDialog(null, "Confirmation", string.Format("Are you sure you want to close {0} tab", tab.Text), (sender, okArgs) =>
            {
                tab.Close();
            }, "Yes", (sender, cancelArgs) =>
            {
                
            }, "No");

            dlg.IsOkVisible = true;
            dlg.IsCancelVisible = true;
        }

        public void OnTabClosing(Tab tab)
        {
            
        }

        public void OnTabVisible(Tab tab, bool isVisible)
        {
            
        }
    }


    public class TestCommands : MonoBehaviour
    {
        [SerializeField]
        private DialogManager m_dialog = null;

        [SerializeField]
        private Button m_defaultLayout = null;

        [SerializeField]
        private Button m_addButton = null;

        [SerializeField]
        private Button m_deleteButton = null;

        [SerializeField]
        private Button m_showPopup = null;

        [SerializeField]
        private Button m_showDialog = null;

        [SerializeField]
        private Button m_showMsgBox = null;

        [SerializeField]
        private DockPanel m_dockPanels = null;

        [SerializeField]
        private Sprite m_sprite = null;

        [SerializeField]
        private string m_headerText = null;

        [SerializeField]
        private Transform m_contentPrefab = null;

        [SerializeField]
        private RegionSplitType m_splitType = RegionSplitType.None;

        private int m_counter;

        private void Awake()
        {
            m_defaultLayout.onClick.AddListener(OnDefaultLayout);
            m_addButton.onClick.AddListener(OnAddClick);
            m_deleteButton.onClick.AddListener(OnDeleteClick);
            m_showPopup.onClick.AddListener(OnShowPopup);
            m_showDialog.onClick.AddListener(OnShowDialog);
            m_showMsgBox.onClick.AddListener(OnShowMsgBox);   
        }

        private void Start()
        {
            OnDefaultLayout();
        }

        private void OnDestroy()
        {
            if(m_defaultLayout != null)
            {
                m_defaultLayout.onClick.RemoveListener(OnDefaultLayout);
            }

            if(m_addButton != null)
            {
                m_addButton.onClick.RemoveListener(OnAddClick);
            }
            
            if(m_deleteButton != null)
            {
                m_deleteButton.onClick.RemoveListener(OnDeleteClick);
            }

            if(m_showDialog != null)
            {
                m_showDialog.onClick.RemoveListener(OnShowDialog);
            }

            if(m_showMsgBox != null)
            {
                m_showMsgBox.onClick.RemoveListener(OnShowMsgBox);
            }

            if(m_showDialog != null)
            {
                m_showDialog.onClick.RemoveListener(OnShowDialog);
            }
        }

        private void SelectRegionIfRequired()
        {
            if (m_dockPanels.SelectedRegion == null || !m_dockPanels.SelectedRegion.CanAdd())
            {
                Region leafRegion = m_dockPanels.GetComponentsInChildren<Region>().Where(r => r.ChildrenPanel.childCount == 0).First();
                leafRegion.IsSelected = true;
            }
        }

        private void OnAddClick()
        {
            SelectRegionIfRequired();

            if (m_dockPanels.SelectedRegion != null)
            {
                m_counter++;

                Transform content = Instantiate(m_contentPrefab);

                Tab tab = Instantiate(m_dockPanels.TabPrefab);
                tab.Icon = m_sprite;
                tab.Text = m_headerText + " " + m_counter;
                tab.TabDelegate = new TestTabDelegate(m_dialog);

                m_dockPanels.SelectedRegion.Add(tab, content, false, m_splitType);
            }
        }

        private void OnDeleteClick()
        {
            SelectRegionIfRequired();

            if (m_dockPanels.SelectedRegion != null)
            {
                Region region = m_dockPanels.SelectedRegion;
                region.RemoveAt(region.ActiveTabIndex);
            }
        }

        private void OnShowPopup()
        {
            m_counter++;

            Transform content = Instantiate(m_contentPrefab);

            Tab tab = Instantiate(m_dockPanels.TabPrefab);
            tab.Icon = m_sprite;
            tab.Text = m_headerText + " " + m_counter;
            
            m_dockPanels.AddPopupRegion(tab, content, new Rect(0, 0, 200, 200), new Vector2(0.5f, 0.5f));

            Region.FindRegion(content).Fit(false);
        }

        private void OnShowDialog()
        {
            m_counter++;

            Transform content = Instantiate(m_contentPrefab);             
            Dialog dlg = m_dialog.ShowDialog(m_sprite, "Modal Dialog " + m_counter, content, (sender, okArgs) =>
            {
                Debug.Log("YES");

            }, "Yes", (sender, cancelArgs) =>
            {
                Debug.Log("NO");
            }, "No");

            dlg.IsOkVisible = true;
            dlg.IsCancelVisible = true;
        }

        private void OnShowMsgBox()
        {
            m_dialog.ShowDialog(m_sprite, "Msg Test", "Your message", (sender, okArgs) =>
            {
                Debug.Log("YES");
                OnShowMsgBox();
                okArgs.Cancel = true;

            }, "Yes", (sender, cancelArgs) =>
            {
                Debug.Log("NO");
            }, "No");
        }


        private void OnDefaultLayout()
        {
            Region rootRegion = m_dockPanels.RootRegion;
            rootRegion.CloseAllTabs();
            foreach (Transform child in m_dockPanels.Free)
            {
                Region region = child.GetComponent<Region>();
                region.Destroy();
            }

            LayoutInfo layout = new LayoutInfo(false,
                CreateLayoutInfo(Instantiate(m_contentPrefab).transform, m_headerText + " " + m_counter++, m_sprite),
                new LayoutInfo(true,
                    CreateLayoutInfo(Instantiate(m_contentPrefab).transform, m_headerText + " " + m_counter++, m_sprite),
                    CreateLayoutInfo(Instantiate(m_contentPrefab).transform, m_headerText + " " + m_counter++, m_sprite),
                    0.5f),
                0.75f);

            m_dockPanels.RootRegion.Build(layout);
        }

        private LayoutInfo CreateLayoutInfo(Transform content, string header, Sprite icon)
        {
            Tab tab = Instantiate(m_dockPanels.TabPrefab);
            tab.Icon = icon;
            tab.Text = header;

            return new LayoutInfo(content, tab);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveLayout("Default");
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                LoadLayout("Default");
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (m_dockPanels.RootRegion != null)
                {
                    m_dockPanels.RootRegion.CloseAllTabs();
                }
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (m_dockPanels.SelectedRegion != null)
                {
                    m_dockPanels.SelectedRegion.Destroy();
                }
            }
        }

        public bool LayoutExist(string name)
        {
            return PlayerPrefs.HasKey("Battlehub.DockPanel.Layout" + name);
        }

        public void SaveLayout(string name)
        {
            PersistentLayoutInfo layoutInfo = new PersistentLayoutInfo();
            ToPersistentLayout(m_dockPanels.RootRegion, layoutInfo);

            string serializedLayout = XmlUtility.ToXml(layoutInfo);
            PlayerPrefs.SetString("Battlehub.DockPanel.Layout" + name, serializedLayout);
            PlayerPrefs.Save();
        }

        private void ToPersistentLayout(Region region, PersistentLayoutInfo layoutInfo)
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

                layoutInfo.Child0 = new PersistentLayoutInfo();
                layoutInfo.Child1 = new PersistentLayoutInfo();

                ToPersistentLayout(childRegion0, layoutInfo.Child0);
                ToPersistentLayout(childRegion1, layoutInfo.Child1);
            }
            else
            {
                if (region.ContentPanel.childCount > 1)
                {
                    layoutInfo.TabGroup = new PersistentLayoutInfo[region.ContentPanel.childCount];
                    for (int i = 0; i < region.ContentPanel.childCount; ++i)
                    {
                        Transform content = region.ContentPanel.GetChild(i);

                        PersistentLayoutInfo tabLayout = new PersistentLayoutInfo();
                        ToPersistentLayout(region, content, tabLayout);
                        layoutInfo.TabGroup[i] = tabLayout;
                    }
                }
                else if (region.ContentPanel.childCount == 1)
                {
                    Transform content = region.ContentPanel.GetChild(0);
                    ToPersistentLayout(region, content, layoutInfo);
                }
            }
        }

        private void ToPersistentLayout(Region region, Transform content, PersistentLayoutInfo layoutInfo)
        {
            layoutInfo.WindowType = "ContentKey";

            Tab tab = Region.FindTab(content);
            if (tab != null)
            {
                layoutInfo.CanDrag = tab.CanDrag;
                layoutInfo.CanClose = tab.CanClose;
            }
            layoutInfo.IsHeaderVisible = region.IsHeaderVisible;

        }

        public LayoutInfo GetLayout(string name)
        {
            string serializedLayout = PlayerPrefs.GetString("Battlehub.DockPanel.Layout" + name);
            if (serializedLayout == null)
            {
                Debug.LogWarningFormat("Layout {0} does not exist ", name);
                return null;
            }

            PersistentLayoutInfo persistentLayoutInfo = XmlUtility.FromXml<PersistentLayoutInfo>(serializedLayout);
            LayoutInfo layoutInfo = new LayoutInfo();
            ToLayout(persistentLayoutInfo, layoutInfo);
            return layoutInfo;
        }

        public void LoadLayout(string name)
        {
            LayoutInfo layoutInfo = GetLayout(name);
            if (layoutInfo == null)
            {
                return;
            }

            m_dockPanels.RootRegion.Build(layoutInfo);
        }

        private void ToLayout(PersistentLayoutInfo persistentLayoutInfo, LayoutInfo layoutInfo)
        {
            if (!string.IsNullOrEmpty(persistentLayoutInfo.WindowType))
            {
                Transform content = Instantiate(m_contentPrefab);
                Tab tab = Instantiate(m_dockPanels.TabPrefab);
                tab.Text = persistentLayoutInfo.WindowType;

                layoutInfo.Tab = tab;
                layoutInfo.Content = content.transform;
                layoutInfo.CanDrag = persistentLayoutInfo.CanDrag;
                layoutInfo.CanClose = persistentLayoutInfo.CanClose;
                layoutInfo.CanMaximize = persistentLayoutInfo.CanMaximize;
                layoutInfo.IsHeaderVisible = persistentLayoutInfo.IsHeaderVisible;
            }
            else
            {
                if (persistentLayoutInfo.TabGroup != null)
                {
                    layoutInfo.TabGroup = new LayoutInfo[persistentLayoutInfo.TabGroup.Length];
                    for (int i = 0; i < persistentLayoutInfo.TabGroup.Length; ++i)
                    {
                        LayoutInfo tabLayoutInfo = new LayoutInfo();
                        ToLayout(persistentLayoutInfo.TabGroup[i], tabLayoutInfo);
                        layoutInfo.TabGroup[i] = tabLayoutInfo;
                    }
                }
                else
                {
                    layoutInfo.IsVertical = persistentLayoutInfo.IsVertical;
                    layoutInfo.Child0 = new LayoutInfo();
                    layoutInfo.Child1 = new LayoutInfo();
                    layoutInfo.Ratio = persistentLayoutInfo.Ratio;

                    ToLayout(persistentLayoutInfo.Child0, layoutInfo.Child0);
                    ToLayout(persistentLayoutInfo.Child1, layoutInfo.Child1);
                }
            }
        }
    }
}
