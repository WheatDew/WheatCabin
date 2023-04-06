
using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileHierarchyWindowInit : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_treeViewItemPrefab = null;

        public override string WindowTypeName
        {
            get { return RuntimeWindowType.Hierarchy.ToString(); }
        }

        protected override void Extend(RuntimeWindow window)
        {
            EnableStyling(m_treeViewItemPrefab.gameObject);

            VirtualizingScrollRect scrollRect = window.GetComponentInChildren<VirtualizingScrollRect>(true);
            scrollRect.ContainerPrefab = m_treeViewItemPrefab;
            
        }
    }
}
