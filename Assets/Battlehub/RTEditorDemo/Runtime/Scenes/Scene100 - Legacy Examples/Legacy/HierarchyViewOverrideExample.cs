using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.MenuControl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Demo
{
    public class HierarchyViewOverrideExample : HierarchyViewImpl
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnItemClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Right)
            {
                IContextMenu menu = IOC.Resolve<IContextMenu>();

                List<MenuItemInfo> menuItems = new List<MenuItemInfo>();
                MenuItemInfo command = new MenuItemInfo { Path = "Run/Cmd" };
                command.Action = new MenuItemEvent();
                command.Command = "Do Something";
                command.Action.AddListener(DoSomething);
                menuItems.Add(command);

                menu.Open(menuItems.ToArray());
            }
        }

        private void DoSomething(string cmd)
        {
            Debug.Log(cmd);
        }
    }
}

