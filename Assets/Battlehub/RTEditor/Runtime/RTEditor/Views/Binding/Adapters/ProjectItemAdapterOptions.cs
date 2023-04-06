using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [CreateAssetMenu(menuName = "Unity Weld/Runtime Editor/Adapter options/ProjectItem adapter options")]
    public class ProjectItemAdapterOptions : AdapterOptions
    {
        [SerializeField]
        public Sprite m_folderIcon = null;

        [SerializeField]
        private Sprite m_sceneIcon = null;

        public Sprite FolderIcon
        {
            get { return m_folderIcon; }
        }

        public Sprite SceneIncon
        {
            get { return m_sceneIcon; }
        }
    }
}
