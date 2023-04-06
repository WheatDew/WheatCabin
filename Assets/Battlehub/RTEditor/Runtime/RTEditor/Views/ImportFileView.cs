using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Views
{
    public class ImportFileView : View
    {
        private FileBrowser m_fileBrowser;

        [NonSerialized]
        public UnityEvent DoubleClick = new UnityEvent();

        [NonSerialized]
        public UnityEvent PathChanged = new UnityEvent();
        private string m_path;
        public string Path
        {
            get { return m_path; }
            set
            {
                m_path = value;
                m_fileBrowser.Open();
            }
        }

        private List<string> m_extensions = new List<string>();
        public List<string> Extensions
        {
            get
            {
                if (m_fileBrowser == null || m_fileBrowser.AllowedExt == null)
                {
                    return m_extensions;
                }

                return m_fileBrowser.AllowedExt;
            }
            set
            {
                m_extensions = value;
                if (m_fileBrowser != null)
                {
                    m_fileBrowser.AllowedExt = value;
                }
            }
        }

        private List<FileIcon> m_icons = new List<FileIcon>();
        public List<FileIcon> Icons
        {
            get
            {
                if (m_fileBrowser == null || m_fileBrowser.Icons == null)
                {
                    return m_icons;
                }

                return m_fileBrowser.Icons;
            }
            set
            {
                m_icons = value;
                if (m_fileBrowser != null)
                {
                    m_fileBrowser.Icons = value;
                }
            }
        }


        protected override void Awake()
        {
            base.Awake();
            m_fileBrowser = GetComponent<FileBrowser>();
            m_fileBrowser.DoubleClick += OnFileBrowserDoubleClick;
            m_fileBrowser.PathChanged += OnPathChanged;
            m_fileBrowser.Icons = new List<FileIcon>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_fileBrowser.DoubleClick -= OnFileBrowserDoubleClick;
            m_fileBrowser.PathChanged -= OnPathChanged;
            m_fileBrowser = null;
        }

        private void OnPathChanged(string path)
        {
            m_path = m_fileBrowser.Text;
            PathChanged?.Invoke();
        }

        private void OnFileBrowserDoubleClick(string path)
        {
            m_path = path;
            PathChanged?.Invoke();
            DoubleClick?.Invoke();
        }

    }

}
