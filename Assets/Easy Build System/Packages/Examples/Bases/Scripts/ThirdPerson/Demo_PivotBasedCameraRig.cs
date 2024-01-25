/// <summary>
/// Project : Easy Build System
/// Class : Demo_PivotBasedCameraRig.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
{
    public abstract class Demo_PivotBasedCameraRig : Demo_AbstractTargetFollower
    {
        [SerializeField] Transform m_Pivot;
        public Transform Pivot { get { return m_Pivot; } }
    }
}