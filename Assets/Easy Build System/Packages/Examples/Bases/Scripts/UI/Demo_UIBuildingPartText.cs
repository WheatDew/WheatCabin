/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIBuildingPartText.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI
{
    public class Demo_UIBuildingPartText : MonoBehaviour
    {
        Text m_BuildingPartText;

        void Awake()
        {
            m_BuildingPartText = GetComponent<Text>();
        }

        void Update()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            BuildingPart buildingPart = BuildingPlacer.Instance.GetSelectedBuildingPart;

            m_BuildingPartText.horizontalOverflow = HorizontalWrapMode.Overflow;
            m_BuildingPartText.text = "Building Part : " + (buildingPart != null ? buildingPart.GetGeneralSettings.Name : "NONE");
        }
    }
}