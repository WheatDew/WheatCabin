/// <summary>
/// Project : Easy Build System
/// Class : BaseInputHandler.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
{
    public class BaseInputHandler : MonoBehaviour
    {
        #region Fields

        BuildingPlacer m_Placer;
        public BuildingPlacer Placer
        {
            get
            {
                if (m_Placer == null)
                {
                    m_Placer = BuildingPlacer.Instance;
                }

                return m_Placer;
            }
        }

        #endregion
    }
}