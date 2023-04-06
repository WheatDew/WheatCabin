using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class ProjectFolderViewInput : BaseViewInput<ProjectFolderView>
    {
        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            if(SelectAllAction())
            {
                View.SelectAll();
            }

            if (DeleteAction())
            {
                View.DeleteSelectedItems();
            }
        }
    }

}
