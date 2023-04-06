using Battlehub.RTSL.Interface;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ProjectTreeViewOverrideExample : ProjectTreeViewImpl
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("ProjectTreeViewOverrideExample");
        }

        protected override bool CanDisplayFolder(ProjectItem projectItem)
        {
            if(projectItem.Name == "Scenes")
            {
                return false;
            }

            return base.CanDisplayFolder(projectItem);
        }
    }

}
