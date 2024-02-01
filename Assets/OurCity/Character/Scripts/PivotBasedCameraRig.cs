/// <summary>
/// Project : Easy Build System
/// Class : Demo_PivotBasedCameraRig.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace OurCity
{
    public abstract class PivotBasedCameraRig : AbstractTargetFollower
    {
        [SerializeField] Transform m_Pivot;
        public Transform Pivot { get { return m_Pivot; } }
    }
}