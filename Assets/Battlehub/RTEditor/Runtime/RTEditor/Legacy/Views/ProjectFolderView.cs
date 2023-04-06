using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class ProjectFolderView : RuntimeWindow
    {
        [SerializeField]
        public GameObject ListBoxPrefab = null;

        private ProjectFolderViewImpl m_impl;
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ProjectFolder;
            base.AwakeOverride();
            
            if (!ListBoxPrefab)
            {
                Debug.LogError("Set ListBoxPrefab field");
                return;
            }
        }

        private void Start()
        {
            m_impl = GetComponent<ProjectFolderViewImpl>();
            if (!m_impl)
            {
                m_impl = gameObject.AddComponent<ProjectFolderViewImpl>();
            }

            if (!GetComponent<ProjectFolderViewInput>())
            {
                gameObject.AddComponent<ProjectFolderViewInput>();
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
