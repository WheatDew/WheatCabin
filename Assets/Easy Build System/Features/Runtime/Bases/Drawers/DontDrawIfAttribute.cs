/// <summary>
/// Project : Easy Build System
/// Class : DontDrawIfAttribute.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Bases.Drawers
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Bases.Drawers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DontDrawIfAttribute : PropertyAttribute
    {
        public enum DisablingType { READ_ONLY = 2, DONT_DRAW = 3 }

        public string ComparedPropertyName { get; private set; }

        public object ComparedValue { get; private set; }

        public DisablingType Disabling { get; private set; }

        public DontDrawIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.DONT_DRAW)
        {
            ComparedPropertyName = comparedPropertyName;
            ComparedValue = comparedValue;
            Disabling = disablingType;
        }
    }
}