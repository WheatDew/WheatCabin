using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using System.Linq;

namespace Battlehub.RTEditor.Examples.Scene10
{
    /// <summary>
    /// In this example, we override the built-in project tree view model and extend the built-in context menu.
    /// ProjectFolderViewModel can be replaced in exactly the same way
    /// </summary>
    public class ExtendProjectTreeWindowExample : RuntimeWindowExtension
    {
        public override string WindowTypeName => BuiltInWindowNames.Project;

        protected override void Extend(RuntimeWindow window)
        {
            RuntimeWindow[] windows = window.GetComponentsInChildren<RuntimeWindow>();

            ViewModelBase.ReplaceWith<ProjectTreeViewModelWithContextMenuExample>(windows.First(w => w.WindowType == RuntimeWindowType.ProjectTree));
        }
    }

}
