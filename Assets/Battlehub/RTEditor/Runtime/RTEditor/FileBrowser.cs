using UnityEngine;
using UnityEngine.UI;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using TMPro;

namespace Battlehub.RTEditor
{
    [Serializable]
    public class FileIcon
    {
        public string Ext;
        public Sprite Icon;
    }

    public enum FsEntryType
    {
        Up,
        Directory,
        File
    }

    public class FsEntry
    {
        public string Path;
        public string Name;
        public string Ext;
        public Sprite Icon;
        public FsEntryType EntryType;
    }

    [DefaultExecutionOrder(-60)]
    public class FileBrowser : MonoBehaviour
    {
        public event Action<string> DoubleClick;
        public event Action<string> SelectionChanged;
        public event Action<string> PathChanged;

        [SerializeField]
        private TMP_Dropdown Drives = null;

        [SerializeField]
        private TMP_InputField Input = null;

        [SerializeField]
        private List<FileIcon> m_icons = null;
        public List<FileIcon> Icons
        {
            get { return m_icons; }
            set
            {
                m_icons = value;
                ExtToIcon();
                if (!string.IsNullOrEmpty(m_currentDir))
                {
                    BindDataItems(m_currentDir);
                }
            }
        }

        [SerializeField]
        private Sprite FolderIcon = null;

        [SerializeField]
        private Sprite FileIcon = null;

        [SerializeField]
        private Sprite EmptyIcon = null;

        private VirtualizingTreeView m_treeView = null;
        private IWindowManager m_windowManager;

        private readonly Dictionary<string, Sprite> m_extToIcon = new Dictionary<string, Sprite>();
        private string m_currentDir;
        public string CurrentDir
        {
            get { return m_currentDir; }
            set
            {
                if(m_currentDir == value)
                {
                    return;
                }

                if (Directory.Exists(value))
                {
                    string oldDir = m_currentDir;
                    try
                    {
                        m_currentDir = NormalizePath(value);
                        BindDataItems(m_currentDir);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        m_windowManager.MessageBox(Path.GetFileName(value), "You Don’t Currently Have Permission to Access this Folder");
                        m_currentDir = oldDir;
                        BindDataItems(m_currentDir);
                    }
                    catch (Exception e)
                    {
                        m_windowManager.MessageBox("Unable to open folder", e.ToString());
                        m_currentDir = oldDir;
                        BindDataItems(m_currentDir);
                    }

                    if (m_started)
                    {
                        Drives.onValueChanged.RemoveListener(OnDriveChanged);
                        FileInfo f = new FileInfo(m_currentDir);
                        string currentDrive = Path.GetPathRoot(f.FullName);
                        Drives.value = Array.IndexOf(Drives.options.Select(o => o.text).ToArray(), currentDrive);
                        Drives.onValueChanged.AddListener(OnDriveChanged);
                    }


                    PlayerPrefs.SetString("Battlehub.FileBrowser.CurrentDir", m_currentDir);
                }
            }
        }

        public string Text
        {
            get { return Input.text; }
            set { Input.text = value; }
        }

        private List<string> m_allowedExt;
        private HashSet<string> m_allowedExtHs;
        public List<string> AllowedExt
        {
            get { return m_allowedExt; }
            set
            {
                m_allowedExt = value;
                if (value == null)
                {
                    m_allowedExtHs = null;
                }
                else
                {
                    m_allowedExtHs = new HashSet<string>(m_allowedExt.Select(ext => ext.ToLower()).Distinct());
                }

                if(!string.IsNullOrEmpty(m_currentDir))
                {
                    m_currentDir = NormalizePath(m_currentDir);
                    BindDataItems(m_currentDir);
                }
            }
        }

        [SerializeField]
        private bool m_showFiles = true;
        public bool ShowFiles
        {
            get { return m_showFiles; }
            set { m_showFiles = value; }
        }


        private bool m_started = false;
        private void Awake()
        {
            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
        }

        private void Start()
        {
            ExtToIcon();

            m_windowManager = IOC.Resolve<IWindowManager>();
            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;
            m_treeView.CanRemove = false;

            if (m_allowedExt != null)
            {
                m_allowedExtHs = new HashSet<string>(m_allowedExt.Distinct());
            }

            if (string.IsNullOrEmpty(CurrentDir))
            {
                CurrentDir = PlayerPrefs.GetString("Battlehub.FileBrowser.CurrentDir");
            }

            if (string.IsNullOrEmpty(CurrentDir))
            {
                CurrentDir = NormalizePath(Application.persistentDataPath);
            }
            else
            {
                m_currentDir = NormalizePath(m_currentDir);
                BindDataItems(m_currentDir);
            }

            FileInfo f = new FileInfo(m_currentDir);
            string currentDrive = Path.GetPathRoot(f.FullName);

            DriveInfo[] drives = DriveInfo.GetDrives();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            int driveIndex = 0;
            for (int i = 0; i < drives.Length; ++i)
            {
                DriveInfo drive = drives[i];
                if (!drive.IsReady)
                {
                    continue;
                }

                if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable)
                {
                    continue;
                }

                options.Add(new TMP_Dropdown.OptionData(drive.VolumeLabel));
                if (currentDrive == drive.VolumeLabel)
                {
                    driveIndex = i;
                }

            }

            Drives.onValueChanged.RemoveListener(OnDriveChanged);
            Drives.options = options;
            Drives.value = driveIndex;
            Drives.onValueChanged.AddListener(OnDriveChanged);

            Input.onValueChanged.AddListener(OnPathChanged);
            Input.onSubmit.AddListener(OnSubmit);

            m_started = true;
        }

