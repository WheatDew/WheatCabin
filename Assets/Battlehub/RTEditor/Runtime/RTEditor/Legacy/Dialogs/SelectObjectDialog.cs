using System;
using UnityEngine;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTSL.Interface;

using UnityObject = UnityEngine.Object;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class SelectObjectDialog : RuntimeWindow, ISelectObjectDialog
    {
        [SerializeField]
        private TMP_InputField m_filter = null;
        [SerializeField]
        private VirtualizingTreeView m_treeView = null;
        [SerializeField]
        private Toggle m_toggleAssets = null;

        public UnityObject SelectedObject
        {
            get;
            private set;
        }

        public Type ObjectType
        {
            get;
            set;
        }

        public bool IsNoneSelected
        {
            get;
            private set;
        }

        private Dialog m_parentDialog;
        private IWindowManager m_windowManager;
        private IProjectAsync m_project;
        private ILocalization m_localization;

        private Guid m_noneGuid = Guid.NewGuid();
        private bool m_previewsCreated;
        private ProjectItem[] m_assetsCache;
        private ProjectItem[] m_sceneCache;
        private Dictionary<object, UnityObject> m_sceneObjects;

        protected override void AwakeOverride()
        {
            IOC.RegisterFallback<ISelectObjectDialog>(this);
            WindowType = RuntimeWindowType.SelectObject;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
        }

        private async void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SelectObjectDialog_Select", "Select");
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SelectObjectDialog_Cancel", "Cancel");
            m_parentDialog.Ok += OnOk;

            m_toggleAssets.onValueChanged.AddListener(OnAssetsTabSelectionChanged);

            m_project = IOC.Resolve<IProjectAsync>();
            m_windowManager = IOC.Resolve<IWindowManager>();

            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            ProjectItem[] assetItems = m_project.State.RootFolder.Flatten(true, false).Where(item =>
            {
                Type type = m_project.Utils.ToType(item);
                if(type == null)
                {
                    return false;
                }
                return type == ObjectType || type.IsSubclassOf(ObjectType);

            }).ToArray();

            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDataBinding += OnItemDataBinding;
            Editor.IsBusy = true;
            m_parentDialog.IsOkInteractable = false;
            byte[][] previews;
            try
            {
                previews = await m_project.Safe.GetPreviewsAsync(assetItems);
            }
            catch(Exception e)
            {
                m_windowManager.MessageBox(m_localization.GetString("ID_RTEditor_SelectObjectDialog_CantGetAssets", "Can't GetAssets"), e.Message);
                Debug.LogException(e);
                return;
            }
            finally
            {
                Editor.IsBusy = false;
            }


            for (int i = 0; i < assetItems.Length; ++i)
            {
                assetItems[i].SetPreview(previews[i]);
            }

            ProjectItem none = m_project.Utils.CreateAssetItem(m_noneGuid, m_localization.GetString("ID_RTEditor_SelectObjectDialog_None", "None"));
            assetItems = new[] { none }.Union(assetItems).ToArray();

            m_previewsCreated = false;
            StartCoroutine(previewUtil.CoCreatePreviews(assetItems, () =>
            {
                m_previewsCreated = true;
                HandleSelectionChanged((ProjectItem)m_treeView.SelectedItem);
                m_treeView.ItemDoubleClick += OnItemDoubleClick;
                m_parentDialog.IsOkInteractable = m_previewsCreated && m_treeView.SelectedItem != null;
                Editor.IsBusy = false;

                if (m_filter != null)
                {
                    if (!string.IsNullOrEmpty(m_filter.text))
                    {
                        ApplyFilter(m_filter.text);
                    }
                    m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                }
            }));

            m_assetsCache = assetItems;
            m_treeView.Items = m_assetsCache;

            List<ProjectItem> sceneCache = new List<ProjectItem>();
            sceneCache.Add(none);

            m_sceneObjects = new Dictionary<object, UnityObject>();
            ExposeToEditor[] sceneObjects = Editor.Object.Get(false, true).ToArray();
            for (int i = 0; i < sceneObjects.Length; ++i)
            {
                ExposeToEditor exposeToEditor = sceneObjects[i];
                UnityObject obj = null;
                if (ObjectType == typeof(GameObject))
                {
                    obj = exposeToEditor.gameObject;
                }
                else if (ObjectType.IsSubclassOf(typeof(Component)))
                {
                    obj = exposeToEditor.GetComponent(ObjectType);
                }

                if (obj != null)
                {
                    ProjectItem assetItem = m_project.Utils.CreateAssetItem(m_project.Utils.ToGuid(typeof(GameObject)), exposeToEditor.name, m_project.Utils.GetExt(typeof(GameObject)));
                    m_project.Utils.SetPersistentID(assetItem, m_project.Utils.ToPersistentID(exposeToEditor));
                    assetItem.ItemGUID = Guid.NewGuid();
                    assetItem.SetPreview(new byte[0]);
                    sceneCache.Add(assetItem);
                    m_sceneObjects.Add(m_project.Utils.ToPersistentID(assetItem), obj);
                }
            }
            m_sceneCache = sceneCache.ToArray();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if(m_toggleAssets != null)
            {
                m_toggleAssets.onValueChanged.RemoveListener(OnAssetsTabSelectionChanged);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDataBinding -= OnItemDataBinding;
            }

            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
            }

            IOC.UnregisterFallback<ISelectObjectDialog>(this);
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem projectItem = e.Item as ProjectItem;
            if (projectItem == null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = null;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = null;
            }
            else
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = projectItem.Name;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = projectItem;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if (!m_previewsCreated)
            {
                return;
            }

            ProjectItem assetItem = (ProjectItem)e.NewItem;
            HandleSelectionChanged(assetItem);
        }

        private async void HandleSelectionChanged(ProjectItem assetItem)
        {
            if (assetItem != null && assetItem.GetTypeGuid() == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
            }
            else
            {
                IsNoneSelected = false;

                if (assetItem != null)
                {
                    if (m_sceneObjects.ContainsKey(m_project.Utils.ToPersistentID(assetItem)))
                    {
                        SelectedObject = m_sceneObjects[m_project.Utils.ToPersistentID(assetItem)];
                    }
                    else
                    {
                        SelectedObject = null;
                        UnityObject[] obj = await m_project.Safe.LoadAsync(new[] { assetItem });
                        SelectedObject = obj[0];
                    }
                }
                else
                {
                    SelectedObject = null;
                }
            }

            m_parentDialog.IsOkInteractable = m_treeView.SelectedItem != null;
        }

        private async void OnItemDoubleClick(object sender, ItemArgs e)
        {
            ProjectItem assetItem = (ProjectItem)e.Items[0];
            if (assetItem != null && assetItem.GetTypeGuid() == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
                m_parentDialog.Close(true);
            }
            else
            {
                IsNoneSelected = false;
                if (assetItem != null)
                {
                    if(m_sceneObjects.ContainsKey(m_project.Utils.ToPersistentID(assetItem)))
                    {
                        SelectedObject = m_sceneObjects[m_project.Utils.ToPersistentID(assetItem)];
                        m_parentDialog.Close(true);
                    }
                    else
                    {
                        SelectedObject = null;
                        Editor.IsBusy = true;
                        UnityObject[] obj = await m_project.Safe.LoadAsync(new[] { assetItem });
                        Editor.IsBusy = false;
                        SelectedObject = obj[0];
                        m_parentDialog.Close(true);
                    }   
                }
                else
                {
                    SelectedObject = null;
                    m_parentDialog.Close(true);
                }
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if (SelectedObject == null && !IsNoneSelected)
            {
                args.Cancel = true;
            }
        }

        private void OnFilterValueChanged(string text)
        {
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSeconds(0.3f);

            ApplyFilterImmediately(filter);
        }

        private void ApplyFilterImmediately(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                m_treeView.Items = m_toggleAssets.isOn ? m_assetsCache : m_sceneCache;
            }
            else
            {
                ProjectItem[] cache = m_toggleAssets.isOn ? m_assetsCache : m_sceneCache;
                m_treeView.Items = cache.Where(item => item.Name.ToLower().Contains(filter.ToLower()));
            }
        }

        private void OnAssetsTabSelectionChanged(bool value)
        {
            ApplyFilterImmediately(m_filter.text);
        }
    }
}

