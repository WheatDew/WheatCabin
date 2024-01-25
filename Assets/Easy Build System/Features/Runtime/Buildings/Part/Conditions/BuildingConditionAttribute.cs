/// <summary>
/// Project : Easy Build System
/// Class : BuildingCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildingConditionAttribute : Attribute
    {
        #region Fields

        readonly string m_Name;
        public string Name { get { return m_Name; } }

        readonly string m_Description;
        public string Description { get { return m_Description; } }

        readonly int m_Order;
        public int Order { get { return m_Order; } }

        Type m_Type;
        public Type Type { get { return m_Type; } set { m_Type = value; } }

        #endregion Fields

        #region Methods

        public BuildingConditionAttribute(string name, string description, int order = 0)
        {
            m_Name = name;
            m_Description = description;
            m_Order = order;
        }

        #endregion Methods
    }
}