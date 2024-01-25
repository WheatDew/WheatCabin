/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollection.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-manager/building-collection")]
    public class BuildingCollection : ScriptableObject
    {
        #region Fields

        [SerializeField] List<BuildingPart> m_BuildingParts = new List<BuildingPart>();
        public List<BuildingPart> BuildingParts { get { return m_BuildingParts; } set { m_BuildingParts = value; } }

        #endregion
    }
}