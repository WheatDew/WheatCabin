using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetLibraryImportViewModel : HierarchicalDataViewModel<ProjectItem>
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


        private bool m_noItemsToImport;
        [Binding]
        public bool NoItemsToImport
        {
            get { return m_noItemsToImport; }
            protected set
            {
                if (m_noItemsToImport != value)
                {
                    m_noItemsToImport = value;
                    RaisePropertyChanged(nameof(NoItemsToImport));
                }
            }
        }

        public string SelectedLibrary
        {
            protected get;
            set;
        }

        public bool IsBuiltIn
        {
            protected get;
            set;
        }

        protected ProjectItem Root
        {
            get;
            set;
        }

        protected ProjectItem[] SelectedAssets
        {
            get { return SelectedItems != null ? SelectedItems.OfType<ProjectItem>().Where(p => !p.IsFolder).ToArray() : null; }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        private IEnumerator m_coCreatePreviews;

        protected override void Start()
        {
            base.Start();
            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetLibImportDialog_Btn_Import", "Import"),
                CancelText = Localization.GetString("ID_RTEditor_AssetLibImportDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;
            m_parentDialog.Cancel += OnCancel;

            LoadAndBindData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_project = null;

            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog.Cancel -= OnCancel;
                m_parentDialog = null;
            }

            if (m_coCreatePreviews != null)
            {
                StopCoroutine(m_coCreatePreviews);
                m_coCreatePreviews = null;
            }
        }

        #region Dialog Event Handlers


        private async void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (Editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }
            if (SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            IRuntimeEditor editor = Editor;
            editor.IsBusy = true;
            await m_project.Safe.ImportAsync(SelectedAssets);
            editor.IsBusy = false;
        }


        private void OnCancel(object sender, DialogViewModel.CancelEventArgs args)
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

            if (Root != null)
            {
                m_project.UnloadImportItems(Root);
            }
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectItem item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override bool HasChildren(ProjectItem parent)
        {
            return parent.Children != null && parent.Children.Count > 0;
        }

        public override IEnumerable<ProjectItem> GetChildren(ProjectItem parent)
        {
            if (parent == null)
            {
                return new[] { Root };
            }

            return parent.Children;
        }


        #endregion

        #region Methods
        protected virtual async void LoadAndBindData()
        {
            Editor.IsBusy = true;
            Root = await m_project.Safe.LoadImportItemsAsync(SelectedLibrary, IsBuiltIn);

            if (Root != null && Root.Children != null && Root.Children.Count > 0)
            {
                BindData();
                SelectedItems = Root.Flatten(false);
                ExpandAll(Root);

                IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();

                m_coCreatePreviews = resourcePreview.CoCreatePreviews(Root.Flatten(false), () =>
                {
                    m_project.UnloadImportItems(Root);
                    Editor.IsBusy = false;
                    m_coCreatePreviews = null;
                });

                StartCoroutine(m_coCreatePreviews);
            }
            else
            {
                Editor.IsBusy = false;

                m_parentDialog.IsOkInteractable = false;

                NoItemsToImport = true;
            }

        }
        #endregion
    }
}
