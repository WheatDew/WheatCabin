using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class AssetLibrarySelectDialog : RuntimeWindow
    {
        [SerializeField]
        private VirtualizingTreeView m_builtInTreeView = null;
        [SerializeField]
        private VirtualizingTreeView m_externalTreeView = null;
        [SerializeField]
        private Toggle m_builtInToggle = null;

        [SerializeField]
        private Sprite AssetLibraryIcon = null;

        private Dialog m_parentDialog;

        private IProjectAsync m_project;

        private bool IsBuiltInLibrary
        {
            get { return m_builtInToggle.isOn; }
        }

        private string SelectedLibrary
        {
            get
            {
                if(IsBuiltInLibrary)
                {
                    return m_builtInTreeView.SelectedItem as string;
                }
                return m_externalTreeView.SelectedItem as string;
            }
        }

        private IWindowManager m_windowManager;
        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SelectAssetLibrary;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
        }

        private async void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_AssetLibSelectDialog_Select", "Select");
                m_parentDialog.IsCancelVisible = true;
                m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_AssetLibSelectDialog_Cancel", "Cancel");
                m_parentDialog.Ok += OnOk;
            }

            if (m_builtInTreeView == null)
            {
                Debug.LogError("m_builtInTreeView == null");
                return;
            }

            if (m_externalTreeView == null)
            {
                Debug.LogError("m_externalTreeView == null");
                return;
            }

            m_windowManager = IOC.Resolve<IWindowManager>();

            m_builtInTreeView.ItemDataBinding += OnItemDataBinding;
            m_builtInTreeView.ItemDoubleClick += OnItemDoubleClick;
            m_builtInTreeView.CanDrag = false;
            m_builtInTreeView.CanEdit = false;
            m_builtInTreeView.CanUnselectAll = false;

            m_externalTreeView.ItemDataBinding += OnItemDataBinding;
            m_externalTreeView.ItemDoubleClick += OnItemDoubleClick;
            m_externalTreeView.CanDrag = false;
            m_externalTreeView.CanEdit = false;
            m_externalTreeView.CanUnselectAll = false;

            m_externalTreeView.transform.parent.parent.gameObject.SetActive(false);
            m_builtInTreeView.transform.parent.parent.gameObject.SetActive(true);

            m_project = IOC.Resolve<IProjectAsync>();
            m_builtInTreeView.Items = (await m_project.GetStaticAssetLibrariesAsync()).Distinct().ToArray();
            m_builtInTreeView.SelectedIndex = 0;

            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;
            string[] assetBundles = await m_project.GetAssetBundlesAsync();
            editor.IsBusy = false;

            m_externalTreeView.Items = assetBundles;
            m_externalTreeView.SelectedIndex = 0;

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }
            
            if (m_builtInTreeView != null)
            {
                m_builtInTreeView.ItemDataBinding -= OnItemDataBinding;
                m_builtInTreeView.ItemDoubleClick -= OnItemDoubleClick;
            }

            if (m_externalTreeView != null)
            {
                m_externalTreeView.ItemDataBinding -= OnItemDataBinding;
                m_externalTreeView.ItemDoubleClick -= OnItemDoubleClick;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            string item = e.Item as string;
            if (item != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = AssetLibraryIcon;
                image.gameObject.SetActive(true);

                e.HasChildren = false;
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            if(m_parentDialog != null)
            {
                m_parentDialog.Close(true);
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if (m_builtInTreeView.SelectedItem == null && IsBuiltInLibrary || m_externalTreeView.SelectedItem == null && !IsBuiltInLibrary)
            {
                args.Cancel = true;
                return;
            }

            args.Cancel = true;
            Import(SelectedLibrary, IsBuiltInLibrary);
        }

        private void Import(string assetLibrary, bool isBuiltIn)
        {
            Transform transform = m_windowManager.CreateWindow(RuntimeWindowType.ImportAssets.ToString());
            AssetLibraryImportDialog assetLibraryImporter = transform.GetComponentInChildren<AssetLibraryImportDialog>();
            assetLibraryImporter.SelectedLibrary = assetLibrary;
            assetLibraryImporter.IsBuiltIn = isBuiltIn;
            Dialog dlg = assetLibraryImporter.GetComponentInParent<Dialog>();
            dlg.Closed += OnAssetLibraryImporterClosed;
        }

        private void OnAssetLibraryImporterClosed(Dialog sender, bool? args)
        {
            sender.Closed -= OnAssetLibraryImporterClosed;

            if(args == true)
            {
                if (m_parentDialog != null)
                {
                    m_parentDialog.Close();
                }
            }
        }
    }
}

