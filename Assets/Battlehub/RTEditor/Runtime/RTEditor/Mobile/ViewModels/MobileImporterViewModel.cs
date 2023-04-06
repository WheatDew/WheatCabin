using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class MobileImporterViewModel : ViewModel
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


        private List<string> m_extensions = new List<string>();
        [Binding]
        public List<string> Extensions
        {
            get { return m_extensions; }
            set
            {
                if (m_extensions != value)
                {
                    m_extensions = value;
                    RaisePropertyChanged(nameof(Extensions));
                }
            }
        }

        private List<FileIcon> m_icons = new List<FileIcon>();
        [Binding]
        public List<FileIcon> Icons
        {
            get { return m_icons; }
            set
            {
                if (m_icons != value)
                {
                    m_icons = value;
                    RaisePropertyChanged(nameof(Icons));
                }
            }
        }

        private string m_path;
        [Binding]
        public string Path
        {
            get { return m_path; }
            set
            {
                if(m_path != value)
                {
                    m_path = value;
                    RaisePropertyChanged(nameof(Path));
                    if(m_parentDialog != null)
                    {
                        m_parentDialog.IsOkInteractable = IsPathValid();
                    }
                }
            }
        }

        private IImporterModel m_importerModel;

        protected override void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_MobileImporterViewModel_Btn_Import", "Import"),
                CancelText = Localization.GetString("ID_RTEditor_MobileImporterViewModel_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.IsOkInteractable = IsPathValid();
            m_parentDialog.Ok += OnOk;
            m_importerModel = IOC.Resolve<IImporterModel>();

            Extensions = m_importerModel.Extensions.ToList();
            Icons = m_importerModel.Icons.Zip(Extensions, (icon, ext) => new FileIcon { Icon = icon, Ext = ext }).ToList();

        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            m_importerModel = null;
            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            args.Cancel = true;

            RaisePropertyChanged(nameof(Path));
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            TryImport();
        }
        #endregion

        #region Methods
        private bool IsPathValid()
        {
            return !string.IsNullOrEmpty(Path);
        }

        protected async void TryImport()
        {
            DialogViewModel parentDialog = m_parentDialog;
            IRTE rte = IOC.Resolve<IRTE>();
            try
            {
                rte.IsBusy = true;

                string ext = System.IO.Path.GetExtension(Path);
                if(string.IsNullOrEmpty(ext))
                {
                    ext = ".gltf";
                }


                if (parentDialog != null)
                {
                    parentDialog.Close();
                }

                await m_importerModel.ImportAsync(Path, ext);
            }
            catch(Exception)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                wm.MessageBox("Error", $"Unable to import {Path}");
                throw;
            }
            finally
            {
                rte.IsBusy = false;
            }
        }

        #endregion
    }

}
