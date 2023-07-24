/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using UnityEditor;

    [CustomEditor(typeof(ImpactConditionBehaviour), true)]
    public class ImpactConditionBehaviourInspector : UIElementsInspector
    {
        protected ImpactConditionBehaviour m_ImpactConditionBehaviour;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ImpactConditionBehaviour = target as ImpactConditionBehaviour;
            base.InitializeInspector();
        }
    }
}