/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Demo
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Demo;
    using UnityEditor;

    /// <summary>
    /// Shows a custom inspector for the ImpactZone.
    /// </summary>
    [CustomEditor(typeof(ImpactZone), true)]
    public class ImpactZoneInspector : UIElementsInspector
    {
        protected override bool ExcludeAllFields => false;
    }
}