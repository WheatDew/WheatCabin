using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Battlehub.RTEditor
{
 

    public class SaveFileDialog : RuntimeWindow, ISaveFileDialog
    {
        private Dialog m_parentDialog;
        private FileBrowser m_fileBrowser;
        private ILocalization m_localization;

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
                    if(m_fileBrowser != null)
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

        private string m_path;
        public string Path
        {
            get { return m_path; }
            set { m_path = value; }
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SaveFile;
            base.AwakeOverride();
            m_localization = IOC.Resolve<ILocalization>();
            IOC.RegisterFallback<ISaveFileDialog>(this);
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
            m_fileBrowser.SelectionChanged += OnFileBrowserSelectionChanged;
            m_fileBrowser.PathChanged += OnPathChanged;
            
            if (m_extensions == null)
            {
                m_fileBrowser.AllowedExt = new List<string>(); 
            }
            else
            {
                m_fileBrowser.AllowedExt = m_extensions.ToList();
            }

            m_fileBrowser.Icons = new List<FileIcon>();

            if(System.IO.Path.IsPathRooted(Path))
            {
                m_fileBrowser.CurrentDir = System.IO.Path.GetDirectoryName(Path);
                m_fileBrowser.Text = Path;
            }
            else
            {
                m_fileBrowser.Text = m_fileBrowser.CurrentDir + "\\" + Path;
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if(m_fileBrowser != null)
            {
                m_fileBrowser.DoubleClick -= OnFileBrowserDoubleClick;
                m_fileBrowser.SelectionChanged -= OnFileBrowserSelectionChanged;
                m_fileBrowser.PathChanged -= OnPathChanged;
            }

            IOC.UnregisterFallback<ISaveFileDialog>(this);
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            args.Cancel = true;

            string path = m_fileBrowser.Save();
            if(string.IsNullOrEmpty(path))
            {    
                return;
            }

            if(!System.IO.Path.IsPathRooted(path) || File.Exists(path) || Directory.Exists(System.IO.Path.GetDirectoryName(path)) && !Directory.Exists(path) || !path.Contains("/") && !path.Contains("\\"))
            {
                TrySetPath(path);
            }
            else
            {
                m_fileBrowser.Open();
            }
        }

        private void OnPathChanged(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
                return;
            }

            if(File.Exists(path) || Directory.Exists(System.IO.Path.GetDirectoryName(path)) && !Directory.Exists(path) || !path.Contains("/") && !path.Contains("\\"))
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Save", "Save");
            }
            else
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
            }
        }

        private void OnFileBrowserSelectionChanged(string path)
        {
            if (File.Exists(path))
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Save", "Save");
            }
            else
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveFileDialog_Btn_Open", "Open");
            }
        }

        private void OnFileBrowserDoubleClick(string path)
        {
            TrySetPath(path);
        }

        private void TrySetPath(string path)
        {
            Path = path;
            if(!System.IO.Path.IsPathRooted(Path))
            {
                string dir = m_fileBrowser.CurrentDir;
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
                        m_localization.GetString("ID_RTEditor_SaveFileDialog_SaveAsConfirmationHeader"),
                        m_localization.GetString("ID_RTEditor_SaveFileDialog_SaveAsConfirmation"),
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
                if(!Directory.Exists(Path))
                {
                    m_parentDialog.Ok -= OnOk;
                    m_parentDialog.Close(true);
                }
            }
        }
    }
}
