/// <summary>
/// Project : Easy Build System
/// Class : UICircularBuildingMenuButton.cs
/// Namespace : EasyBuildSystem.Packages.Addons.CircularBuildingMenu
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EasyBuildSystem.Packages.Addons.CircularBuildingMenu
{
    public class UICircularBuildingMenuButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] RawImage m_UIIconImage;
        public RawImage UIIconImage { get { return m_UIIconImage; } }

        [SerializeField] string m_UIText;
        public string UIText { get { return m_UIText; } set { m_UIText = value; } }

        [SerializeField] string m_UIDescriptionText;
        public string UIDescriptionText { get { return m_UIDescriptionText; } set { m_UIDescriptionText = value; } }

        [SerializeField] UnityEvent m_Action;
        public UnityEvent Action { get { return m_Action; } set { m_Action = value; } }

        [SerializeField] Features.Runtime.Buildings.Part.BuildingPart m_BuildingPart;
        public Features.Runtime.Buildings.Part.BuildingPart BuildingPart { get { return m_BuildingPart; } set { m_BuildingPart = value; } }

        public void SetButton(UICircularBuildingMenu.CircularButtonSettings circularButton)
        {
            if (circularButton.Icon != null && circularButton.Icon is Texture2D)
            {
                m_UIIconImage.texture = circularButton.Icon;
            }

            m_UIText = circularButton.Name;
            m_UIDescriptionText = circularButton.Description;
            m_Action = circularButton.Action;
            m_BuildingPart = circularButton.BuildingPart;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_Action.Invoke();
        }
    }
}