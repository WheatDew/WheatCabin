using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class AssetLibrarySelectView : HierarchicalDataView
    {
        [SerializeField]
        private GameObject m_builtInLibrariesPanel;

        [SerializeField]
        private GameObject m_externalLibrariesPanel;
        
        public bool ShowBuiltInLibraries
        {
            get { return m_builtInLibrariesPanel.activeSelf; }
            set
            {
                m_builtInLibrariesPanel.SetActive(value);
                m_externalLibrariesPanel.SetActive(!value);
            }
        }
    }
}


