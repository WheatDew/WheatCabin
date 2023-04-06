using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public interface ISelectObjectDialog
    {
        Type ObjectType
        {
            get;
            set;
        }

        bool IsNoneSelected
        {
            get;
        }

        UnityObject SelectedObject
        {
            get;
        }
    }
}

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SelectObjectViewModel : HierarchicalDataViewModel<ProjectItem>, ISelectObjectDialog
    {
        private DialogViewModel m_parentDialog;

        [Binding]
        public DialogViewModel ParentDialog
        {
            get
            {
                if (m_parentDialog == null)
                {
                    m_parentDialog = new DialogViewModel();
                }
                return m_parentDialog;
            }
        }

        #region ISelectObjectDialog
        public Type ObjectType
        {
            get;
            set;
        }

        public bool IsNoneSelected
        {
            get;
            protected set;
        }

        protected UnityObject m_selectedUnityObject;
        UnityObject ISelectObjectDialog.SelectedObject
        {
            get { return m_selectedUnityObject; }
        }
        #endregion

        private bool m_isAssetsTabSelected = true;
        
        [Binding]
        public bool IsAssetsTabSelected
        {
            get { return m_isAssetsTabSelected; }
            set
            {
                if(m_isAssetsTabSelected != value)
                {
                    m_isAssetsTabSelected = value;
                    OnAssetsTabSelectionChanged();
                }
            }
        }

        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;
                    RaisePropertyChanged(nameof(FilterText));
                    BindData();
                }
            }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        private ProjectItem[] m_items;
        private ProjectItem[] m_assetsCache;
        private ProjectItem[] m_sceneCache;
        private Dictionary<object, UnityObject> m_sceneObjects;
        private Guid m_noneGuid = Guid.NewGuid();
        private bool m_previewsCreated = false;

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ISelectObjectDialog>(this);
        }

        protected override async void Start()
        {
            base.Start();

            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_SelectObjectDialog_Select", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_SelectObjectDialog_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            await LoadDataAsync();
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }
            m_project = null;

            IOC.UnregisterFallback<ISelectObjectDialog>(this);

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs e)
        {
            e.Cancel = !CanClose();
        }
        #endregion

        #region Bound UnityEvent Handlers
        protected virtual void OnAssetsTabSelectionChanged()
        {
            BindData();
        }

        protected override async void OnSelectedItemsChanged(IEnumerable<ProjectItem> unselectedObjects, IEnumerable<ProjectItem> selectedObjects)
        {
            if (!m_previewsCreated)
            {
                return;
            }
            ProjectItem assetItem = selectedObjects.FirstOrDefault();
            await HandleSelectionChangedAsync(assetItem);
        }

        public override async void OnItemDoubleClick()
        {
            if (!m_previewsCreated)
            {
                return;
            }

            ProjectItem assetItem = TargetItem;
            if (assetItem != null && assetItem.GetTypeGuid() == m_noneGuid)
            {
                IsNoneSelected = true;
                m_selectedUnityObject = null;
                m_parentDialog?.Close(true);
            }
            else
            {
                IsNoneSelected = false;
                if (assetItem != null)
                {
                    if (m_sceneObjects.ContainsKey(m_project.Utils.ToPersistentID(assetItem)))
                    {
                        m_selectedUnityObject = m_sceneObjects[m_project.Utils.ToPersistentID(assetItem)];
                        m_parentDialog?.Close(true);
                    }
                    else
                    {
                        m_selectedUnityObject = null;
                        Editor.IsBusy = true;
                        UnityObject[] obj = await m_project.Safe.LoadAsync(new[] { assetItem });
                        Editor.IsBusy = false;
                        m_selectedUnityObject = obj[0];
                        m_parentDialog?.Close(true);
                    }
                }
                else
                {
                    m_selectedUnityObject = null;
                    m_parentDialog?.Close(true);
                }
            }
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanDrag;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanUnselectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;
            flags &= ~HierarchicalDataFlags.CanEdit;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectItem item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<ProjectItem> GetChildren(ProjectItem parent)
        {
            ApplyFilter();
            return m_items;
        }
        #endregion

        #region Methods

        protected virtual async Task LoadDataAsync()
        {
            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            ProjectItem[] assetItems = m_project.State.RootFolder.Flatten(true, false).Where(item =>
            {
                Type type = m_project.Utils.ToType(item);
                if (type == null)
                {
                    return false;
                }
                return type == ObjectType || type.IsSubclassOf(ObjectType);

            }).ToArray();

            ParentDialog.IsOkInteractable = false;

            byte[][] previews;
            try
            {
                Editor.IsBusy = true;
                previews = await m_project.Safe.GetPreviewsAsync(assetItems);
            }
            catch (Exception e)
            {
                WindowManager.MessageBox(Localization.GetString("ID_RTEditor_SelectObjectDialog_CantGetAssets", "Can't GetAssets"), e.Message);
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

            ProjectItem none = m_project.Utils.CreateAssetItem(m_noneGuid, Localization.GetString("ID_RTEditor_SelectObjectDialog_None", "None"));
            assetItems = new[] { none }.Union(assetItems).ToArray();
            m_assetsCache = assetItems;

            BindData();

            m_previewsCreated = false;
            TaskCompletionSource<object> waitForPreviews = new TaskCompletionSource<object>();
            StartCoroutine(previewUtil.CoCreatePreviews(assetItems, () =>
            {
                waitForPreviews.SetResult(true);
            }));

            await waitForPreviews.Task;

            m_previewsCreated = true;
            await HandleSelectionChangedAsync(SelectedItem);

            ParentDialog.IsOkInteractable = SelectedItem != null;
            Editor.IsBusy = false;
            
            m_sceneObjects = new Dictionary<object, UnityObject>();
            List<ProjectItem> sceneCache = new List<ProjectItem>();
            sceneCache.Add(none);
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

        protected virtual async Task HandleSelectionChangedAsync(ProjectItem assetItem)
        {
            if (assetItem != null && assetItem.GetTypeGuid() == m_noneGuid)
            {
                IsNoneSelected = true;
                m_selectedUnityObject = null;
            }
            else
            {
                IsNoneSelected = false;

                if (assetItem != null)
                {
                    if (m_sceneObjects.ContainsKey(m_project.Utils.ToPersistentID(assetItem)))
                    {
                        m_selectedUnityObject = m_sceneObjects[m_project.Utils.ToPersistentID(assetItem)];
                    }
                    else
                    {
                        m_selectedUnityObject = null;
                        UnityObject[] obj = await m_project.Safe.LoadAsync(new[] { assetItem });
                        m_selectedUnityObject = obj[0];
                    }
                }
                else
                {
                    m_selectedUnityObject = null;
                }
            }

            ParentDialog.IsOkInteractable = SelectedItem != null;
        }

        protected virtual bool CanClose()
        {
            return SelectedItem != null || IsNoneSelected;
        }

        protected virtual void ApplyFilter()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                m_items = IsAssetsTabSelected ? m_assetsCache : m_sceneCache;
                if(m_items != null)
                {
                    m_items = m_items.ToArray();
                }
            }
            else
            {
                ProjectItem[] cache = IsAssetsTabSelected ? m_assetsCache : m_sceneCache;
                m_items = cache != null ? cache.Where(Filter).ToArray() : null;
            }
        }

        protected bool Filter(ProjectItem item)
        {
            return item.Name.ToLower().Contains(FilterText.ToLower());
        }

        #endregion
    }
}
