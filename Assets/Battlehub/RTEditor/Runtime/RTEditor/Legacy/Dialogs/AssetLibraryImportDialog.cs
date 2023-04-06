using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class AssetLibraryImportDialog : RuntimeWindow
    {
        [SerializeField]
        private VirtualizingTreeView TreeViewPrefab = null;

        [SerializeField]
        private GameObject m_txtNoItemsToImport = null;

        private Dialog m_parentDialog;
        private VirtualizingTreeView m_treeView;

        private IProjectAsync m_project;

        private bool m_isBuiltIn;
        public bool IsBuiltIn
        {
            set { m_isBuiltIn = value; }
        }

        private string m_selectedLibrary;
        public string SelectedLibrary
        {
            set { m_selectedLibrary = value; }
        }

        private ProjectItem[] SelectedAssets
        {
            get { return m_treeView.SelectedItems.OfType<ProjectItem>().Where(p => !p.IsFolder).ToArray(); }
        }

        private ProjectItem m_root;
        public ProjectItem AssetLibraryRoot
        {
            get { return m_root; }
        }

        private IEnumerator m_coCreatePreviews;

        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ImportAssets;
            base.AwakeOverride();
            m_localization = IOC.Resolve<ILocalization>();
        }

        private async void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_AssetLibImportDialog_Btn_Import", "Import");
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_AssetLibImportDialog_Btn_Cancel", "Cancel");
            m_parentDialog.Ok += OnOk;
            m_parentDialog.Cancel += OnCancel;

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            if (m_treeView == null)
            {
                m_treeView = Instantiate(TreeViewPrefab, transform);
                m_treeView.name = "ImportAssetsTreeView";
            }

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemExpanding += OnItemExpanding;

            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;

            m_project = IOC.Resolve<IProjectAsync>();

            Editor.IsBusy = true;

            ProjectItem root = await m_project.Safe.LoadImportItemsAsync(m_selectedLibrary, m_isBuiltIn);

            m_root = root;

            if (root != null && root.Children != null && root.Children.Count > 0)
            {
                if (m_txtNoItemsToImport != null)
                {
                    m_txtNoItemsToImport.SetActive(false);
                }
                m_treeView.gameObject.SetActive(true);
                m_treeView.Items = new[] { root };
                m_treeView.SelectedItems = root.Flatten(false);
                ExpandAll(root);

                IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();

                m_coCreatePreviews = resourcePreview.CoCreatePreviews(root.Flatten(false), () =>
                {
                    m_project.UnloadImportItems(root);
                    Editor.IsBusy = false;
                    m_coCreatePreviews = null;
                });

                StartCoroutine(m_coCreatePreviews);
            }
            else
            {
                Editor.IsBusy = false;

                m_parentDialog.IsOkInteractable = false;

                if (m_txtNoItemsToImport != null)
                {
                    m_txtNoItemsToImport.SetActive(true);
                }

                m_treeView.gameObject.SetActive(false);
            }

        }

        private void ExpandAll(ProjectItem item)
        {
            if (item.Children != null)
            {
                m_treeView.Expand(item);

                foreach (ProjectItem child in item.Children)
                {
                    ExpandAll(child);
                }
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog.Cancel -= OnCancel;
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemExpanding -= OnItemExpanding;
            }

            if (m_coCreatePreviews != null)
            {
                StopCoroutine(m_coCreatePreviews);
                m_coCreatePreviews = null;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item.Name;

                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = item;

                Toggle toogle = e.ItemPresenter.GetComponentInChildren<Toggle>(true);
                toogle.isOn = m_treeView.IsItemSelected(item);

                AssetLibraryImportStatus status = e.ItemPresenter.GetComponentInChildren<AssetLibraryImportStatus>(true);
                if (item is ImportAssetItem)
                {
                    ImportAssetItem importItem = (ImportAssetItem)item;
                    status.Current = importItem.Status;
                }
                else
                {
                    status.Current = ImportStatus.None;
                }

                e.HasChildren = item.Children != null && item.Children.Count > 0;
            }
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ProjectItem item = (ProjectItem)e.Item;
            e.Children = item.Children;
        }


        private async void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if (Editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }
            if (m_treeView.SelectedItemsCount == 0)
            {
                args.Cancel = true;
                return;
            }

            Editor.IsBusy = true;
            await m_project.Safe.ImportAsync(SelectedAssets);
            Editor.IsBusy = false;
        }

        private void OnCancel(Dialog sender, DialogCancelArgs args)
        {
            if (Editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }

            if (m_coCreatePreviews != null)
            {
                StopCoroutine(m_coCreatePreviews);
                m_coCreatePreviews = null;
            }

            if (m_treeView.Items != null)
            {
                m_project.UnloadImportItems(m_treeView.Items.OfType<ProjectItem>().FirstOrDefault());
            }
            //else
            //{
            //    Debug.LogWarning("m_treeView.Items == null");
            //}
        }
    }
}
