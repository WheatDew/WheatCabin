/// <summary>
/// Project : Easy Build System
/// Class : UIBuildingMenuButton.cs
/// Namespace : EasyBuildSystem.Packages.Addons.BuildingMenu
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

namespace EasyBuildSystem.Packages.Addons.BuildingMenu
{
    public class UIBuildingMenuButton : MonoBehaviour
    {
        [SerializeField] RawImage m_UIBuildingThumbnail;
        [SerializeField] Text m_UIBuildingText;

        [SerializeField] Button m_UIBuildingButton;
        public Button UIBuildingButton { get { return m_UIBuildingButton; } }

        public void SetSlot(UIBuildingMenu.BuildingMenuCategory.Item item)
        {
            if (item.BuildingPart == null)
            {
                return;
            }

            m_UIBuildingThumbnail.texture = item.BuildingPart.GetGeneralSettings.Thumbnail;
            m_UIBuildingText.text = item.BuyPrice + "$";
        }
    }
}