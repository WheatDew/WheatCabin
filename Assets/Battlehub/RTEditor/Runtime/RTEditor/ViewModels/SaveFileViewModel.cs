using Battlehub.RTCommon;
using System;
using System.IO;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor
{
    public interface ISaveFileDialog
    {
        string[] Extensions
        {
            get;
            set;
        }

        string Path
        {
            get;
            set;
        }
    }
}

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SaveFileViewModel : ViewModel, ISaveFileDialog
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
                if (m_extensions != value)
                {
                    m_extensions = value;
                    RaisePropertyChanged(nameof(Extensions));
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
                if (m_path != value)
                {
                    m_path = value;
                    UpdateVisualState();
                }
            }
        }

        private string m_currentDir;
        [Binding]
        public string CurrentDir
        {
            get { return m_currentDir; }
            set { m_currentDir = value;}
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ISaveFileDialog>(this);
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

            string path = Path;
            if(!string.IsNullOrEmpty(Path))
            {
                if (System.IO.Path.IsPathRooted(path))
                {
                    CurrentDir = System.IO.Path.GetDirectoryName(path);
                    RaisePropertyChanged(nameof(CurrentDir));
                    
                    Path = path;
                    RaisePropertyChanged(nameof(Path));
                }
                else
                {
                    string dir = CurrentDir;
                    if (!dir.EndsWith("\\") && !dir.EndsWith("/"))
                    {
                        Path = dir + System.IO.Path.DirectorySeparatorChar + path;
                    }
                    else
                    {
                        Path = dir + path;
                    }
                    RaisePropertyChanged(nameof(Path));
                }
                UpdateVisualState();
            }
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            IOC.UnregisterFallback<ISaveFileDialog>(this);

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            args.Cancel = true;

            char[] invalidPathCharacters = System.IO.Path.GetInvalidPathChars();
            if (string.IsNullOrEmpty(m_path) || m_path.Any(c => Array.IndexOf(invalidPathCharacters, c) >= 0))
            {
                return;
            }

            if (!System.IO.Path.IsPathRooted(Path) || File.Exists(Path) || Directory.Exists(System.IO.Path.GetDirectoryName(Path)) && !Directory.Exists(Path) || !Path.Contains("/") && !Path.Contains("\\"))
            {
                TrySetPath();
            }
            else
            {
                CurrentDir = Path;
                RaisePropertyChanged(nameof(CurrentDir));
            }
        }
        #endregion

        #region Bound UnityEvents

        [Binding]
        public virtual void OnFileBrowserDoubleClick()
        {
            TrySetPath();
        }

        #endregion

        #region Methods
        private void TrySetPath()
        {
            if (!System.IO.Path.IsPathRooted(Path))
            {
                string dir = CurrentDir;
                if (!dir.EndsWith("\\") && !dir.EndsWith("/"))
                {
                    Path = dir + System.IO.Path.DirectorySeparatorChar + Path;
                }
                else
                {
                    Path = dir + Path;
                }
            }

            if (File.Exists(Path))
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                if (wm != null)
                {
                    wm.Confirmation(
                        Localization.GetString("ID_RTEditor_SaveFileDialog_SaveAsConfirmationHeader"),
                        Localization.GetString("ID_RTEditor_SaveFileDialog_SaveAsConfirmation"),
                        (dialog, okArgs) =>
                        {
                            m_parentDialog.Ok -= OnOk;
                            m_parentDialog.Close(true);
                        },
                        (dialog, cancelArgs) => { });
                }
            }
            else
            {
                if (!Directory.Exists(Path))
                {
                    m_parentDialog.Ok -= OnOk;
                    m_parentDialog.Close(true);
                }
            }
        }

        private void UpdateVisualState()
        {
            char[] invalidPathCharacters = System.IO.Path.GetInvalidPathChars();
            if (string.IsNullOrEmpty(m_path) || m_path.Any(c => Array.IndexOf(invalidPathCharacters, c) >= 0)) 
            {
                m_parentDialog.OkText = Localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
                return;
            }

            if (File.Exists(m_path) || Directory.Exists(System.IO.Path.GetDirectoryName(m_path)) && !Directory.Exists(m_path) || !m_path.Contains("/") && !m_path.Contains("\\"))
            {
                m_parentDialog.OkText = Localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Save", "Save");
            }
            else
            {
                m_parentDialog.OkText = Localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
            }
        }

        #endregion
    }
}
