using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor.Examples.Scene10
{
    /// <summary>
    /// In this example, we override the built-in hierarchy view model and extend the built-in context menu
    /// </summary>
    public class ExtendHierarchyWindowExample : RuntimeWindowExtension
    {
        public override string WindowTypeName => BuiltInWindowNames.Hierarchy;

        protected override void Extend(RuntimeWindow window)
        {
            ViewModelBase.ReplaceWith<HierarchyViewModelWithContextMenuExample>(window);
        }
    }
}

