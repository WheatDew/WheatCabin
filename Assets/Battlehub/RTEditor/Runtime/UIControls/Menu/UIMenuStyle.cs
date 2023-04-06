using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.UIControls
{
    public class UIMenuStyle : UIStyle 
    {
        public static void ApplyMainButtonColor(UIStyle uiStyle, Color normal, Color pointerOver, Color focused)
        {
            MainMenuButton mainMenuButton = uiStyle.GetComponent<MainMenuButton>();
            if (mainMenuButton != null)
            {
                mainMenuButton.NormalColor = normal;
                mainMenuButton.PointerOverColor = pointerOver;
                mainMenuButton.FocusedColor = focused;
            }
        }

        public static void ApplyMenuItemColor(UIStyle uiStyle, Color selectionColor, Color textColor, Color disabledSelectionColor, Color disabledTextColor)
        {
            MenuItem menuItem = uiStyle.GetComponent<MenuItem>();
            if(menuItem != null)
            {
                menuItem.SelectionColor = selectionColor;
                menuItem.TextColor = textColor;
                menuItem.DisabledSelectionColor = disabledSelectionColor;
                menuItem.DisableTextColor = disabledTextColor;
            }
        }
    }
}