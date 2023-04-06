using Battlehub.RTEditor.ViewModels;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Examples.Scene10
{
    /// <summary>
    /// In this example we clear standard hierarchy context menu and add My Context Menu Command
    /// </summary>
    [Binding]
    public class HierarchyViewModelWithContextMenuExample : HierarchyViewModel
    {
        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            base.OnContextMenu(menuItems);

            menuItems.Clear();
            
            MenuItemViewModel myCommand = new MenuItemViewModel { Path = "My Context Menu Command", Command = "My Command Args" };
            myCommand.Action = MenuCmd;
            myCommand.Validate = ValidateMenuCmd;
            menuItems.Add(myCommand);
        }

        private void ValidateMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems)
            {
                args.IsValid = false;
            }
        }

        private void MenuCmd(string arg)
        {
            Debug.Log($"My Context Menu Command with {arg}");
        }
    }

}
