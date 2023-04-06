using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetLibrarySelectViewModel : HierarchicalDataViewModel<AssetLibrarySelectViewModel.AssetLibrary>
    {
        [Binding]
        public class AssetLibrary
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            public AssetLibrary(string name)
            {
                Name = name;
            }
        }

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

        private bool m_showBuiltInLibraries = true;
        [Binding]
        public bool ShowBuiltInLibraries
        {
            get { return m_showBuiltInLibraries; }
            set 
            {
                if(m_showBuiltInLibraries != value)
                {
                    m_showBuiltInLibraries = value;
                    RaisePropertyChanged(nameof(ShowBuiltInLibraries));
                    ParentDialog.IsOkInteractable = SelectedItem != null;
                    BindData();
                }
            }
        }

        public override IEnumerable<AssetLibrary> SelectedItems 
        {
            get { return base.SelectedItems; }
            set
            {
                base.SelectedItems = value;
                if(ShowBuiltInLibraries)
                {
                    m_selectedBuiltInAssetLibraries = value;
                }
                else
                {
                    m_selectedExternalAssetLibraries = value;
                }

                ParentDialog.IsOkInteractable = SelectedItem != null;
            }
        }

        private IEnumerable<AssetLibrary> m_selectedBuiltInAssetLibraries;
        private IEnumerable<AssetLibrary> m_selectedExternalAssetLibraries;
        private AssetLibrary[] m_builtInAssetLibraries;
        private AssetLibrary[] m_externalAssetLibraries;

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        protected override void Start()
        {
            base.Start();

            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetLibSelectDialog_Select", "Select"),
                CancelText = Localization.GetString("ID_RTEditor_AssetLibSelectDialog_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            LoadAndBindData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_project = null;

            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }
        }

        #region Dialog Event Handlers
        protected virtual void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            args.Cancel = true;
            Import(SelectedItem, ShowBuiltInLibraries);
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(AssetLibrary item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<AssetLibrary> GetChildren(AssetLibrary parent)
        {
            if(m_showBuiltInLibraries)
            {
                return m_builtInAssetLibraries;
            }
            return m_externalAssetLibraries;
        }

        #endregion

        #region Bound Unity EventHandlers

        public override void OnItemDoubleClick()
        {
            base.OnItemDoubleClick();
            ParentDialog.Close(true);
        }

        #endregion

        #region Methods

        public override void BindData()
        {
            base.BindData();

            if (ShowBuiltInLibraries)
            {
                SelectedItems = m_selectedBuiltInAssetLibraries;
            }
            else
            {
                SelectedItems = m_selectedExternalAssetLibraries;
            }
        }

        protected virtual async void LoadAndBindData()
        {
            Editor.IsBusy = true;
            m_builtInAssetLibraries = (await m_project.GetStaticAssetLibrariesAsync()).Distinct().Select(lib => new AssetLibrary(lib)).ToArray();
            m_externalAssetLibraries = (await m_project.GetAssetBundlesAsync()).Distinct().Select(lib => new AssetLibrary(lib)).ToArray();
            Editor.IsBusy = false;

            if(m_builtInAssetLibraries.Length > 0)
            {
                m_selectedBuiltInAssetLibraries = new[] { m_builtInAssetLibraries.First() };
            }

            if(m_externalAssetLibraries.Length > 0)
            {
                m_selectedExternalAssetLibraries = new[] { m_externalAssetLibraries.First() };
            }
            
            BindData();
        }

        protected virtual void Import(AssetLibrary assetLibrary, bool isBuiltIn)
        {
            AssetLibraryImportViewModel assetLibraryImporter = WindowManager
                .CreateWindow(RuntimeWindowType.ImportAssets.ToString())
                .GetComponentInChildren<AssetLibraryImportViewModel>();
            
            assetLibraryImporter.SelectedLibrary = assetLibrary.Name;
            assetLibraryImporter.IsBuiltIn = isBuiltIn;

            assetLibraryImporter.ParentDialog.Closed += OnAssetLibraryImporterClosed;   
        }

        private void OnAssetLibraryImporterClosed(object sender, DialogViewModel.CloseEventArgs args)
        {
            if(args.Result != null)
            {
                DialogViewModel dialogViewModel = (DialogViewModel)sender;
                dialogViewModel.Closed -= OnAssetLibraryImporterClosed;

                if (args.Result == true)
                {
                    if (m_parentDialog != null)
                    {
                        m_parentDialog.Close();
                    }
                }
            }
        }

        #endregion
    }
}

