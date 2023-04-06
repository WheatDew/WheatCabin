using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Views
{
    public class SaveFileView : View
    {
        private FileBrowser m_fileBrowser;

        [NonSerialized] 
        public UnityEvent DoubleClick = new UnityEvent();

        [NonSerialized] 
        public UnityEvent CurrentDirChanged = new UnityEvent();
        public string CurrentDir
        {
            get { return m_fileBrowser.CurrentDir; }
            set { m_fileBrowser.CurrentDir = value; }
        }

        [NonSerialized]
        public UnityEvent PathChanged = new UnityEvent();

        private string m_path;
        public string Path
        {
            get { return m_path; }
            set
            {
                m_path = value;
                //m_fileBrowser.CurrentDir = System.IO.Path.GetDirectoryName(value);
                if (m_fileBrowser.Text != value)
                {
                    m_fileBrowser.Text = value;
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
            m_path = path;
            CurrentDirChanged?.Invoke();
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

