using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class ImportFileViewModel : ViewModel
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
                if(m_icons != value)
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
                    if (File.Exists(m_path))
                    {
                        m_parentDialog.OkText = Localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Import", "Import");
                    }
                    else
                    {
                        m_parentDialog.OkText = Localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Open", "Open");
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
                OkText = Localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Open", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

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

            if (!File.Exists(Path))
            {
                return;
            }

            TryImport();
        }
        #endregion

        #region Bound UnityEvents

        [Binding]
        public virtual void OnFileBrowserDoubleClick()
        {
            if (File.Exists(Path))
            {
                TryImport();
            }
        }

        #endregion

        #region Methods
        protected async void TryImport()
        {
            DialogViewModel parentDialog = m_parentDialog;
            IRTE rte = IOC.Resolve<IRTE>();
           
            rte.IsBusy = true;
            await m_importerModel.ImportAsync(Path, System.IO.Path.GetExtension(Path));
            rte.IsBusy = false;

            if(parentDialog != null)
            {
                parentDialog.Close();
            }
        }

        #endregion
    }
}
