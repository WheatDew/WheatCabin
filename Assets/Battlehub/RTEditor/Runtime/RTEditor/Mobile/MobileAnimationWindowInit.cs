using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileAnimationWindowInit : RuntimeWindowExtension
    {
        public override string WindowTypeName
        {
            get { return BuiltInWindowNames.Animation; }
        }

        protected override void Extend(RuntimeWindow window)
        {
            bool isVerticalLayout = Screen.width <= Screen.height;

            Region region = window.GetComponentInChildren<Region>(true);
            HorizontalOrVerticalLayoutGroup layoutGroup = region.ChildrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if(layoutGroup != null)
            {
                DestroyImmediate(layoutGroup);
            }

            if (isVerticalLayout)
            {
                region.ChildrenPanel.gameObject.AddComponent<VerticalLayoutGroup>();
                foreach(Region childRegion in region.ChildrenPanel.GetComponentsInChildren<Region>(true))
                {
                    childRegion.CanResize = false;
                }

                Transform border = region.ChildrenPanel.GetChild(0).Find("VerticalBorder");
                if (border != null)
                {
                    border.gameObject.SetActive(false);
                }
            }
            else
            {
                HorizontalLayoutGroup horizontalLayoutGrup = region.ChildrenPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGrup.childControlHeight = true;
                horizontalLayoutGrup.childControlWidth = true;
                horizontalLayoutGrup.childForceExpandHeight = true;
                horizontalLayoutGrup.childForceExpandWidth = false;

                foreach (Region childRegion in region.ChildrenPanel.GetComponentsInChildren<Region>(true))
                {
                    childRegion.CanResize = true;
                }
            }
        }
    }
}
