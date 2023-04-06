using Battlehub.RTCommon;
using Battlehub.UIControls;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileProjectWindowInit : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_treeViewItemPrefab = null;

        [SerializeField]
        private RectTransform m_listBoxItemPrefab = null;

        public override string WindowTypeName
        {
            get { return RuntimeWindowType.Project.ToString(); }
        }

        protected override void Extend(RuntimeWindow window)
        {
            RuntimeWindow[] windows = window.GetComponentsInChildren<RuntimeWindow>();

            EnableStyling(m_treeViewItemPrefab.gameObject);
            EnableStyling(m_listBoxItemPrefab.gameObject);

            VirtualizingScrollRect scrollRect = windows
                .Where(w => w.WindowType == RuntimeWindowType.ProjectTree)
                .First()
                .GetComponentInChildren<VirtualizingScrollRect>(true);

            scrollRect.ContainerPrefab = m_treeViewItemPrefab;

            scrollRect = windows
                .Where(w => w.WindowType == RuntimeWindowType.ProjectFolder)
                .First()
                .GetComponentInChildren<VirtualizingScrollRect>(true);

            scrollRect.ContainerPrefab = m_listBoxItemPrefab;

            
        }
    }
}
