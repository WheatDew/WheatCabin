using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTEditor.Models
{
    public abstract class BaseLayoutStorageModel : MonoBehaviour, ILayoutStorageModel
    {
        public abstract string DefaultLayoutName
        {
            get;
        }

        public abstract bool LayoutExists(string path);

        public abstract LayoutInfo LoadLayout(string path);

        public abstract void SaveLayout(string path, LayoutInfo layout);

        public abstract string[] GetLayouts();

        public abstract void DeleteLayout(string path);

        protected PersistentLayoutInfo ToPersistentLayout(LayoutInfo layout)
        {
            PersistentLayoutInfo persistentLayoutInfo = new PersistentLayoutInfo();
            ToPersistentLayout(layout, persistentLayoutInfo);
            return persistentLayoutInfo;
        }

        protected LayoutInfo ToLayout(PersistentLayoutInfo persistentLayoutInfo)
        {
            LayoutInfo layout = new LayoutInfo();
            ToLayout(persistentLayoutInfo, layout, null);
            return layout;
        }

        private void ToPersistentLayout(LayoutInfo layoutInfo, PersistentLayoutInfo persistentLayoutInfo)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            string windowType = wm.GetWindowTypeName(layoutInfo.Content);

            if (windowType != null)
            {
                persistentLayoutInfo.WindowType = windowType;
                persistentLayoutInfo.Args = layoutInfo.Args;
                persistentLayoutInfo.CanDrag = layoutInfo.CanDrag;
                persistentLayoutInfo.CanClose = layoutInfo.CanClose;
                persistentLayoutInfo.CanMaximize = layoutInfo.CanMaximize;
                persistentLayoutInfo.IsHeaderVisible = layoutInfo.IsHeaderVisible;
                persistentLayoutInfo.IsOn = layoutInfo.IsOn;
            }
            else
            {
                if (layoutInfo.TabGroup != null)
                {
                    persistentLayoutInfo.TabGroup = new PersistentLayoutInfo[layoutInfo.TabGroup.Length];
                    for (int i = 0; i < layoutInfo.TabGroup.Length; ++i)
                    {
                        PersistentLayoutInfo tabLayoutInfo = new PersistentLayoutInfo();
                        ToPersistentLayout(layoutInfo.TabGroup[i], tabLayoutInfo);
                        persistentLayoutInfo.TabGroup[i] = tabLayoutInfo;
                    }
                }
                else
                {
                    persistentLayoutInfo.IsVertical = layoutInfo.IsVertical;
                    if (layoutInfo.Child0 != null && layoutInfo.Child0 != null)
                    {
                        persistentLayoutInfo.Child0 = new PersistentLayoutInfo();
                        persistentLayoutInfo.Child1 = new PersistentLayoutInfo();
                        persistentLayoutInfo.Ratio = layoutInfo.Ratio;

                        ToPersistentLayout(layoutInfo.Child0, persistentLayoutInfo.Child0);
                        ToPersistentLayout(layoutInfo.Child1, persistentLayoutInfo.Child1);
                    }
                }
            }
        }

        private void ToLayout(PersistentLayoutInfo persistentLayoutInfo, LayoutInfo layoutInfo, GameObject tabPrefab)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (!string.IsNullOrEmpty(persistentLayoutInfo.WindowType))
            {
                WindowDescriptor wd;
                GameObject content;
                wm.CreateWindow(persistentLayoutInfo.WindowType, out wd, out content, out _);
                Tab prefab = tabPrefab != null ? tabPrefab.GetComponent<Tab>() : null;
                Tab tab;
                if (prefab != null)
                {
                    tab = Instantiate(prefab);
                }
                else
                {
                    tab = GetTabPrefab(wd);
                    tab = Instantiate(tab);
                }

                tab.name = "Tab";
                if (content == null)
                {
                    tab.Text = "Empty";
                    layoutInfo.Content = new GameObject("Empty").AddComponent<RectTransform>();
                    layoutInfo.Tab = tab;
                }
                else
                {
                    tab.Text = wd.Header;
                    tab.Icon = wd.Icon;
                    layoutInfo.Content = content.transform;
                    layoutInfo.Tab = tab;
                    layoutInfo.Args = persistentLayoutInfo.Args;
                    layoutInfo.CanDrag = persistentLayoutInfo.CanDrag;
                    layoutInfo.CanClose = persistentLayoutInfo.CanClose;
                    layoutInfo.CanMaximize = persistentLayoutInfo.CanMaximize;
                    layoutInfo.IsHeaderVisible = persistentLayoutInfo.IsHeaderVisible;
                    layoutInfo.IsOn = persistentLayoutInfo.IsOn;

                    RuntimeWindow window = content.GetComponentInChildren<RuntimeWindow>(true);
                    if (window != null)
                    {
                        window.Args = persistentLayoutInfo.Args;
                    }
                }
            }
            else
            {
                if (persistentLayoutInfo.TabGroup != null)
                {
                    layoutInfo.TabGroup = new LayoutInfo[persistentLayoutInfo.TabGroup.Length];
                    for (int i = 0; i < persistentLayoutInfo.TabGroup.Length; ++i)
                    {
                        LayoutInfo tabLayoutInfo = new LayoutInfo();
                        ToLayout(persistentLayoutInfo.TabGroup[i], tabLayoutInfo, tabPrefab);
                        layoutInfo.TabGroup[i] = tabLayoutInfo;
                    }
                }
                else
                {
                    layoutInfo.IsVertical = persistentLayoutInfo.IsVertical;
                    if (persistentLayoutInfo.Child0 != null && persistentLayoutInfo.Child0 != null)
                    {
                        layoutInfo.Child0 = new LayoutInfo();
                        layoutInfo.Child1 = new LayoutInfo();
                        layoutInfo.Ratio = persistentLayoutInfo.Ratio;

                        ToLayout(persistentLayoutInfo.Child0, layoutInfo.Child0, tabPrefab);
                        ToLayout(persistentLayoutInfo.Child1, layoutInfo.Child1, tabPrefab);
                    }
                }
            }
        }

        private Tab GetTabPrefab(WindowDescriptor wd)
        {
            if (wd.TabPrefab != null)
            {
                return wd.TabPrefab.GetComponent<Tab>();
            }
            return null;
        }
    }


    public class PlayerPrefsLayoutStorageModel : BaseLayoutStorageModel
    {
        [SerializeField]
        private string m_keyPrefix = null;

        private string m_layoutsListKey = "{0}.Battlehub.RTEditor.Layout.LayoutsList";
        private HashSet<string> m_layouts;

        [SerializeField]
        private string m_defaultLayoutName = "DefaultLayout";
        public override string DefaultLayoutName
        {
            get { return m_defaultLayoutName; }
        }

        private void Awake()
        {
            if(string.IsNullOrEmpty(m_keyPrefix))
            {
                m_keyPrefix = SceneManager.GetActiveScene().name;
            }

            if (PlayerPrefs.HasKey(string.Format(m_layoutsListKey, m_keyPrefix)))
            {
                string layouts = PlayerPrefs.GetString(string.Format(m_layoutsListKey, m_keyPrefix));
                m_layouts = layouts != null ? new HashSet<string>(layouts.Split(';')) : new HashSet<string>();
            }
            else
            {
                m_layouts = new HashSet<string>();
            }
        }

        public override bool LayoutExists(string path)
        {
            return PlayerPrefs.HasKey($"{m_keyPrefix}Battlehub.RTEditor.Layout{path}");
        }

        public override string[] GetLayouts()
        {
            return m_layouts.ToArray();
        }

        public override LayoutInfo LoadLayout(string path)
        {
            string serializedLayout = PlayerPrefs.GetString($"{m_keyPrefix}Battlehub.RTEditor.Layout{path}");
            PersistentLayoutInfo persistentLayoutInfo = XmlUtility.FromXml<PersistentLayoutInfo>(serializedLayout);

            LayoutInfo layout = ToLayout(persistentLayoutInfo);
            return layout;
        }

        public override void SaveLayout(string path, LayoutInfo layout)
        {
            PersistentLayoutInfo persistentLayoutInfo = ToPersistentLayout(layout);

            string serializedLayout = XmlUtility.ToXml(persistentLayoutInfo);

            PlayerPrefs.SetString($"{m_keyPrefix}Battlehub.RTEditor.Layout{path}", serializedLayout);
            PlayerPrefs.Save();

            m_layouts.Add($"{m_keyPrefix}.Battlehub.RTEditor.Layout.{path}");
            PlayerPrefs.SetString(string.Format(m_layoutsListKey, m_keyPrefix), string.Join(";", m_layouts));
        }


        public override void DeleteLayout(string path)
        {
            PlayerPrefs.DeleteKey($"{m_keyPrefix}Battlehub.RTEditor.Layout{path}");
        }
    }
}
