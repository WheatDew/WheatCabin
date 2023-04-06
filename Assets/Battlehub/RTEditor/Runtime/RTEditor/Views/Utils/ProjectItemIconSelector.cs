using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Views
{
    public class ProjectItemIconSelector : MonoBehaviour
    {
        [SerializeField]
        private Image m_image;

        [SerializeField]
        private Sprite m_folderIcon;
        public Sprite FolderIcon
        {
            get { return m_folderIcon; }
            set { m_folderIcon = value; }
        }
        
        [SerializeField]
        private Sprite m_assetIcon;

        public Sprite AssetIcon
        {
            get { return m_assetIcon; }
            set { m_assetIcon = value; }
        }

        private ProjectItem m_projectItem;
        public ProjectItem ProjectItem
        {
            get { return m_projectItem; }
            set
            {
                if (m_projectItem != value)
                {
                    m_projectItem = value;
                    if (m_projectItem != null)
                    {
                        if (m_projectItem.IsFolder)
                        {
                            if (m_folderIcon == null)
                            {
                                ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
                                m_folderIcon = settingsComponent.SelectedTheme.GetIcon("RTEAsset_FolderSmall");
                            }
                            m_image.sprite = m_folderIcon;
                        }
                        else
                        {
                            if (m_assetIcon == null)
                            {
                                ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
                                m_assetIcon = settingsComponent.SelectedTheme.GetIcon("RTEAsset_Object");
                            }
                            m_image.sprite = m_assetIcon;
                        }
                    }
                }
            }
        }

    }
}

