using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Battlehub.RTEditor
{
   
    public class OpenFileDialog : RuntimeWindow, IOpenFileDialog
    {
        private Dialog m_parentDialog;
        private FileBrowser m_fileBrowser;
        private ILocalization m_localization;

        private bool m_selectDirectory;
        public bool SelectDirectory
        {
            get { return m_selectDirectory; }
            set 
            {
                m_selectDirectory = value;
                if(m_fileBrowser != null)
                {
                    m_fileBrowser.ShowFiles = !value;
                }
            }
        }

        private string[] m_extensions = new string[0];
        public string[] Extensions
        {
            get
            {
                if (m_fileBrowser == null || m_fileBrowser.AllowedExt == null)
                {
                    return m_extensions;
                }

                return m_fileBrowser.AllowedExt.ToArray();
            }
            set
            {

                if (value == null)
                {
                    m_extensions = null;
                    if (m_fileBrowser != null)
                    {
                        m_fileBrowser.AllowedExt = null;
                    }
                }
                else
                {
                    m_extensions = value;
                    if (m_fileBrowser != null)
                    {
                        m_fileBrowser.AllowedExt = value.ToList();
                    }
                }
            }
        }

        public string Path
        {
            get;
            private set;
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SaveFile;
            base.AwakeOverride();
            m_localization = IOC.Resolve<ILocalization>();
            IOC.RegisterFallback<IOpenFileDialog>(this);
        }

        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Cancel", "Cancel");
            m_parentDialog.IsCancelVisible = true;

            m_fileBrowser = GetComponent<FileBrowser>();
            m_fileBrowser.DoubleClick += OnFileBrowserDoubleClick;

            if (m_extensions == null)
            {
                m_fileBrowser.AllowedExt = new List<string>();
            }
            else
            {
                m_fileBrowser.AllowedExt = m_extensions.ToList();
            }

            m_fileBrowser.ShowFiles = !SelectDirectory;
            m_fileBrowser.Icons = new List<FileIcon>();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if (m_fileBrowser != null)
            {
                m_fileBrowser.DoubleClick -= OnFileBrowserDoubleClick;
            }

            IOC.UnregisterFallback<IOpenFileDialog>(this);
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            args.Cancel = true;

            string path = m_fileBrowser.Save();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if(SelectDirectory)
            {
                if(Directory.Exists(path))
                {
                    TrySetPath(path);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    TrySetPath(path);
                }
                else
                {
                    m_fileBrowser.Open();
                }
            }

           
        }

        private void OnFileBrowserDoubleClick(string path)
        {
            if(!SelectDirectory)
            {
                TrySetPath(path);
            }
        }

        private void TrySetPath(string path)
        {
            Path = path;
            if (!System.IO.Path.IsPathRooted(Path))
            {
                Path = m_fileBrowser.CurrentDir + "\\" + Path;
            }

            if (File.Exists(Path) || SelectDirectory && Directory.Exists(path))
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog.Close(true);
            }
        }
    }
}
