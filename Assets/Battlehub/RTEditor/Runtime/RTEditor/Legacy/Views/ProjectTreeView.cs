using System;
using UnityEngine;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class ProjectTreeView : RuntimeWindow
    {
        [SerializeField]
        public GameObject TreeViewPrefab = null;
        [SerializeField]
        public Sprite FolderIcon = null;
        [SerializeField, Obsolete]
        public Sprite ExposedFolderIcon = null;
        [HideInInspector]
        public bool ShowRootFolder = true;
        private ProjectTreeViewImpl m_impl;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ProjectTree;
            base.AwakeOverride();            

            if(TreeViewPrefab == null)
            {
                Debug.LogError("TreeViewPrefab is null");
                return;
            }
        }

        private void Start()
        {
            m_impl = GetComponent<ProjectTreeViewImpl>();
            if (!m_impl)
            {
                m_impl = gameObject.AddComponent<ProjectTreeViewImpl>();
            }

            if (!GetComponent<ProjectTreeViewInput>())
            {
                gameObject.AddComponent<ProjectTreeViewInput>();
            }
        }

        public void SelectAll()
        {
            m_impl.SelectAll();
        }

        public void DeleteSelectedItems()
        {
            m_impl.DeleteSelectedItems();
        }
    }
}