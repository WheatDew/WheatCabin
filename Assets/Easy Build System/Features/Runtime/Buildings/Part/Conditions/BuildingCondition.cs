/// <summary>
/// Project : Easy Build System
/// Class : BuildingCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    public class BuildingCondition : MonoBehaviour
    {
        #region Fields

        BuildingPart m_BuildingPart;
        public BuildingPart GetBuildingPart
        {
            get
            {
                if (m_BuildingPart == null)
                {
                    m_BuildingPart = GetComponent<BuildingPart>();
                }

                return m_BuildingPart;
            }
        }

        #endregion

        #region Methods

        public virtual bool CheckPlacingCondition() { return true; }

        public virtual bool CheckDestroyCondition() { return true; }

        public virtual bool CheckEditingCondition() { return true; }

        #endregion
    }
}