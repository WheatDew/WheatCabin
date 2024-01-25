/// <summary>
/// Project : Easy Build System
/// Class : BuildingType.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using System.Collections.Generic;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager
{
    public class BuildingType : ScriptableObject
    {
        #region Fields

        public static BuildingType Instance
        {
            get
            {
                return Resources.Load<BuildingType>("Building Types");
            }
        }

        [SerializeField] List<string> m_AllBuildingTypes = new List<string>();
        public List<string> AllBuildingTypes { get { return m_AllBuildingTypes; } set { m_AllBuildingTypes = value; } }

        #endregion
    }
}