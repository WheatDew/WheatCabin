using Battlehub.RTCommon;
using System.IO;
using UnityWeld.Binding;

namespace Battlehub.RTEditor
{
    public interface IOpenFileDialog
    {
        string[] Extensions
        {
            get;
            set;
        }

        string Path
        {
            get;
        }

        bool SelectDirectory
        {
            get;
            set;
        }
    }
}

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class OpenFileViewModel : ViewModel, IOpenFileDialog
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

        private string[] m_extensions = new string[0];
        [Binding]
        public string[] Extensions
        {
            get { return m_extensions; }
            set
            {
                if(m_extensions != value)
                {
                    m_extensions = value;
                    RaisePropertyChanged(nameof(Extensions));
                }
            }
        }

        private bool m_selectDirectory;
        [Binding]
        public bool SelectDirectory
        {
            get { return m_selectDirectory; }
            set
            {
                if(m_selectDirectory != value)
                {
                    m_selectDirectory = value;
                    RaisePropertyChanged(nameof(SelectDirectory));
                }
            }
        }

        [Binding]
        public string Path
        {
            get;
            set;
        }

        [Binding]
        public string CurrentDir
        {
            get;
            set;
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<IOpenFileDialog>(this);
        }

        protected override void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            IOC.UnregisterFallback<IOpenFileDialog>(this);

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            args.Cancel = true;

            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            if (SelectDirectory)
            {
                if (Directory.Exists(Path))
                {
                    TrySetPath();
                }
            }
            else
            {
                if (File.Exists(Path))
                {
                    TrySetPath();
                }
                else
                {
                    RaisePropertyChanged(nameof(CurrentDir));
                }
            }
        }
        #endregion

        #region Bound UnityEvents

        [Binding]
        public virtual void OnFileBrowserDoubleClick()
        {
            if(!SelectDirectory)
            {
                TrySetPath();
            }
        }

        #endregion

        #region Methods
        private void TrySetPath()
        {
            if (!System.IO.Path.IsPathRooted(Path))
            {
                Path = CurrentDir + "\\" + Path;
            }

            if (File.Exists(Path) || SelectDirectory && Directory.Exists(Path))
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog.Close(true);
            }
        }
        #endregion
    }
}
