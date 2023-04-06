using Battlehub.RTSL.Interface;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ProjectFolderViewOverrideExample : ProjectFolderViewImpl
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("ProjectFolderViewOverrideExample");
        }

        protected override bool CanDisplayItem(ProjectItem projectItem)
        {
            if(projectItem.IsFolder && projectItem.Name == "Scenes")
            {
                return false;
            }

            ProjectItem parent = projectItem.Parent;
            if(parent != null)
            {
                return CanDisplayItem(parent);
            }

            return true;
        }
    }
}