        private void ExtToIcon()
        {
            m_extToIcon.Clear();
            for (int i = 0; i < m_icons.Count; ++i)
            {
                FileIcon fileIcon = m_icons[i];
                if (fileIcon == null && fileIcon.Icon != null)
                {
                    continue;
                }

                if (!m_extToIcon.ContainsKey(fileIcon.Ext.ToLower()))
                {
                    m_extToIcon.Add(fileIcon.Ext.ToLower(), fileIcon.Icon);
                }
            }
        }

        private void BindDataItems(string dir)
        {
            List<FsEntry> content = GetDirectoryContent(dir);
            m_treeView.Items = content;
            if (content.Count > 0)
            {
                m_treeView.SelectedItem = content[0];
            }
        }

        private void OnDestroy()
        {
            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
            }

            if (Drives != null)
            {
                Drives.onValueChanged.RemoveListener(OnDriveChanged);
            }

            if (Input != null)
            {
                Input.onValueChanged.RemoveListener(OnPathChanged);
                Input.onSubmit.RemoveListener(OnSubmit);
            }
        }

        private void OnDriveChanged(int value)
        {
            CurrentDir = Drives.options[value].text;
        }

        private void OnPathChanged(string value)
        {
            if (PathChanged != null)
            {
                PathChanged(value);
            }
        }

        private void OnSubmit(string value)
        {
            CurrentDir = value;
        }

        public string Save()
        {
            return Input.text;
        }

        public string Open()
        {
            if (m_treeView.SelectedItem == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(Input.text))
            {
                Input.ActivateInputField();
                return null;
            }

            FsEntry fsEntry = m_treeView.SelectedItem as FsEntry;
            if (fsEntry != null && fsEntry.EntryType == FsEntryType.Up)
            {
                if (Input.text == CurrentDir && Directory.Exists(Input.text))
                {
                    Input.text = Directory.GetParent(Input.text).ToString();
                }
            }

            if (Directory.Exists(Input.text))
            {
                CurrentDir = Input.text;
                return CurrentDir;
            }
            else if (File.Exists(Input.text))
            {
                return Input.text;
            }

            Input.text = CurrentDir;
            return null;
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            FsEntry item = e.Item as FsEntry;
            if (item != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item.Name;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = item.Icon;
                image.gameObject.SetActive(true);

                e.HasChildren = false;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if (e.NewItem != null)
            {
                FsEntry entry = (FsEntry)e.NewItem;
                switch (entry.EntryType)
                {
                    case FsEntryType.Up:
                        Input.text = CurrentDir;
                        break;
                    case FsEntryType.Directory:
                        Input.text = entry.Path;
                        break;
                    case FsEntryType.File:
                        Input.text = entry.Path;
                        break;
                }
            }

            if (SelectionChanged != null)
            {
                FsEntry entry = (FsEntry)e.NewItem;
                if (entry != null)
                {
                    SelectionChanged(entry.Path);
                }
                else
                {
                    SelectionChanged(null);
                }

            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs args)
        {
            FsEntry entry = (FsEntry)args.Items[0];
            string currentDir = CurrentDir;
            currentDir = NormalizePath(currentDir);

            switch (entry.EntryType)
            {
                case FsEntryType.Up:
                    currentDir = Directory.GetParent(CurrentDir).ToString();
                    break;
                case FsEntryType.Directory:
                    currentDir = entry.Path;
                    break;
            }

            CurrentDir = currentDir;

            if (entry.EntryType != FsEntryType.Up)
            {
                if (DoubleClick != null)
                {
                    DoubleClick(entry.Path);
                }
            }
        }

        public static string NormalizePath(string path)
        {
            path = NormalizeDriveLetter(path);
            path = Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            path = NormalizeDriveLetter(path);
            return path;
        }

        public static string NormalizeDriveLetter(string path)
        {
            int colonIndex = path.IndexOf(':');
            if (colonIndex > 0 && colonIndex == path.Length - 1)
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        private List<FsEntry> GetDirectoryContent(string currentDir)
        {
            List<FsEntry> content = new List<FsEntry>();

            DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
            DirectoryInfo parent = dirInfo.Parent;
            if (parent != null)
            {
                FsEntry up = new FsEntry();
                up.EntryType = FsEntryType.Up;
                up.Name = "...";
                up.Icon = EmptyIcon;
                content.Add(up);
            }

            string[] directories = Directory.GetDirectories(currentDir).OrderBy(d => d).ToArray();
            string[] files = m_showFiles ? Directory.GetFiles(currentDir).OrderBy(f => f).ToArray() : new string[0];
            for (int i = 0; i < directories.Length; ++i)
            {
                string dir = NormalizePath(directories[i]);

                FsEntry fsEntry = new FsEntry();
                fsEntry.EntryType = FsEntryType.Directory;
                fsEntry.Name = Path.GetFileName(dir);
                fsEntry.Icon = FolderIcon;
                fsEntry.Path = dir;

                content.Add(fsEntry);
            }
  
            for (int i = 0; i < files.Length; ++i)
            {
                string file = NormalizePath(files[i]);
                string ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext))
                {
                    ext = ext.ToLower();
                }
                if (m_allowedExtHs != null && m_allowedExtHs.Count > 0 && !m_allowedExtHs.Contains(ext))
                {
                    continue;
                }

                FsEntry fsEntry = new FsEntry();
                fsEntry.EntryType = FsEntryType.File;
                fsEntry.Name = Path.GetFileNameWithoutExtension(file);
                fsEntry.Ext = ext;

                Sprite icon;
                if (!m_extToIcon.TryGetValue(fsEntry.Ext, out icon))
                {
                    icon = FileIcon;
                }
                else
                {
                    if (icon == null)
                    {
                        icon = FileIcon;
                    }
                }
                fsEntry.Icon = icon;
                fsEntry.Path = file;

                content.Add(fsEntry);
            }

            return content;
        }
    }
}

