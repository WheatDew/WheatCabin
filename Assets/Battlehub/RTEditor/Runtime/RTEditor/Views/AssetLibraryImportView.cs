using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class AssetLibraryImportView : HierarchicalDataView
    {
        [SerializeField]
        private GameObject m_hierarchyPanel = null;

        [SerializeField]
        private GameObject m_noItemsPanel = null;

        public bool NoItemsToImport
        {
            get { return m_noItemsPanel.activeSelf; }
            set
            {
                m_noItemsPanel.SetActive(value);
                m_hierarchyPanel.SetActive(!value);
            }
        }

    }
}


