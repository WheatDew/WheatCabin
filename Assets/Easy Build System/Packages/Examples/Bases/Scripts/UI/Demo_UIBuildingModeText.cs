/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIBuildingModeText.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI
{
    public class Demo_UIBuildingModeText : MonoBehaviour
    {
        Text m_BuildingModeText;

        void Awake()
        {
            m_BuildingModeText = GetComponent<Text>();
        }

        void Update()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            m_BuildingModeText.text = "Building Mode : " + BuildingPlacer.Instance.GetBuildMode.ToString();
        }
    }
}