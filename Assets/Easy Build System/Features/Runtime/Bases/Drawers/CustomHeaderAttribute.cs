/// <summary>
/// Project : Master Survival Kit
/// Class : CustomHeaderAttribute.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Bases.Drawers
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Bases.Drawers
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CustomHeaderAttribute : PropertyAttribute
    {
        public string Text { get; private set; }

        public string Description { get; private set; }
    }
}