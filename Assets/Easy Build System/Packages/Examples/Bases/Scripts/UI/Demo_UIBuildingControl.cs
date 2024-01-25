/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIBuildingControl.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI
{
    public class Demo_UIBuildingControl : MonoBehaviour
    {
        [SerializeField] Transform m_ActiveTransformByDefault;
        [SerializeField] Transform m_ActiveTransformWhenPlacingMode;
        [SerializeField] Transform m_ActiveTransformWhenEditingMode;
        [SerializeField] Transform m_ActiveTransformWhenDestroyMode;

        void Update()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }

            if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                m_ActiveTransformByDefault.gameObject.SetActive(true);
                m_ActiveTransformWhenPlacingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenEditingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenDestroyMode.gameObject.SetActive(false);
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
            {
                m_ActiveTransformByDefault.gameObject.SetActive(false);
                m_ActiveTransformWhenPlacingMode.gameObject.SetActive(true);
                m_ActiveTransformWhenEditingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenDestroyMode.gameObject.SetActive(false);
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
            {
                m_ActiveTransformByDefault.gameObject.SetActive(false);
                m_ActiveTransformWhenPlacingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenEditingMode.gameObject.SetActive(true);
                m_ActiveTransformWhenDestroyMode.gameObject.SetActive(false);
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
            {
                m_ActiveTransformByDefault.gameObject.SetActive(false);
                m_ActiveTransformWhenPlacingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenEditingMode.gameObject.SetActive(false);
                m_ActiveTransformWhenDestroyMode.gameObject.SetActive(true);
            }
        }
    }
}