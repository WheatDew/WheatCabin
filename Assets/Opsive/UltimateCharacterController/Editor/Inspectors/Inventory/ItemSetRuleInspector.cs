/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the ItemSetManagerBase component.
    /// </summary>
    [CustomEditor(typeof(ItemSetRule), true)]
    public class ItemSetRuleInspector : UIElementsInspector
    {
        protected override void ShowElementsEnd(VisualElement container)
        {
            base.ShowElementsEnd(container);
            
            // Move the State list to the bottom of the inspector.
            var states = container.Q("ReorderableStateListAttributeControl");
            container.Add(states);
        }
    }
}